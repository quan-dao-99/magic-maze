using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    public class GridCell
    {
        public Vector3 CenterWorldPosition => _grid.GetCellCenterWorldPosition(_x, _y);
        public Character CharacterOnTop { get; private set; }
        public Portal Portal { get; }
        public Elevator Elevator { get; }
        public ResearchPoint ResearchPoint { get; }
        public List<Exit> Exits { get; }
        public Pickup Pickup { get; }
        public Tile Tile { get; }

        private readonly CustomGrid<GridCell> _grid;
        private readonly int _x;
        private readonly int _y;

        public GridCell(CustomGrid<GridCell> grid, int x, int y, Portal portal, Elevator elevator,
            ResearchPoint researchPoint, List<Exit> exits, Pickup pickup, Tile tile)
        {
            _grid = grid;
            _x = x;
            _y = y;
            Portal = portal;
            Elevator = elevator;
            ResearchPoint = researchPoint;
            Exits = exits;
            Tile = tile;
            Pickup = pickup;
        }

        public void SetCharacter(Character targetCharacter)
        {
            CharacterOnTop = targetCharacter;
        }

        public void ClearCharacter()
        {
            CharacterOnTop = null;
        }
    }
}