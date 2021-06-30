using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using LiftStudio.EventChannels;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace LiftStudio
{
    public class Game : MonoBehaviour, IOnEventCallback
    {
        [SerializeField] private TileStackController tileStackController;
        [SerializeField] private Transform outOfBoardTransform;
        [SerializeField] private LayerMask groundLayerMask;

        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;
        [SerializeField] private PickedUpAllItemsEventChannel pickedUpAllItemsEventChannel;
        [SerializeField] private QuitGameEventChannel quitGameEventChannel;

        public Transform OutOfBoardTransform => outOfBoardTransform;

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

            if (photonEvent.Code != (int) PhotonEventCodes.TryTakeNewTiles &&
                photonEvent.Code != (int) PhotonEventCodes.ConfirmCharacterResearch) return;

            switch (photonEvent.Code)
            {
                case (int) PhotonEventCodes.TryTakeNewTiles:
                    HandleTryTakeNewTile();
                    break;
                case (int) PhotonEventCodes.ConfirmCharacterResearch:
                    var data = (object[]) photonEvent.CustomData;
                    HandleConfirmCharacterResearch(data);
                    break;
            }
        }

        public void HandleTakeNewTile()
        {
            PhotonNetwork.RaiseEvent((int) PhotonEventCodes.TryTakeNewTiles, null, RaiseEventOptionsHelper.MasterClient,
                SendOptions.SendReliable);
        }

        public void NotifyCharacterPlacedOnPickupCell(Transform tempCharacterTransform)
        {
            if (HasCharactersBeenOnPickupCells) return;

            var allCharacterOnPickupCells = true;
            foreach (var pair in CharacterOnTileDictionary)
            {
                var tile = pair.Value;
                var character = pair.Key;
                var targetCharacterPosition = character.transform.position;
                var finalCharacterPosition = Math.Abs(targetCharacterPosition.y - 0.5f) < 0.01f
                    ? tempCharacterTransform.position
                    : targetCharacterPosition;
                var characterGridCell = tile.Grid.GetGridCellObject(finalCharacterPosition);
                if (characterGridCell.Pickup == null ||
                    characterGridCell.Pickup.targetCharacterType != character.CharacterType)
                {
                    allCharacterOnPickupCells = false;
                }
            }

            HasCharactersBeenOnPickupCells = allCharacterOnPickupCells;
            if (HasCharactersBeenOnPickupCells)
            {
                pickedUpAllItemsEventChannel.RaiseEvent();
            }
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
        
        private static void SendConfirmCharacterResearchEvent(KeyValuePair<Character, Tile> pair, bool shouldPlaceNewTile)
        {
            var content = new object[] {pair.Key.CharacterType, shouldPlaceNewTile};
            PhotonNetwork.RaiseEvent((int) PhotonEventCodes.ConfirmCharacterResearch, content,
                RaiseEventOptionsHelper.Others, SendOptions.SendReliable);
        }

        private void HandleTryTakeNewTile()
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

                if (gridCellResearchPoint.targetCharacterType != pair.Key.CharacterType) continue;

                TilePlacer.PlaceTile(tileStackController.GameTileStacks.Pop(),
                    attachPoint.position,
                    Quaternion.LookRotation(attachPoint.forward));
                gridCellResearchPoint.hasResearched = true;
                Physics.SyncTransforms();
                SendConfirmCharacterResearchEvent(pair, true);
            }
        }

        private void HandleConfirmCharacterResearch(IReadOnlyList<object> data)
        {
            foreach (var pair in CharacterOnTileDictionary)
            {
                if (pair.Key.CharacterType != (CharacterType) data[0]) continue;
                
                var characterGridCell = pair.Value.Grid.GetGridCellObject(pair.Key.transform.position);
                var gridCellResearchPoint = characterGridCell.ResearchPoint;
                
                if (gridCellResearchPoint.hasResearched) return;

                var shouldPlaceNewTile = (bool) data[1];
                gridCellResearchPoint.hasResearched = true;
                
                if (!shouldPlaceNewTile) return;
                
                var attachPoint = gridCellResearchPoint.attachPoint;
                TilePlacer.PlaceTile(tileStackController.GameTileStacks.Pop(),
                    attachPoint.position,
                    Quaternion.LookRotation(attachPoint.forward));
                Physics.SyncTransforms();
            }
        }
        
        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }
}