using System;
using UnityEngine;

namespace LiftStudio
{
    [CreateAssetMenu(fileName = "PickedUpAllItemsEventChannel", menuName = "Events/PickedUpAllItemsEventChannel")]
    public class PickedUpAllItemsEventChannel : ScriptableObject
    {
        public event Action AllItemsPickedUp;

        public void RaiseEvent()
        {
            AllItemsPickedUp?.Invoke();
        }

        [ContextMenu("TriggerEvent")]
        public void TriggerEvent()
        {
            RaiseEvent();
        }
    }
}