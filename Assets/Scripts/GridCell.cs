using System;
using System.Collections.Generic;
using LiftStudio.EventChannels;
using UnityEngine;

namespace LiftStudio
{
    public class GridCell : IDisposable
    {
        public Vector3 CenterWorldPosition { get; }
        public Character CharacterOnTop { get; private set; }
        public Portal Portal { get; }
        public Elevator Elevator { get; }
        public ResearchPoint ResearchPoint { get; }
        public List<Exit> Exits { get; }
        public Pickup Pickup { get; }
        public Hourglass Hourglass { get; }
        public Tile Tile { get; }

        private readonly PickedUpAllItemsEventChannel _pickedUpAllItemsEventChannel;
        private readonly AllMovableGridCellsSetEventChannel _allMovableGridCellsSetEventChannel;
        private readonly GridCellHighlighterHandler _highlighterHandler;

        private readonly int _x;
        private readonly int _y;

        public GridCell(Tile tile, CustomGrid<GridCell> grid, int x, int y, Portal portal, Elevator elevator,
            ResearchPoint researchPoint, List<Exit> exits, Pickup pickup, Hourglass hourglass,
            PickedUpAllItemsEventChannel pickedUpAllItemsEventChannel,
            AllMovableGridCellsSetEventChannel allMovableGridCellsSetEventChannel,
            GridCellHighlighterHandler highlighterHandler)
        {
            _x = x;
            _y = y;
            
            Portal = portal;
            Elevator = elevator;
            ResearchPoint = researchPoint;
            Exits = exits;
            Pickup = pickup;
            Hourglass = hourglass;
            Tile = tile;
            CenterWorldPosition = grid.GetCellCenterWorldPosition(x, y);

            if (Portal != null)
            {
                _pickedUpAllItemsEventChannel = pickedUpAllItemsEventChannel;

                pickedUpAllItemsEventChannel.AllItemsPickedUp += OnAllItemsPickedUp;
            }

            _highlighterHandler = highlighterHandler;
            _allMovableGridCellsSetEventChannel = allMovableGridCellsSetEventChannel;
            
            allMovableGridCellsSetEventChannel.AllMovableGridCellsSet += OnAllMovableGridCellsSet;
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

        private void OnAllMovableGridCellsSet(List<GridCell> allMovableGridCells)
        {
            if (allMovableGridCells.Count == 0 || !allMovableGridCells.Contains(this))
            {
                _highlighterHandler.OnDehighlighted();
                return;
            }

            _highlighterHandler.OnHighlighted(this);
        }

        public void Dispose()
        {
            if (_pickedUpAllItemsEventChannel == null) return;

            _pickedUpAllItemsEventChannel.AllItemsPickedUp -= OnAllItemsPickedUp;
            _allMovableGridCellsSetEventChannel.AllMovableGridCellsSet -= OnAllMovableGridCellsSet;
        }
    }
}