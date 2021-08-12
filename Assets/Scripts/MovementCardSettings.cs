using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    [CreateAssetMenu(fileName = "MovementCardSettings", menuName = "Movement Card Settings")]
    public class MovementCardSettings : ScriptableObject
    {
        [EnumFlag] public MovementDirection movementDirection;
        public bool canUseElevator;
        public bool canUsePortal;
        public bool canUseResearch;

        public List<Vector3> GetAllPossibleMovementVector()
        {
            var allMovementVectors = new List<Vector3>();
            if (movementDirection.HasDirectionFlag(MovementDirection.Right))
            {
                allMovementVectors.Add(Vector3.right);
            }

            if (movementDirection.HasDirectionFlag(MovementDirection.Down))
            {
                allMovementVectors.Add(-Vector3.forward);
            }

            if (movementDirection.HasDirectionFlag(MovementDirection.Left))
            {
                allMovementVectors.Add(Vector3.left);
            }

            if (movementDirection.HasDirectionFlag(MovementDirection.Up))
            {
                allMovementVectors.Add(Vector3.forward);
            }

            return allMovementVectors;
        }

        public List<float> GetCardArrowRotation()
        {
            var allCardArrowDirection = new List<float>();
            if (movementDirection.HasDirectionFlag(MovementDirection.Right))
            {
                allCardArrowDirection.Add(-90f);
            }

            if (movementDirection.HasDirectionFlag(MovementDirection.Down))
            {
                allCardArrowDirection.Add(180f);
            }

            if (movementDirection.HasDirectionFlag(MovementDirection.Left))
            {
                allCardArrowDirection.Add(90f);
            }

            if (movementDirection.HasDirectionFlag(MovementDirection.Up))
            {
                allCardArrowDirection.Add(0f);
            }
            
            return allCardArrowDirection;
        }
    }
}