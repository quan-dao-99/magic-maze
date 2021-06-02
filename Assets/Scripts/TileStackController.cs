using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    public class TileStackController : MonoBehaviour
    {
        [SerializeField] private List<Tile> allTiles;
        [SerializeField] private Transform tileStackSpawnPosition;

        public Stack<Tile> GameTileStacks { get; } = new Stack<Tile>();

        private void Start()
        {
            var allTilesCopy = new List<Tile>(allTiles);
            var nextTileVerticalPosition = tileStackSpawnPosition.position;
            while (allTilesCopy.Count != 0)
            {
                var randomTileIndex = Random.Range(0, allTilesCopy.Count);
                nextTileVerticalPosition.y += 0.5f;
                var tile = Instantiate(allTilesCopy[randomTileIndex], nextTileVerticalPosition,
                    Quaternion.Euler(180, 0, 0),
                    tileStackSpawnPosition);
                GameTileStacks.Push(tile);
                allTilesCopy.RemoveAt(randomTileIndex);
            }
        }
    }
}