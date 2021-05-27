using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    public class GameSetup : MonoBehaviour
    {
        [SerializeField] private CharacterMovementController characterMovementController;
        [SerializeField] private List<Character> allCharacters;
        [SerializeField] private StartingTile startingTile;
        [SerializeField] private Transform startTilePositionTransform;

        private void Start()
        {
            var spawnPosition = startTilePositionTransform.position;
            var tile = Instantiate(startingTile, spawnPosition, Quaternion.identity);
            TilePlacer.PlaceTile(tile, spawnPosition, Quaternion.identity);

            foreach (var character in allCharacters)
            {
                var characterPosition = tile.GetRandomCharacterSpawnPosition().position;
                var spawnedCharacter =
                    Instantiate(character, characterPosition, Quaternion.identity);
                characterMovementController.CharacterOnTileDictionary[spawnedCharacter] = tile;
                tile.Grid.GetGridCellObject(characterPosition).SetCharacter(spawnedCharacter);
            }
        }
    }
}