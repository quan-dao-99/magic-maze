using System;
using UnityEngine;

namespace LiftStudio.EventChannels
{
    [CreateAssetMenu(fileName = "GameEndedEventChannel", menuName = "Events/GameEndedEventChannel")]
    public class GameEndedEventChannel : ScriptableObject
    {
        public event Action GameEnded;

        public void RaiseEvent()
        {
            GameEnded?.Invoke();
        }
        
        [ContextMenu("TriggerEvent")]
        public void TriggerEvent()
        {
            RaiseEvent();
        }
    }
}