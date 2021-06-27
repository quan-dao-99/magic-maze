using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace LiftStudio
{
    public class GameSetup : MonoBehaviour
    {
        [SerializeField] private TileStackController tileStackController;
        [SerializeField] private Game gameHandler;
        [SerializeField] private List<Character> allCharacters;
        [SerializeField] private StartingTile startingTile;
        [SerializeField] private Transform startTilePositionTransform;

        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            tileStackController.SetupTileStacks();
            var spawnPosition = startTilePositionTransform.position;
            var tile = PhotonNetwork.InstantiateRoomObject(startingTile.name, spawnPosition, Quaternion.identity)
                .GetComponent<StartingTile>();
            TilePlacer.PlaceTile(tile, spawnPosition, Quaternion.identity);

            foreach (var character in allCharacters)
            {
                var characterPosition = tile.GetRandomCharacterSpawnPosition().position;
                var spawnedCharacter =
                    PhotonNetwork.InstantiateRoomObject(character.name, characterPosition, Quaternion.identity)
                        .GetComponent<Character>();
                gameHandler.CharacterOnTileDictionary[spawnedCharacter] = tile;
                tile.Grid.GetGridCellObject(characterPosition).SetCharacter(spawnedCharacter);
            }
        }
    }
}