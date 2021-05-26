using UnityEngine;

namespace LiftStudio
{
    public class GridCell
    {
        public Character characterOnTop;

        public Vector3 CenterWorldPosition => _grid.GetCellCenterWorldPosition(_x, _y);
        public Portal Portal { get; }
        public ResearchPoint ResearchPoint { get; }
        public Elevator Elevator { get; }
        public Tile Tile { get; }

        private readonly CustomGrid<GridCell> _grid;
        private readonly int _x;
        private readonly int _y;

        public GridCell(CustomGrid<GridCell> grid, int x, int y, Portal portal, Elevator elevator,
            ResearchPoint researchPoint, Tile tile)
        {
            _grid = grid;
            _x = x;
            _y = y;
            Portal = portal;
            Elevator = elevator;
            ResearchPoint = researchPoint;
            Tile = tile;
        }
    }
}