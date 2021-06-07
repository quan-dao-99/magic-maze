using System;
using UnityEngine;

namespace LiftStudio
{
    [Serializable]
    public class PortalSetup
    {
        public GameObject usedMarker;
        public SpriteRenderer portalSpriteRenderer;
        public CharacterType targetCharacterType;
        public Vector2Int gridPosition;
    }

    public class Portal
    {
        public GameObject usedMarker;
        public SpriteRenderer spriteRenderer;
        public CharacterType targetCharacterType;
    }
}