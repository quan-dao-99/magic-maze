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
        public Hourglass Hourglass { get; }
        public Tile Tile { get; }

        private readonly CustomGrid<GridCell> _grid;
        private readonly int _x;
        private readonly int _y;

        public GridCell(CustomGrid<GridCell> grid, int x, int y, Portal portal, Elevator elevator,
            ResearchPoint researchPoint, List<Exit> exits, Pickup pickup, Hourglass hourglass, Tile tile)
        {
            _grid = grid;
            _x = x;
            _y = y;
            Portal = portal;
            Elevator = elevator;
            ResearchPoint = researchPoint;
            Exits = exits;
            Pickup = pickup;
            Hourglass = hourglass;
            Tile = tile;

            if (Portal == null) return;

            Game.AllItemsPickedUp += OnAllItemsPickedUp;
        }

        public void SetCharacter(Character targetCharacter)
        {
            CharacterOnTop = targetCharacter;
        }

        public void ClearCharacter()
        {
            CharacterOnTop = null;
        }

        public void UseHourglass()
        {
            Hourglass.isAvailable = false;
            Hourglass.usedMarker.SetActive(true);
            var fadedColor = Hourglass.spriteRenderer.color;
            fadedColor.a /= 3;
            Hourglass.spriteRenderer.color = fadedColor;
        }

        private void OnAllItemsPickedUp()
        {
            Portal.usedMarker.SetActive(true);
            var fadedColor = Portal.spriteRenderer.color;
            fadedColor.a /= 3;
            Portal.spriteRenderer.color = fadedColor;
        }
    }
}