using System.Collections.Generic;
using LiftStudio.EventChannels;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiftStudio
{
    public class CharacterMovementController : MonoBehaviourPun, IPunInstantiateMagicCallback, IPunObservable
    {
        [SerializeField] private LayerMask characterLayerMask;
        [SerializeField] private LayerMask wallLayerMask;
        [SerializeField] private float characterFloatHeight = 0.5f;
        [SerializeField] private Texture2D holdCursor;
        [SerializeField] private Vector2Int cursorOffset;
        [SerializeField] private float characterMoveSpeed;

        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;
        [SerializeField] private AllMovableGridCellsSetEventChannel allMovableGridCellsSetEventChannel;

        public MovementCardSettings MovementCardSettings { get; private set; }

        private Camera _gameCamera;
        private Transform _tempCharacter;
        private TilePlacer _tilePlacer;
        private Game _gameHandler;
        private Timer _timer;

        private Vector3 _mouseStartPosition;
        private Vector3 _placementMouseStartPosition;
        private Character _selectedCharacter;
        private GridCell _startGridCell;
        private GridCell _targetGridCell;
        private Vector3 _additionalFloatPosition;

        private Plane _plane = new Plane(Vector3.up, Vector3.zero);

        private Vector3 _otherCharacterTargetPosition;
        private readonly Queue<Vector3> _otherCharacterPositions = new Queue<Vector3>();

        private static GameSetup GameSetupInstance => GameSetup.Instance;
        private static CharacterMovementController _localPlayerController;

        private Vector3 SelectedCharacterPosition
        {
            get => _selectedCharacter.transform.position;
            set => _selectedCharacter.transform.position = value;
        }

        private readonly List<GridCell> _allMovableGridCells = new List<GridCell>();

        private void Awake()
        {
            _additionalFloatPosition = new Vector3(0, characterFloatHeight, 0);
            gameEndedEventChannel.GameEnded += OnGameEnded;
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            if (photonView.IsMine)
            {
                _localPlayerController = this;
            }

            var data = info.photonView.InstantiationData;
            var cardSetupIndex = (int)data[0];
            var cardIndex = (int)data[1];
            var setupData = GameSetupInstance.GetCharacterMovementControllerSetupData(cardSetupIndex, cardIndex);
            _gameCamera = setupData.GameCamera;
            MovementCardSettings = setupData.MovementCardSettings;
            _tempCharacter = setupData.TempCharacter;
            _tilePlacer = setupData.TilePlacer;
            _gameHandler = setupData.GameHandler;
            _timer = setupData.Timer;
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (_selectedCharacter == null)
                {
                    HandleSelectingCharacter();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_selectedCharacter != null && _targetGridCell != null)
                {
                    HandlePlacingSelectedCharacter();
                }
            }
        }

        private void LateUpdate()
        {
            if (!_selectedCharacter) return;
            if (!photonView.IsMine)
            {
                if (!_gameHandler.CharactersMoving[_selectedCharacter.Type]) return;
                if (SelectedCharacterPosition == _otherCharacterTargetPosition && _otherCharacterPositions.Count > 0)
                {
                    _otherCharacterTargetPosition = _otherCharacterPositions.Dequeue();
                }

                SelectedCharacterPosition = Vector3.MoveTowards(SelectedCharacterPosition, _otherCharacterTargetPosition, characterMoveSpeed * Time.deltaTime);
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (!Input.GetMouseButton(0)) return;

            var ray = _gameCamera.ScreenPointToRay(Input.mousePosition);
            _plane.Raycast(ray, out var enter);
            var planeHitPoint = ray.GetPoint(enter);
            var mouseMoveDirection = planeHitPoint - _mouseStartPosition;
            var placementMouseMoveDirection = planeHitPoint - _placementMouseStartPosition;
            var targetPlacementPosition = _targetGridCell.CenterWorldPosition + MathUtils.FloorOrCeilToIntVector3(placementMouseMoveDirection);
            var targetPosition = _startGridCell.CenterWorldPosition + mouseMoveDirection;

            foreach (var placedTile in _tilePlacer.AllPlacedTiles)
            {
                var targetGridCell = placedTile.Grid.GetGridCellObject(targetPlacementPosition);
                if (targetGridCell == null) continue;
                if (targetGridCell.CharacterOnTop && targetGridCell != _startGridCell) return;
                if (targetGridCell.Exits != null && !_gameHandler.HasCharactersBeenOnPickupCells) return;

                SelectedCharacterPosition = targetPosition + _additionalFloatPosition;
                _placementMouseStartPosition += targetGridCell.CenterWorldPosition - _targetGridCell.CenterWorldPosition;
                _targetGridCell = targetGridCell;
                _tempCharacter.position = targetGridCell.CenterWorldPosition;
                break;
            }
        }

        private void HandleSelectingCharacter()
        {
            var ray = _gameCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var characterHitInfo, Mathf.Infinity, characterLayerMask)) return;

            var selectedCharacter = characterHitInfo.transform.GetComponent<Character>();
            if (_gameHandler.CharactersMoving[selectedCharacter.Type]) return;

            _plane.Raycast(ray, out var enter);
            var mouseStartPosition = ray.GetPoint(enter);
            _mouseStartPosition = mouseStartPosition;
            _placementMouseStartPosition = mouseStartPosition;
            _selectedCharacter = selectedCharacter;
            _tempCharacter.gameObject.SetActive(true);
            _gameHandler.CharactersMoving[selectedCharacter.Type] = true;
            var boardTile = _gameHandler.CharacterOnTileDictionary[selectedCharacter];
            _startGridCell = boardTile.Grid.GetGridCellObject(SelectedCharacterPosition);
            _targetGridCell = _startGridCell;
            TryGetAllPossibleTargetGridCells();
            SelectedCharacterPosition += _additionalFloatPosition;
            Cursor.SetCursor(holdCursor, cursorOffset, CursorMode.Auto);
            var content = new object[] { _selectedCharacter.Type };
            photonView.RPC("SelectCharacterRPC", RpcTarget.Others, content);
        }

        private void HandlePlacingSelectedCharacter()
        {
            if (float.IsNegativeInfinity(_targetGridCell.CenterWorldPosition.x)) return;

            if (!_allMovableGridCells.Contains(_targetGridCell))
            {
                MoveCharacterToTargetPosition(_startGridCell);
                return;
            }

            if (_targetGridCell.Exits != null &&
                _targetGridCell.Exits.Exists(exit => exit.targetCharacterType == _selectedCharacter.Type))
            {
                TakeCharacterOutOfBoard(_selectedCharacter);
                return;
            }

            if (_targetGridCell.Pickup != null &&
                _targetGridCell.Pickup.TargetCharacterType == _selectedCharacter.Type)
            {
                _gameHandler.NotifyCharacterPlacedOnPickupCell(_selectedCharacter.Type, _tempCharacter);
            }
            else if (_targetGridCell.Hourglass is { isAvailable: true })
            {
                var content = new object[]
                {
                    _tilePlacer.AllPlacedTiles.IndexOf(_targetGridCell.Tile),
                    _targetGridCell.CenterWorldPosition, _timer.CurrentTime
                };
                photonView.RPC("FlipHourglassRPC", RpcTarget.Others, content);
                _targetGridCell.UseHourglass();
                _timer.FlipHourglassTimer();
            }

            MoveCharacterToTargetPosition(_targetGridCell);
        }

        private void MoveCharacterToTargetPosition(GridCell targetGridCell)
        {
            _allMovableGridCells.Clear();
            allMovableGridCellsSetEventChannel.RaiseEvent(_allMovableGridCells);
            var targetTileIndex = _tilePlacer.AllPlacedTiles.IndexOf(targetGridCell.Tile);
            var startTileIndex = _tilePlacer.AllPlacedTiles.IndexOf(_startGridCell.Tile);
            var content = new object[]
            {
                _selectedCharacter.Type, targetTileIndex, targetGridCell.CenterWorldPosition, startTileIndex,
                _startGridCell.CenterWorldPosition, PhotonNetwork.LocalPlayer.UserId
            };
            photonView.RPC("TryPlaceCharacterRPC", RpcTarget.MasterClient, content);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }

        private void TakeCharacterOutOfBoard(Character targetCharacter)
        {
            var eventContent = new object[] { targetCharacter.Type, _startGridCell.CenterWorldPosition };
            _startGridCell.ClearCharacter();
            _allMovableGridCells.Clear();
            allMovableGridCellsSetEventChannel.RaiseEvent(_allMovableGridCells);
            _tempCharacter.gameObject.SetActive(false);
            _startGridCell = null;
            _selectedCharacter = null;
            _targetGridCell = null;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);

            photonView.RPC("TakeCharacterOutOfBoardRPC", RpcTarget.All, eventContent);
        }

        private void TryGetAllPossibleTargetGridCells()
        {
            if (_selectedCharacter == null) return;

            _allMovableGridCells.Clear();
            _allMovableGridCells.Add(_startGridCell);
            var possibleMovementDirection = MovementCardSettings.GetAllPossibleMovementVector();
            var sortedPlacedTiles = new List<Tile>(_tilePlacer.AllPlacedTiles);
            sortedPlacedTiles.Sort((tile, nextTile) =>
            {
                if (tile == _gameHandler.CharacterOnTileDictionary[_selectedCharacter]) return -1;

                return 0;
            });
            foreach (var movementDirection in possibleMovementDirection)
            {
                var startGridCell = _startGridCell;
                var nextGridCellPosition = startGridCell.CenterWorldPosition + movementDirection;
                foreach (var placedTile in sortedPlacedTiles)
                {
                    if (MovementCardSettings.canUsePortal)
                    {
                        var portalGridCell =
                            placedTile.GetTargetCharacterPortalGridCell(_selectedCharacter.Type);
                        if (portalGridCell != null && portalGridCell.CharacterOnTop == null &&
                            !_gameHandler.HasCharactersBeenOnPickupCells &&
                            !_allMovableGridCells.Contains(portalGridCell))
                        {
                            _allMovableGridCells.Add(portalGridCell);
                        }
                    }

                    if (MovementCardSettings.canUseElevator)
                    {
                        if (_startGridCell.Elevator != null &&
                            placedTile == _gameHandler.CharacterOnTileDictionary[_selectedCharacter])
                        {
                            var otherEndElevatorGridCell = placedTile.GetOtherElevatorEndGridCell(_startGridCell);
                            if (otherEndElevatorGridCell != null &&
                                !_allMovableGridCells.Contains(otherEndElevatorGridCell))
                            {
                                _allMovableGridCells.Add(otherEndElevatorGridCell);
                            }
                        }
                    }

                    do
                    {
                        var nextGridCell = placedTile.Grid.GetGridCellObject(nextGridCellPosition);
                        if (nextGridCell == null) break;

                        if (!Physics.Raycast(startGridCell.CenterWorldPosition, movementDirection, TilePlacer.CellSize,
                                wallLayerMask) && nextGridCell.CharacterOnTop == null)
                        {
                            _allMovableGridCells.Add(nextGridCell);
                            startGridCell = nextGridCell;
                            nextGridCellPosition = startGridCell.CenterWorldPosition + movementDirection;
                            continue;
                        }

                        break;
                    } while (true);
                }
            }

            allMovableGridCellsSetEventChannel.RaiseEvent(_allMovableGridCells);
        }

        private void ConfirmPlaceCharacter(string senderUserId, CharacterType placedCharacterType,
            GridCell targetGridCell)
        {
            if (_selectedCharacter == null) return;
            if (_selectedCharacter.Type != placedCharacterType ||
                senderUserId != PhotonNetwork.LocalPlayer.UserId)
            {
                TryGetAllPossibleTargetGridCells();
                return;
            }

            _tempCharacter.gameObject.SetActive(false);
            _startGridCell = targetGridCell;
            _selectedCharacter = null;
            _targetGridCell = null;
            _allMovableGridCells.Clear();
            allMovableGridCellsSetEventChannel.RaiseEvent(_allMovableGridCells);
        }

        [PunRPC]
        private void SelectCharacterRPC(CharacterType targetCharacterType)
        {
            _gameHandler.CharactersMoving[targetCharacterType] = true;
            var targetCharacter = _gameHandler.CharacterFromTypeDictionary[targetCharacterType];
            var targetCharacterTransform = targetCharacter.transform;
            var selectedCharacterPosition = targetCharacterTransform.position;
            selectedCharacterPosition += _additionalFloatPosition;
            targetCharacterTransform.position = selectedCharacterPosition;
        }

        [PunRPC]
        private void TryPlaceCharacterRPC(object[] data)
        {
            var targetCharacterType = (CharacterType)data[0];
            if (!_gameHandler.CharactersMoving[targetCharacterType]) return;

            var targetTileIndex = (int)data[1];
            var targetTilePosition = (Vector3)data[2];

            var targetCharacter = _gameHandler.CharacterFromTypeDictionary[targetCharacterType];
            var targetTile = _tilePlacer.AllPlacedTiles[targetTileIndex];
            var targetGridCell = targetTile.Grid.GetGridCellObject(targetTilePosition);
            if (targetGridCell.CharacterOnTop && targetGridCell.CharacterOnTop != targetCharacter) return;

            photonView.RPC("ConfirmPlaceCharacterRPC", RpcTarget.All, data);
        }

        [PunRPC]
        private void ConfirmPlaceCharacterRPC(object[] data)
        {
            var targetCharacterType = (CharacterType)data[0];
            if (!_gameHandler.CharactersMoving[targetCharacterType]) return;

            var targetCharacter = _gameHandler.CharacterFromTypeDictionary[targetCharacterType];
            var targetTile = _tilePlacer.AllPlacedTiles[(int)data[1]];
            var targetGridCell = targetTile.Grid.GetGridCellObject((Vector3)data[2]);
            var startTile = _tilePlacer.AllPlacedTiles[(int)data[3]];
            var startGridCell = startTile.Grid.GetGridCellObject((Vector3)data[4]);
            var senderUserId = (string)data[5];

            _gameHandler.CharactersMoving[targetCharacterType] = false;
            startGridCell.ClearCharacter();
            targetGridCell.SetCharacter(targetCharacter);
            _gameHandler.CharacterOnTileDictionary[targetCharacter] = targetGridCell.Tile;
            targetCharacter.transform.position = targetGridCell.CenterWorldPosition;
            _localPlayerController.ConfirmPlaceCharacter(senderUserId, targetCharacterType, targetGridCell);
            _selectedCharacter = null;
        }

        [PunRPC]
        private void TakeCharacterOutOfBoardRPC(CharacterType targetCharacterType, Vector3 gridCellPosition)
        {
            var targetCharacter = _gameHandler.CharacterFromTypeDictionary[targetCharacterType];
            var startTargetCharacterTile = _gameHandler.CharacterOnTileDictionary[targetCharacter];
            var targetCharacterInitialGridCell =
                startTargetCharacterTile.Grid.GetGridCellObject(gridCellPosition);
            targetCharacterInitialGridCell.ClearCharacter();
            targetCharacter.transform.position = _gameHandler.OutOfBoardTransform.position;
            targetCharacter.ToggleColliderOff();
            _gameHandler.CharactersMoving[targetCharacterType] = false;
            _localPlayerController.TryGetAllPossibleTargetGridCells();
            _gameHandler.NotifyTakeCharacterOutOfBoard(targetCharacter);
        }

        [PunRPC]
        private void FlipHourglassRPC(int tileIndex, Vector3 gridCellPosition, float senderTime)
        {
            var targetTile = _tilePlacer.AllPlacedTiles[tileIndex];
            var targetGridCell = targetTile.Grid.GetGridCellObject(gridCellPosition);
            targetGridCell.UseHourglass();
            _timer.FlipHourglassTimer(senderTime);
        }

        private void OnGameEnded()
        {
            enabled = false;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (!_selectedCharacter) return;
                var characterType = _selectedCharacter.Type;
                stream.SendNext(characterType);
                stream.SendNext(_selectedCharacter.transform.position);
                return;
            }

            var targetCharacterType = (CharacterType)stream.ReceiveNext();
            var targetCharacterPosition = (Vector3)stream.ReceiveNext();
            if (!_gameHandler.CharactersMoving[targetCharacterType]) return;

            _selectedCharacter = _gameHandler.CharacterFromTypeDictionary[targetCharacterType];
            _otherCharacterPositions.Enqueue(targetCharacterPosition);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void OnDestroy()
        {
            gameEndedEventChannel.GameEnded -= OnGameEnded;
            _localPlayerController = null;
        }
    }
}