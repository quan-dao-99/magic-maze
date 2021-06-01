using System;
using UnityEngine;

namespace LiftStudio
{
    [Serializable]
    public class HourglassSetup
    {
        public Vector2Int gridPosition = new Vector2Int(-1, -1);
        public GameObject usedMarker;
    }

    public class Hourglass
    {
        public bool isAvailable;
        public GameObject usedMarker;

        public Hourglass(GameObject usedMarker)
        {
            this.usedMarker = usedMarker;
            isAvailable = true;
        }
    }
}