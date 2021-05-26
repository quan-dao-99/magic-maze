using System;

namespace LiftStudio
{
    [Flags]
    public enum MovementDirection
    {
        Right = 1,
        Down = 2,
        Left = 4,
        Up = 8
    }

    public static class MovementDirectionExtension
    {
        public static bool HasDirectionFlag(this MovementDirection movementDirection, MovementDirection directionToCheck)
        {
            return (movementDirection & directionToCheck) == directionToCheck;
        }
    }
}