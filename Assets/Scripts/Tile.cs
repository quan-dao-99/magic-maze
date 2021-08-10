using System.Collections.Generic;
using LiftStudio.EventChannels;
using UnityEngine;

namespace LiftStudio
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private PickedUpAllItemsEventChannel pickedUpAllItemsEventChannel;
        [SerializeField] private Transform tileVisualTransform;
        [SerializeField] private List<PortalSetup> portals;
        [SerializeField] private List<Elevator> elevators;
        [SerializeField] private List<ResearchPoint> researchTiles;
        [SerializeField] private List<ExitSetup> exitSetups;
        [SerializeField] private PickupSetup pickupSetup;
        [SerializeField] private HourglassSetup hourglassSetup;

        public CustomGrid<GridCell> Grid { get; private set; }

        private Transform OwnTransform => transform;

        private int _width;
        private int _height;
        private Vector3 _originPosition = Vector3.zero;

        public void SetupGrid(int width, int height, int cellSize)
        {
            _width = width;
            _height = height;
            _originPosition = tileVisualTransform.position +
                              OwnTransform.rotation * tileVisualTransform.localRotation * new Vector3(-2, 0, -2);
            Grid = new CustomGrid<GridCell>(OwnTransform, tileVisualTransform, _originPosition, width, height, cellSize,
                (grid, x, y) =>
                {
                    var portalSetup = portals.Find(portal => portal.gridPosition.x == x && portal.gridPosition.y == y);
                    var targetPortal = portalSetup != null
                        ? new Portal
                        {
                            targetCharacterType = portalSetup.targetCharacterType, usedMarker = portalSetup.usedMarker,
                            spriteRenderer = portalSetup.portalSpriteRenderer
                        }
                        : null;
                    var targetElevator = elevators.Find(elevator =>
                        elevator.firstGridPosition.x == x && elevator.firstGridPosition.y == y ||
                        elevator.secondGridPosition.x == x && elevator.secondGridPosition.y == y);
                    var targetResearchPoint = researchTiles.Find(researchPoint =>
                        researchPoint.gridPosition.x == x && researchPoint.gridPosition.y == y);
                    var isExitTile =
                        exitSetups.FindIndex(
                            exitSetup => exitSetup.gridPosition.x == x && exitSetup.gridPosition.y == y) > -1;
                    List<Exit> exitLists = null;
                    if (isExitTile)
                    {
                        exitLists = new List<Exit>();
                        foreach (var exitSetup in exitSetups)
                        {
                            exitLists.Add(new Exit {targetCharacterType = exitSetup.targetCharacterType});
                        }
                    }

                    var isPickupTile = pickupSetup.gridPosition.x == x && pickupSetup.gridPosition.y == y;
                    var pickup = isPickupTile
                        ? new Pickup {TargetCharacterType = pickupSetup.targetCharacterType}
                        : null;
                    var isHourglass = hourglassSetup.gridPosition.x == x && hourglassSetup.gridPosition.y == y;
                    var hourglass = isHourglass
                        ? new Hourglass(hourglassSetup.usedMarker, hourglassSetup.hourglassSpriteRenderer)
                        : null;
                    return new GridCell(grid, x, y, targetPortal, targetElevator, targetResearchPoint, exitLists,
                        pickup, hourglass, pickedUpAllItemsEventChannel, this);
                });
        }

        public GridCell GetTargetCharacterPortalGridCell(CharacterType targetCharacterType)
        {
            for (var w = 0; w < _width; w++)
            {
                for (var h = 0; h < _height; h++)
                {
                    var gridCell = Grid.GetGridCellObject(w, h);
                    if (gridCell.Portal == null) continue;
                    
                    if (gridCell.Portal.targetCharacterType != targetCharacterType) continue;

                    return gridCell;
                }
            }
            return null;
        }
        
        public GridCell GetOtherElevatorEndGridCell(GridCell initialGridCell)
        {
            for (var w = 0; w < _width; w++)
            {
                for (var h = 0; h < _height; h++)
                {
                    var gridCell = Grid.GetGridCellObject(w, h);
                    if (gridCell.Elevator == null) continue;
                    if (gridCell == initialGridCell) continue;
                    if (gridCell.Elevator != initialGridCell.Elevator) continue;

                    return gridCell;
                }
            }
            return null;
        }

        private void OnDestroy()
        {
            Grid?.Dispose();
        }
    }
}