using UnityEngine;

namespace LiftStudio
{
    public static class MathUtils
    {
        public static Vector3 FloorOrCeilToIntVector3(Vector3 sourceVector)
        {
            var floorX = sourceVector.x >= 0 ? Mathf.FloorToInt(sourceVector.x) : Mathf.CeilToInt(sourceVector.x);
            var floorY = sourceVector.y >= 0 ? Mathf.FloorToInt(sourceVector.y) : Mathf.CeilToInt(sourceVector.y);
            var floorZ = sourceVector.z >= 0 ? Mathf.FloorToInt(sourceVector.z) : Mathf.CeilToInt(sourceVector.z);
            return new Vector3(floorX, floorY, floorZ);
        }
    }
}