using System;
using UnityEngine;

namespace LiftStudio
{
    [Serializable]
    public class Portal
    {
        public CharacterType targetCharacterType;
        public Vector2Int gridPosition;
    }
}