using LiftStudio.EventChannels;
using UnityEngine;

namespace LiftStudio
{
    public class TakeNewTileButton : MonoBehaviour
    {
        [SerializeField] private LocalControllerSetEventChannel controllerSetEventChannel;

        private void Awake()
        {
            controllerSetEventChannel.LocalCharacterMovementControllerSet += OnControllerSet;
        }

        private void OnControllerSet(CharacterMovementController characterMovementController)
        {
            if (characterMovementController.MovementCardSettings.canUseResearch) return;
            
            gameObject.SetActive(false);
        }
    }
}