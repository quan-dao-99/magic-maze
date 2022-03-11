using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    public class CharacterMovementControllerSetup
    {
        public readonly Camera GameCamera;
        public readonly MovementCardSettings MovementCardSettings;
        public readonly Transform TempCharacter;
        public readonly TilePlacer TilePlacer;
        public readonly Game GameHandler;
        public readonly Timer Timer;

        public CharacterMovementControllerSetup(Camera gameCamera, List<MovementCardSettings> movementCardSettings,
            Transform tempCharacter, TilePlacer tilePlacer, Game gameHandler, Timer timer)
        {
            GameCamera = gameCamera;
            MovementCardSettings = CombineMovementCards(movementCardSettings);
            TempCharacter = tempCharacter;
            TilePlacer = tilePlacer;
            GameHandler = gameHandler;
            Timer = timer;
        }

        private static MovementCardSettings CombineMovementCards(List<MovementCardSettings> movementCardSettingsList)
        {
            var combinedCard = ScriptableObject.CreateInstance<MovementCardSettings>();
            foreach (var movementCard in movementCardSettingsList)
            {
                combinedCard.movementDirection |= movementCard.movementDirection;
                if (!combinedCard.canUseElevator) combinedCard.canUseElevator = movementCard.canUseElevator;
                if (!combinedCard.canUsePortal) combinedCard.canUsePortal = movementCard.canUsePortal;
                if (!combinedCard.canUseResearch) combinedCard.canUseResearch = movementCard.canUseResearch;
            }

            return combinedCard;
        }
    }
}