using System;
using UnityEngine;

namespace LiftStudio
{
    [Serializable]
    public class PickupSetup
    {
        public CharacterType targetCharacterType;
        public Vector2Int gridPosition;
    }

    public class Pickup
    {
        public CharacterType targetCharacterType;
    }
}