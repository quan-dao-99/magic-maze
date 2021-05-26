using System;
using UnityEngine;

namespace LiftStudio
{
    [Serializable]
    public class ResearchPoint
    {
        public CharacterType targetCharacterType;
        public Vector2Int gridPosition;
        public Transform attachPoint;
        [HideInInspector] public bool hasResearched;
    }
}