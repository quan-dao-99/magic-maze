using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace LiftStudio
{
    public class GameSetup : MonoBehaviourPun
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private MovementCardSettings movementCardSettings;
        [SerializeField] private Transform tempCharacter;
        [SerializeField] private Timer timer;
        [SerializeField] private TilePlacer tilePlacer;
        [SerializeField] private TileStackController tileStackController;
        [SerializeField] private Game gameHandler;
        [SerializeField] private List<Character> allCharacters;
        [SerializeField] private StartingTile startingTile;
        [SerializeField] private Transform startTilePositionTransform;

        public static GameSetup Instance;

        private StartingTile _spawnedStartingTile;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (Instance == this) return;
            
            Destroy(Instance);
            Instance = this;
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

            var spawnedController = PhotonNetwork.Instantiate("CharacterMovementController", Vector3.zero, Quaternion.identity);
            gameHandler.SetupLocalCharacterMovementController(spawnedController.GetComponent<CharacterMovementController>());
            
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

        public CharacterMovementControllerSetup GetCharacterMovementControllerSetupData()
        {
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
                gameHandler.CharacterFromTypeDictionary[spawnedCharacter.CharacterType] = spawnedCharacter;
                _spawnedStartingTile.Grid.GetGridCellObject(position).SetCharacter(spawnedCharacter);
            }
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }
}