using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Transform tileVisualTransform;
        [SerializeField] private List<Elevator> elevators;
        [SerializeField] private List<Portal> portals;
        [SerializeField] private List<ResearchPoint> researchTiles;

        public CustomGrid<GridCell> Grid { get; private set; }

        private Transform OwnTransform => transform;

        private const int Width = 4;
        private const int Height = 4;
        private const int CellSize = 1;
        private Vector3 _originPosition = Vector3.zero;

        public void SetupGrid()
        {
            _originPosition = tileVisualTransform.position +
                              OwnTransform.rotation * tileVisualTransform.localRotation * new Vector3(-2, 0, -2);
            Grid = new CustomGrid<GridCell>(OwnTransform, tileVisualTransform, _originPosition, Width, Height, CellSize,
                (grid, x, y) =>
                {
                    var targetPortal = portals.Find(portal => portal.gridPosition.x == x && portal.gridPosition.y == y);
                    var targetElevator = elevators.Find(elevator =>
                        elevator.firstGridPosition.x == x && elevator.firstGridPosition.y == y ||
                        elevator.secondGridPosition.x == x && elevator.secondGridPosition.y == y);
                    var targetResearchPoint = researchTiles.Find(researchPoint =>
                        researchPoint.gridPosition.x == x && researchPoint.gridPosition.y == y);
                    return new GridCell(grid, x, y, targetPortal, targetElevator, targetResearchPoint, this);
                });
        }

        private void OnDrawGizmos()
        {
            Debug.DrawLine(GetCellWorldPosition(0, 0), GetCellWorldPosition(1, 1), Color.red, 1f);
            Debug.DrawLine(GetCellWorldPosition(0, 1), GetCellWorldPosition(1, 0), Color.red, 1f);
        }

        private Vector3 GetCellWorldPosition(int x, int y)
        {
            return OwnTransform.rotation * tileVisualTransform.localRotation * (new Vector3(x, 0, y) * CellSize) +
                   _originPosition;
        }
    }
}