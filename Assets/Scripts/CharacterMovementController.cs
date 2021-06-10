using UnityEngine;
using UnityEngine.EventSystems;

namespace LiftStudio
{
    public class CharacterMovementController : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private LayerMask characterLayerMask;
        [SerializeField] private LayerMask wallLayerMask;
        [SerializeField] private MovementCardSettings movementCardSettings;
        [SerializeField] private Game gameHandler;
        [SerializeField] private Timer timer;

        private Vector3 _mouseStartPosition;
        private Character _selectedCharacter;
        private GridCell _startGridCell;
        private GridCell _targetGridCell;
        private Plane _plane = new Plane(Vector3.up, Vector3.zero);

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

                _targetGridCell = targetGridCell;
                _selectedCharacter.transform.position = _targetGridCell.CenterWorldPosition;
            }
        }

        private void HandleSelectingCharacter()
        {
            var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var characterHitInfo, Mathf.Infinity, characterLayerMask)) return;

            _plane.Raycast(ray, out var enter);
            _mouseStartPosition = ray.GetPoint(enter);
            _selectedCharacter = characterHitInfo.transform.GetComponent<Character>();
            var boardTile = gameHandler.CharacterOnTileDictionary[_selectedCharacter];
            _startGridCell = boardTile.Grid.GetGridCellObject(_selectedCharacter.transform.position);
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
                gameHandler.NotifyCharacterPlacedOnPickupCell();
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
            targetGridCell.SetCharacter(_selectedCharacter);
            _startGridCell.ClearCharacter();
            gameHandler.CharacterOnTileDictionary[_selectedCharacter] = targetGridCell.Tile;
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
    }
}