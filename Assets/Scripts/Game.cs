using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using LiftStudio.EventChannels;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiftStudio
{
    public class Game : MonoBehaviourPun, IOnEventCallback
    {
        [SerializeField] private TilePlacer tilePlacer;
        [SerializeField] private TileStackController tileStackController;
        [SerializeField] private Transform outOfBoardTransform;
        [SerializeField] private LayerMask groundLayerMask;

        [SerializeField] private LocalControllerSetEventChannel controllerSetEventChannel;
        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;
        [SerializeField] private PickedUpAllItemsEventChannel pickedUpAllItemsEventChannel;
        [SerializeField] private QuitGameEventChannel quitGameEventChannel;

        public Transform OutOfBoardTransform => outOfBoardTransform;

        public Dictionary<CharacterType, bool> CharactersMoving { get; } = new Dictionary<CharacterType, bool>
        {
            { CharacterType.Axe, false }, { CharacterType.Bow, false },
            { CharacterType.Potion, false }, { CharacterType.Sword, false }
        };
        public Dictionary<CharacterType, Character> CharacterFromTypeDictionary { get; } =
            new Dictionary<CharacterType, Character>();
        public Dictionary<Character, Tile> CharacterOnTileDictionary { get; } =
            new Dictionary<Character, Tile>();

        public bool HasCharactersBeenOnPickupCells { get; private set; }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Update()
        {
            if (!Input.GetKeyUp(KeyCode.Escape)) return;

            quitGameEventChannel.RaiseEvent();
        }
        
        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code >= 200) return;
            
            if (photonEvent.Code != (int) PhotonEventCodes.RestartGameCode) return;

            switch (photonEvent.Code)
            {
                case (int) PhotonEventCodes.RestartGameCode:
                    PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex);
                    break;
            }
        }

        public void HandleTakeNewTile()
        {
            photonView.RPC("TryTakeNewTileRPC", RpcTarget.MasterClient);
        }

        public void NotifyCharacterPlacedOnPickupCell(CharacterType movingCharacterType, Transform tempCharacterTransform)
        {
            if (HasCharactersBeenOnPickupCells) return;

            var content = new object[] {movingCharacterType, tempCharacterTransform.position};
            photonView.RPC("NotifyCharacterPlacedOnPickupCellRPC", RpcTarget.MasterClient, content);
        }

        public void NotifyTakeCharacterOutOfBoard(Character targetCharacter)
        {
            CharacterOnTileDictionary[targetCharacter] = null;
            var allCharactersOutOfBoard = CharacterOnTileDictionary.Values.All(tile => tile == null);
            if (allCharactersOutOfBoard)
            {
                gameEndedEventChannel.RaiseEvent();
            }
        }
        
        public void SetupLocalCharacterMovementController(CharacterMovementController controller)
        {
            controllerSetEventChannel.RaiseEvent(controller);
        }
        
        private void SendConfirmCharacterResearchEvent(KeyValuePair<Character, Tile> pair, bool shouldPlaceNewTile)
        {
            var content = new object[] {pair.Key.Type, shouldPlaceNewTile};
            photonView.RPC("ConfirmCharacterResearchRPC", RpcTarget.Others, content);
        }

        [PunRPC]
        private void TryTakeNewTileRPC()
        {
            foreach (var pair in CharacterOnTileDictionary)
            {
                var characterGridCell = pair.Value.Grid.GetGridCellObject(pair.Key.transform.position);
                var gridCellResearchPoint = characterGridCell.ResearchPoint;
                if (gridCellResearchPoint == null) continue;

                if (gridCellResearchPoint.hasResearched) continue;

                var attachPoint = gridCellResearchPoint.attachPoint;
                if (Physics.CheckBox(
                    attachPoint.position + attachPoint.forward * 2f, new Vector3(1, 0, 1) / 4,
                    Quaternion.identity, groundLayerMask))
                {
                    gridCellResearchPoint.hasResearched = true;
                    SendConfirmCharacterResearchEvent(pair, false);
                    continue;
                }

                if (gridCellResearchPoint.targetCharacterType != pair.Key.Type) continue;

                tilePlacer.PlaceTile(tileStackController.GameTileStacks.Pop(),
                    attachPoint.position,
                    Quaternion.LookRotation(attachPoint.forward));
                gridCellResearchPoint.hasResearched = true;
                Physics.SyncTransforms();
                SendConfirmCharacterResearchEvent(pair, true);
            }
        }

        [PunRPC]
        private void ConfirmCharacterResearchRPC(CharacterType characterType, bool shouldPlaceNewTile)
        {
            foreach (var pair in CharacterOnTileDictionary)
            {
                if (pair.Key.Type != characterType) continue;
                
                var characterGridCell = pair.Value.Grid.GetGridCellObject(pair.Key.transform.position);
                var gridCellResearchPoint = characterGridCell.ResearchPoint;
                
                if (gridCellResearchPoint.hasResearched) return;
                
                gridCellResearchPoint.hasResearched = true;
                
                if (!shouldPlaceNewTile) return;
                
                var attachPoint = gridCellResearchPoint.attachPoint;
                tilePlacer.PlaceTile(tileStackController.GameTileStacks.Pop(),
                    attachPoint.position,
                    Quaternion.LookRotation(attachPoint.forward));
                Physics.SyncTransforms();
            }
        }
        
        [PunRPC]
        private void NotifyCharacterPlacedOnPickupCellRPC(CharacterType characterType, Vector3 tempCharacterPosition)
        {
            if (CharactersMoving.Any(pair =>
                pair.Key != characterType && pair.Value))
            {
                HasCharactersBeenOnPickupCells = false;
                return;
            }

            var allCharacterOnPickupCells = true;
            foreach (var pair in CharacterOnTileDictionary)
            {
                var tile = pair.Value;
                var character = pair.Key;
                var targetCharacterPosition = character.transform.position;
                var finalCharacterPosition = targetCharacterPosition.y > 0f
                    ? tempCharacterPosition
                    : targetCharacterPosition;
                var characterGridCell = tile.Grid.GetGridCellObject(finalCharacterPosition);
                if (characterGridCell.Pickup == null ||
                    characterGridCell.Pickup.TargetCharacterType != character.Type)
                {
                    allCharacterOnPickupCells = false;
                }
            }

            HasCharactersBeenOnPickupCells = allCharacterOnPickupCells;
            if (!HasCharactersBeenOnPickupCells) return;
            
            photonView.RPC("ConfirmAllCharactersBeenOnPickupCellsRPC", RpcTarget.Others);
            pickedUpAllItemsEventChannel.RaiseEvent();
        }

        [PunRPC]
        private void ConfirmAllCharactersBeenOnPickupCellsRPC()
        {
            HasCharactersBeenOnPickupCells = true;
            pickedUpAllItemsEventChannel.RaiseEvent();
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }
}