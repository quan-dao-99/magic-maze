using System;
using UnityEngine;

namespace LiftStudio
{
    [Serializable]
    public class PickupSetup
    {
        public CharacterType targetCharacterType;
        public Vector2Int gridPosition = new Vector2Int(-1, -1);
    }

    public class Pickup
    {
        public CharacterType targetCharacterType;
    }
}