using System;
using UnityEngine;

namespace LiftStudio
{
    [Serializable]
    public class ExitSetup
    {
        public CharacterType targetCharacterType;
        public Vector2Int gridPosition;
    }

    public class Exit
    {
        public CharacterType targetCharacterType;
    }
}