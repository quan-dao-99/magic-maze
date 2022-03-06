using LiftStudio.Pools;
using LiftStudio.Pools.PoolItems;
using UnityEngine;

namespace LiftStudio
{
    public class GridCellHighlighterHandler
    {
        private readonly GridCellHighlighterPool _gridCellHighlighterPool;

        private GridCellHighlighter _gridCellHighlighter;
        
        public GridCellHighlighterHandler(GridCellHighlighterPool gridCellHighlighterPool)
        {
            _gridCellHighlighterPool = gridCellHighlighterPool;
        }

        public void OnHighlighted(GridCell targetGridCell)
        {
            if (_gridCellHighlighter != null) return;

            _gridCellHighlighter = _gridCellHighlighterPool.Get(_gridCellHighlighterPool.transform);
            _gridCellHighlighter.SetPosition(targetGridCell.CenterWorldPosition);
            _gridCellHighlighter.gameObject.SetActive(true);
        }

        public void OnDehighlighted()
        {
            if (_gridCellHighlighter == null) return;

            _gridCellHighlighterPool.ReturnToPool(_gridCellHighlighter);
            _gridCellHighlighter = null;
        }
    }
}