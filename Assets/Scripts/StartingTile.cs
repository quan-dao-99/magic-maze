using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    public class StartingTile : Tile
    {
        [SerializeField] private List<Transform> characterSpawnPositions;

        private List<Transform> _runTimeCharacterSpawnPositions;

        private void Awake()
        {
            _runTimeCharacterSpawnPositions = new List<Transform>(characterSpawnPositions);
        }

        public Transform GetRandomCharacterSpawnPosition()
        {
            var randomIndex = Random.Range(0, _runTimeCharacterSpawnPositions.Count);
            var targetTransform = _runTimeCharacterSpawnPositions[randomIndex];
            _runTimeCharacterSpawnPositions.RemoveAt(randomIndex);
            return targetTransform;
        }
    }
}