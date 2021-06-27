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
        [SerializeField] private Game gameHandler;
        [SerializeField] private Timer timer;

        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;

        private const byte SelectCharacterEventCode = 1;
        private const byte MoveCharacterEventCode = 2;

        private Vector3 _mouseStartPosition;
        private Character _selectedCharacter;
        private GridCell _startGridCell;
        private GridCell _targetGridCell;
        private Vector3 _additionalFloatPosition;

        private Plane _plane = new Plane(Vector3.up, Vector3.zero);
        private readonly RaiseEventOptions _raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };

        private Vector3 SelectedCharacterPosition
        {
            get => _selectedCharacter.transform.position;
            set => _selectedCharacter.transform.position = value;
        }

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

            if (_selectedCharacter == null) return;

            if (!Input.GetMouseButton(0)) return;

            var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            _plane.Raycast(ray, out var enter);
            var planeHitPoint = ray.GetPoint(enter);
            var mouseMoveDirection = planeHitPoint - _mouseStartPosition;
            var targetPosition = _startGridCell.CenterWorldPosition + mouseMoveDirection;

            foreach (var placedTile in TilePlacer.AllPlacedTiles)
            {
                var targetGridCell = placedTile.Grid.GetGridCellObject(targetPosition);
                if (targetGridCell == null) continue;

                if (targetGridCell.CharacterOnTop != null && targetGridCell != _startGridCell) return;

                if (targetGridCell.Exits != null && !gameHandler.HasCharactersBeenOnPickupCells) return;

                var content = new object[] {_selectedCharacter.CharacterType, targetPosition};
                PhotonNetwork.RaiseEvent(MoveCharacterEventCode, content, _raiseEventOptions,
                    SendOptions.SendReliable);
                _targetGridCell = targetGridCell;
                tempCharacter.position = _targetGridCell.CenterWorldPosition;
                tempCharacter.gameObject.SetActive(true);
            }
        }
        
        public void OnEvent(EventData photonEvent)
        {
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
                case SelectCharacterEventCode:
                    var boardTile = gameHandler.CharacterOnTileDictionary[targetCharacter];
                    var selectedCharacterPosition = targetCharacter.transform.position;
                    _startGridCell = boardTile.Grid.GetGridCellObject(selectedCharacterPosition);
                    _selectedCharacter = targetCharacter;
                    selectedCharacterPosition += _additionalFloatPosition;
                    targetCharacter.transform.position = selectedCharacterPosition;
                    break;
                case MoveCharacterEventCode:
                    var targetPosition = (Vector3) data[1];
                    targetCharacter.transform.position = targetPosition + _additionalFloatPosition;
                    break;
            }
        }

        private void HandleSelectingCharacter()
        {
            var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var characterHitInfo, Mathf.Infinity, characterLayerMask)) return;

            _plane.Raycast(ray, out var enter);
            _mouseStartPosition = ray.GetPoint(enter);
            var selectedCharacter = characterHitInfo.transform.GetComponent<Character>();
            var boardTile = gameHandler.CharacterOnTileDictionary[selectedCharacter];
            var selectedCharacterPosition = selectedCharacter.transform.position;
            _startGridCell = boardTile.Grid.GetGridCellObject(selectedCharacterPosition);
            _selectedCharacter = selectedCharacter;
            Cursor.SetCursor(holdCursor, cursorOffset, CursorMode.Auto);
            var content = new object[] {_selectedCharacter.CharacterType};
            PhotonNetwork.RaiseEvent(SelectCharacterEventCode, content, _raiseEventOptions,
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

            if (_targetGridCell.Pickup != null &&
                _targetGridCell.Pickup.targetCharacterType == _selectedCharacter.CharacterType)
            {
                gameHandler.NotifyCharacterPlacedOnPickupCell(tempCharacter);
            }
            else if (_targetGridCell.Hourglass is {isAvailable: true})
            {
                _targetGridCell.UseHourglass();
                timer.FlipHourglassTimer();
            }

            MoveCharacterToTargetPosition(_targetGridCell);
        }

        private void MoveCharacterToTargetPosition(GridCell targetGridCell)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            targetGridCell.SetCharacter(_selectedCharacter);
            _startGridCell.ClearCharacter();
            gameHandler.CharacterOnTileDictionary[_selectedCharacter] = targetGridCell.Tile;
            tempCharacter.gameObject.SetActive(false);
            _startGridCell = targetGridCell;
            _selectedCharacter.transform.position = targetGridCell.CenterWorldPosition;
            _selectedCharacter = null;
            _targetGridCell = null;
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