using System;
using UnityEngine;

namespace LiftStudio.EventChannels
{
    [CreateAssetMenu(fileName = "LocalControllerSetEventChannel",
        menuName = "Events/LocalControllerSetEventChannel")]
    public class LocalControllerSetEventChannel : ScriptableObject
    {
        public event Action<CharacterMovementController> LocalCharacterMovementControllerSet;

        public void RaiseEvent(CharacterMovementController characterMovementController)
        {
            LocalCharacterMovementControllerSet?.Invoke(characterMovementController);
        }
    }
}