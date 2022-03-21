using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio.EventChannels
{
    [CreateAssetMenu(fileName = "AllMovableGridCellsSetEventChannel", menuName = "Events/AllMovableGridCellsSetEventChannel")]
    public class AllMovableGridCellsSetEventChannel : ScriptableObject
    {
        public event Action<List<GridCell>> AllMovableGridCellsSet;

        public void RaiseEvent(List<GridCell> allMovableGridCells)
        {
            AllMovableGridCellsSet?.Invoke(allMovableGridCells);
        }
    }
}