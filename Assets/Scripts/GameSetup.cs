using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using SystemRandom = System.Random;

namespace LiftStudio
{
    public class GameSetup : MonoBehaviourPun
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private MovementCardsSetupCollection movementCardsSetupCollection;
        [SerializeField] private Transform tempCharacter;
        [SerializeField] private Timer timer;
        [SerializeField] private TilePlacer tilePlacer;
        [SerializeField] private TileStackController tileStackController;
        [SerializeField] private Game gameHandler;
        [SerializeField] private List<Character> allCharacters;
        [SerializeField] private StartingTile startingTile;
        [SerializeField] private MovementCard movementCard;
        [SerializeField] private Transform startTilePositionTransform;
        [SerializeField] private Transform movementCardContainerParent;

        public static GameSetup Instance;

        private int _cardSetupIndex;
        private StartingTile _spawnedStartingTile;
        private List<MovementCardSettings> _runtimeMovementCardSettingsList;

        private void Awake()
        {
            if (Instance != this)
            {
                Destroy(Instance);
                Instance = this;
            }

            if (!PhotonNetwork.IsMasterClient) return;

            for (var index = 0; index < movementCardsSetupCollection.allSetups.Count; index++)
            {
                var movementCardsSetup = movementCardsSetupCollection.allSetups[index];
                if (movementCardsSetup.playerCount != PhotonNetwork.CurrentRoom.PlayerCount) continue;

                _cardSetupIndex = index;
                var random = new SystemRandom();
                var randomMovementCards = movementCardsSetup.cardSet.OrderBy(item => random.Next());
                _runtimeMovementCardSettingsList = new List<MovementCardSettings>(randomMovementCards);
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start()
        {
            var spawnPosition = startTilePositionTransform.position;
            _spawnedStartingTile = Instantiate(startingTile, spawnPosition, Quaternion.identity);
            tilePlacer.PlaceTile(_spawnedStartingTile, spawnPosition, Quaternion.identity);

            photonView.RPC("GetMovementCardSettingsRPC", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.UserId);
            
            if (!PhotonNetwork.IsMasterClient) return;
            
            tileStackController.SetupTileStacks();
            var content = new Dictionary<int, object>();

            for (var index = 0; index < allCharacters.Count; index++)
            {
                var characterPosition = _spawnedStartingTile.GetRandomCharacterSpawnPosition();
                content.Add(index, characterPosition);
            }
            
            photonView.RPC("SetupPlayersRPC", RpcTarget.All, content);
        }

        public CharacterMovementControllerSetup GetCharacterMovementControllerSetupData(int cardSetupIndex, int cardIndex)
        {
            var movementCardSettings = movementCardsSetupCollection.allSetups[cardSetupIndex].cardSet[cardIndex];
            return new CharacterMovementControllerSetup(gameCamera, movementCardSettings, tempCharacter, tilePlacer, gameHandler, timer);
        }
        
        [PunRPC]
        private void SetupPlayersRPC(Dictionary<int, object> charactersPositions)
        {
            foreach (var pair in charactersPositions)
            {
                var position = (Vector3) pair.Value;
                var spawnedCharacter = Instantiate(allCharacters[pair.Key], position, Quaternion.identity);
                gameHandler.CharacterOnTileDictionary[spawnedCharacter] = _spawnedStartingTile;
                gameHandler.CharacterFromTypeDictionary[spawnedCharacter.Type] = spawnedCharacter;
                _spawnedStartingTile.Grid.GetGridCellObject(position).SetCharacter(spawnedCharacter);
            }
        }

        [PunRPC]
        private void GetMovementCardSettingsRPC(string senderUserId)
        {
            var nextMovementCard = _runtimeMovementCardSettingsList[0];
            var cardIndex = movementCardsSetupCollection.allSetups[_cardSetupIndex].cardSet.IndexOf(nextMovementCard);
            var content = new object[] {senderUserId, _cardSetupIndex, cardIndex};
            photonView.RPC("ReceivedMovementCardSettingsRPC", RpcTarget.All, content);
            _runtimeMovementCardSettingsList.RemoveAt(0);
        }

        [PunRPC]
        private void ReceivedMovementCardSettingsRPC(string userId, int cardSetupIndex, int cardIndex)
        {
            if (userId != PhotonNetwork.LocalPlayer.UserId) return;
            
            var content = new object[] {cardSetupIndex, cardIndex};
            var spawnedController = PhotonNetwork.Instantiate("CharacterMovementController", Vector3.zero,
                Quaternion.identity, data: content);
            photonView.RPC("InstantiateMovementCardsRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber,
                cardSetupIndex, cardIndex);
            gameHandler.SetupLocalCharacterMovementController(spawnedController.GetComponent<CharacterMovementController>());
        }
        
        [PunRPC]
        private void InstantiateMovementCardsRPC(int actorNumber, int cardSetupIndex, int cardIndex)
        {
            var targetPlayerNickname = PhotonNetwork.CurrentRoom.Players[actorNumber].NickName;
            var movementCardSettings = movementCardsSetupCollection.allSetups[cardSetupIndex].cardSet[cardIndex];
            var spawnedCardContainer = Instantiate(movementCard, movementCardContainerParent);
            spawnedCardContainer.SetMovementCardSettings(targetPlayerNickname, movementCardSettings);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }
}