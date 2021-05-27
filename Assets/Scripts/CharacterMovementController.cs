using System.Collections.Generic;
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
        [SerializeField] private TileStackController tileStackController;

        public Dictionary<Character, Tile> CharacterOnTileDict { get; } = new Dictionary<Character, Tile>();

        private Vector3 _mouseStartPosition;
        private Character _selectedCharacter;
        private GridCell _startGridCell;
        private GridCell _targetGridCell;
        private Plane _plane = new Plane(Vector3.up, Vector3.zero);

        public void HandleTakeNewTile()
        {
            foreach (var pair in CharacterOnTileDict)
            {
                var characterGridCell = pair.Value.Grid.GetGridCellObject(pair.Key.transform.position);
                var gridCellResearchPoint = characterGridCell.ResearchPoint;
                if (gridCellResearchPoint == null) continue;

                if (gridCellResearchPoint.targetCharacterType != pair.Key.CharacterType) continue;

                if (gridCellResearchPoint.hasResearched) break;

                TilePlacer.PlaceTile(tileStackController.GameTileStacks.Pop(),
                    gridCellResearchPoint.attachPoint.position,
                    Quaternion.LookRotation(gridCellResearchPoint.attachPoint.forward));
                gridCellResearchPoint.hasResearched = true;
            }
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
            var boardTile = CharacterOnTileDict[_selectedCharacter];
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
            if (movementCardSettings.canUsePortal)
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
            MoveCharacterToTargetPosition(_targetGridCell);
        }

        private void MoveCharacterToTargetPosition(GridCell targetGridCell)
        {
            targetGridCell.SetCharacter(_selectedCharacter);
            _startGridCell.ClearCharacter();
            CharacterOnTileDict[_selectedCharacter] = targetGridCell.Tile;
            _startGridCell = targetGridCell;
            _selectedCharacter.transform.position = targetGridCell.CenterWorldPosition;
            _selectedCharacter = null;
            _targetGridCell = null;
        }
    }
}