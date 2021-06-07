using System;
using UnityEngine;

namespace LiftStudio
{
    [Serializable]
    public class HourglassSetup
    {
        public Vector2Int gridPosition = new Vector2Int(-1, -1);
        public GameObject usedMarker;
        public SpriteRenderer hourglassSpriteRenderer;
    }

    public class Hourglass
    {
        public bool isAvailable;
        public readonly GameObject usedMarker;
        public readonly SpriteRenderer spriteRenderer;

        public Hourglass(GameObject usedMarker, SpriteRenderer spriteRenderer)
        {
            this.usedMarker = usedMarker;
            this.spriteRenderer = spriteRenderer;
            isAvailable = true;
        }
    }
}