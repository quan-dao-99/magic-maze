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

        public CharacterMovementControllerSetup(Camera gameCamera, MovementCardSettings movementCardSettings,
            Transform tempCharacter, TilePlacer tilePlacer, Game gameHandler, Timer timer)
        {
            GameCamera = gameCamera;
            MovementCardSettings = movementCardSettings;
            TempCharacter = tempCharacter;
            TilePlacer = tilePlacer;
            GameHandler = gameHandler;
            Timer = timer;
        }
    }
}