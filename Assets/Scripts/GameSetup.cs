using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace LiftStudio
{
    public class GameSetup : MonoBehaviour, IOnEventCallback
    {
        [SerializeField] private TileStackController tileStackController;
        [SerializeField] private Game gameHandler;
        [SerializeField] private List<Character> allCharacters;
        [SerializeField] private StartingTile startingTile;
        [SerializeField] private Transform startTilePositionTransform;

        private readonly RaiseEventOptions _raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};

        private StartingTile _spawnedStartingTile;

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start()
        {
            var spawnPosition = startTilePositionTransform.position;
            _spawnedStartingTile = Instantiate(startingTile, spawnPosition, Quaternion.identity);
            TilePlacer.PlaceTile(_spawnedStartingTile, spawnPosition, Quaternion.identity);
            
            if (!PhotonNetwork.IsMasterClient) return;
            
            tileStackController.SetupTileStacks();
            var content = new Dictionary<int, object>();

            for (var index = 0; index < allCharacters.Count; index++)
            {
                var characterPosition = _spawnedStartingTile.GetRandomCharacterSpawnPosition();
                content.Add(index, characterPosition);
            }

            PhotonNetwork.RaiseEvent((int) PhotonEventCodes.SetupPlayersCode, content, _raiseEventOptions,
                SendOptions.SendReliable);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code >= 200) return;

            if (photonEvent.Code != (int) PhotonEventCodes.SetupPlayersCode) return;

            var charactersPositionDictionary = (Dictionary<int, object>) photonEvent.CustomData;
            foreach (var pair in charactersPositionDictionary)
            {
                var position = (Vector3) pair.Value;
                var spawnedCharacter = Instantiate(allCharacters[pair.Key], position, Quaternion.identity);
                gameHandler.CharacterOnTileDictionary[spawnedCharacter] = _spawnedStartingTile;
                _spawnedStartingTile.Grid.GetGridCellObject(position).SetCharacter(spawnedCharacter);
            }
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }
}