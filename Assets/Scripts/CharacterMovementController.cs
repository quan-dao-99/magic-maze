using System.Collections.Generic;
using ExitGames.Client.Photon;
using LiftStudio.EventChannels;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiftStudio
{
    public class CharacterMovementController : MonoBehaviour, IOnEventCallback
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private LayerMask characterLayerMask;
        [SerializeField] private LayerMask wallLayerMask;
        [SerializeField] private MovementCardSettings movementCardSettings;
        [SerializeField] private Transform tempCharacter;
        [SerializeField] private float characterFloatHeight = 0.5f;
        [SerializeField] private Texture2D holdCursor;
        [SerializeField] private Vector2Int cursorOffset;

        [Space]
        [SerializeField] private TilePlacer tilePlacer;
        [SerializeField] private Game gameHandler;
        [SerializeField] private Timer timer;

        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;

        private Vector3 _mouseStartPosition;
        private Character _selectedCharacter;
        private GridCell _startGridCell;
        private GridCell _targetGridCell;
        private Vector3 _additionalFloatPosition;

        private Plane _plane = new Plane(Vector3.up, Vector3.zero);

        private Vector3 SelectedCharacterPosition
        {
            get => _selectedCharacter.transform.position;
            set => _selectedCharacter.transform.position = value;
        }

        public Dictionary<CharacterType, bool> CharactersMoving { get; } = new Dictionary<CharacterType, bool>
        {
            {CharacterType.Axe, false}, {CharacterType.Bow, false},
            {CharacterType.Potion, false}, {CharacterType.Sword, false}
        };

        private void Awake()
        {
            _additionalFloatPosition = new Vector3(0, characterFloatHeight, 0);
            gameEndedEventChannel.GameEnded += OnGameEnded;
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Update()
        {
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
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (!_selectedCharacter) return;

            if (!Input.GetMouseButton(0)) return;

            var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            _plane.Raycast(ray, out var enter);
            var planeHitPoint = ray.GetPoint(enter);
            var mouseMoveDirection = planeHitPoint - _mouseStartPosition;
            var targetPosition = _startGridCell.CenterWorldPosition + mouseMoveDirection;

            foreach (var placedTile in tilePlacer.AllPlacedTiles)
            {
                var targetGridCell = placedTile.Grid.GetGridCellObject(targetPosition);
                if (targetGridCell == null) continue;

                if (targetGridCell.CharacterOnTop != null && targetGridCell != _startGridCell) return;

                if (targetGridCell.Exits != null && !gameHandler.HasCharactersBeenOnPickupCells) return;

                var content = new object[] {_selectedCharacter.CharacterType, targetPosition};
                PhotonNetwork.RaiseEvent((int) PhotonEventCodes.MoveCharacterCode, content,
                    RaiseEventOptionsHelper.Others,
                    SendOptions.SendReliable);
                SelectedCharacterPosition = targetPosition + _additionalFloatPosition;
                _targetGridCell = targetGridCell;
                tempCharacter.position = _targetGridCell.CenterWorldPosition;
                tempCharacter.gameObject.SetActive(true);
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code >= 200) return;

            if (photonEvent.Code != (int) PhotonEventCodes.SelectCharacterCode &&
                photonEvent.Code != (int) PhotonEventCodes.MoveCharacterCode &&
                photonEvent.Code != (int) PhotonEventCodes.TryPlaceCharacterCode &&
                photonEvent.Code != (int) PhotonEventCodes.ConfirmPlaceCharacterCode) return;

            var data = (object[]) photonEvent.CustomData;
            var targetCharacterType = (CharacterType) data[0];
            Character targetCharacter = null;
            foreach (var character in gameHandler.CharacterOnTileDictionary.Keys)
            {
                if (character.CharacterType != targetCharacterType) continue;

                targetCharacter = character;
            }

            if (targetCharacter == null) return;

            switch (photonEvent.Code)
            {
                case (int) PhotonEventCodes.SelectCharacterCode:
                    HandleReceiveSelectCharacterCode(targetCharacterType, targetCharacter);
                    break;
                case (int) PhotonEventCodes.MoveCharacterCode:
                    if (_selectedCharacter && targetCharacterType == _selectedCharacter.CharacterType) return;

                    targetCharacter.transform.position = (Vector3) data[1] + _additionalFloatPosition;
                    break;
                case (int) PhotonEventCodes.TryPlaceCharacterCode:
                    HandleReceiveTryPlaceCharacterCode(targetCharacterType, targetCharacter, data);
                    break;
                case (int) PhotonEventCodes.ConfirmPlaceCharacterCode:
                    HandleReceiveConfirmPlaceCharacterCode(targetCharacterType, targetCharacter, data);
                    break;
            }
        }

        private void HandleReceiveSelectCharacterCode(CharacterType targetCharacterType, Component targetCharacter)
        {
            if (_selectedCharacter && targetCharacterType == _selectedCharacter.CharacterType) return;

            CharactersMoving[targetCharacterType] = true;
            var targetCharacterTransform = targetCharacter.transform;
            var selectedCharacterPosition = targetCharacterTransform.position;
            selectedCharacterPosition += _additionalFloatPosition;
            targetCharacterTransform.position = selectedCharacterPosition;
        }

        private void HandleReceiveTryPlaceCharacterCode(CharacterType targetCharacterType, Object targetCharacter, IReadOnlyList<object> data)
        {
            if (!CharactersMoving[targetCharacterType]) return;

            var targetTile = tilePlacer.AllPlacedTiles[(int) data[1]];
            var targetGridCell = targetTile.Grid.GetGridCellObject((Vector3) data[2]);
            if (targetGridCell.CharacterOnTop &&
                targetGridCell.CharacterOnTop != targetCharacter) return;

            PhotonNetwork.RaiseEvent((int) PhotonEventCodes.ConfirmPlaceCharacterCode, data,
                RaiseEventOptionsHelper.All,
                SendOptions.SendReliable);
        }

        private void HandleReceiveConfirmPlaceCharacterCode(CharacterType targetCharacterType,
            Character targetCharacter, IReadOnlyList<object> data)
        {
            if (!CharactersMoving[targetCharacterType]) return;

            var targetTile = tilePlacer.AllPlacedTiles[(int) data[1]];
            var targetGridCell = targetTile.Grid.GetGridCellObject((Vector3) data[2]);
            var startTile = tilePlacer.AllPlacedTiles[(int) data[3]];
            var startGridCell = startTile.Grid.GetGridCellObject((Vector3) data[4]);

            CharactersMoving[targetCharacterType] = false;
            startGridCell.ClearCharacter();
            targetGridCell.SetCharacter(targetCharacter);
            gameHandler.CharacterOnTileDictionary[targetCharacter] = targetGridCell.Tile;
            targetCharacter.transform.position = targetGridCell.CenterWorldPosition;

            if (!_selectedCharacter || targetCharacterType != _selectedCharacter.CharacterType) return;

            tempCharacter.gameObject.SetActive(false);
            _startGridCell = targetGridCell;
            _selectedCharacter = null;
            _targetGridCell = null;
        }

        private void HandleSelectingCharacter()
        {
            var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var characterHitInfo, Mathf.Infinity, characterLayerMask)) return;

            _plane.Raycast(ray, out var enter);
            _mouseStartPosition = ray.GetPoint(enter);
            var selectedCharacter = characterHitInfo.transform.GetComponent<Character>();
            if (CharactersMoving[selectedCharacter.CharacterType]) return;

            _selectedCharacter = selectedCharacter;
            CharactersMoving[selectedCharacter.CharacterType] = true;
            var boardTile = gameHandler.CharacterOnTileDictionary[selectedCharacter];
            _startGridCell = boardTile.Grid.GetGridCellObject(SelectedCharacterPosition);
            SelectedCharacterPosition += _additionalFloatPosition;
            Cursor.SetCursor(holdCursor, cursorOffset, CursorMode.Auto);
            var content = new object[] {_selectedCharacter.CharacterType};
            PhotonNetwork.RaiseEvent((int) PhotonEventCodes.SelectCharacterCode, content,
                RaiseEventOptionsHelper.Others,
                SendOptions.SendReliable);
        }

        private void HandlePlacingSelectedCharacter()
        {
            if (float.IsNegativeInfinity(_targetGridCell.CenterWorldPosition.x)) return;

            if (_targetGridCell == _startGridCell)
            {
                MoveCharacterToTargetPosition(_targetGridCell);
                return;
            }

            var direction = _targetGridCell.CenterWorldPosition -
                            _startGridCell.CenterWorldPosition;
            var normalizedDirection = direction.normalized;
            var movementDirection = movementCardSettings.GetAllPossibleMovementVector();
            if (movementCardSettings.canUsePortal && !gameHandler.HasCharactersBeenOnPickupCells)
            {
                var targetPortal = _targetGridCell.Portal;
                if (targetPortal != null && targetPortal.targetCharacterType == _selectedCharacter.CharacterType)
                {
                    MoveCharacterToTargetPosition(_targetGridCell);
                    return;
                }
            }

            if (movementCardSettings.canUseElevator)
            {
                if (_startGridCell.Elevator != null && _targetGridCell.Elevator != null &&
                    _startGridCell.Elevator == _targetGridCell.Elevator)
                {
                    MoveCharacterToTargetPosition(_targetGridCell);
                    return;
                }
            }

            _selectedCharacter.ToggleColliderOff();
            if (!movementDirection.Contains(normalizedDirection) ||
                Physics.Raycast(_startGridCell.CenterWorldPosition, normalizedDirection, direction.magnitude,
                    wallLayerMask) ||
                Physics.Raycast(_startGridCell.CenterWorldPosition, normalizedDirection,
                    direction.magnitude,
                    characterLayerMask))
            {
                _selectedCharacter.ToggleColliderOn();
                MoveCharacterToTargetPosition(_startGridCell);
                return;
            }

            _selectedCharacter.ToggleColliderOn();

            if (_targetGridCell.Exits != null &&
                _targetGridCell.Exits.Exists(exit => exit.targetCharacterType == _selectedCharacter.CharacterType))
            {
                TakeCharacterOutOfBoard(_selectedCharacter);
                return;
            }

            var targetPickup = _targetGridCell.Pickup;
            var targetHourglass = _targetGridCell.Hourglass;
            MoveCharacterToTargetPosition(_targetGridCell);

            if (targetPickup != null &&
                targetPickup.TargetCharacterType == _selectedCharacter.CharacterType)
            {
                gameHandler.NotifyCharacterPlacedOnPickupCell(_selectedCharacter.CharacterType, tempCharacter);
            }
            else if (targetHourglass is {isAvailable: true})
            {
                _targetGridCell.UseHourglass();
                timer.FlipHourglassTimer();
            }

        }

        private void MoveCharacterToTargetPosition(GridCell targetGridCell)
        {
            var targetTileIndex = tilePlacer.AllPlacedTiles.IndexOf(targetGridCell.Tile);
            var startTileIndex = tilePlacer.AllPlacedTiles.IndexOf(_startGridCell.Tile);
            var content = new object[]
            {
                _selectedCharacter.CharacterType, targetTileIndex, targetGridCell.CenterWorldPosition, startTileIndex,
                _startGridCell.CenterWorldPosition
            };
            PhotonNetwork.RaiseEvent((int) PhotonEventCodes.TryPlaceCharacterCode, content,
                RaiseEventOptionsHelper.MasterClient,
                SendOptions.SendReliable);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }

        private void TakeCharacterOutOfBoard(Character targetCharacter)
        {
            _startGridCell.ClearCharacter();
            gameHandler.NotifyTakeCharacterOutOfBoard(targetCharacter);
            targetCharacter.ToggleColliderOff();
            targetCharacter.transform.position = gameHandler.OutOfBoardTransform.position;
            _startGridCell = null;
            _selectedCharacter = null;
            _targetGridCell = null;
        }

        private void OnGameEnded()
        {
            enabled = false;
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void OnDestroy()
        {
            gameEndedEventChannel.GameEnded -= OnGameEnded;
        }
    }
}