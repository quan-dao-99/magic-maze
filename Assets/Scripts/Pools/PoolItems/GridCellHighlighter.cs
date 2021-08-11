using UnityEngine;

namespace LiftStudio.Pools.PoolItems
{
    public class GridCellHighlighter : MonoBehaviour
    {
        [SerializeField] private Vector3 offSet;

        public void SetPosition(Vector3 basePosition)
        {
            transform.position = basePosition + offSet;
        }
    }
}