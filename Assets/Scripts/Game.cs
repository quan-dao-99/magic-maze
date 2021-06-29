using System;
using System.Collections.Generic;
using System.Linq;
using LiftStudio.EventChannels;
using UnityEngine;

namespace LiftStudio
{
    public class Game : MonoBehaviour
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

        private void Update()
        {
            if (!Input.GetKeyUp(KeyCode.Escape)) return;

            quitGameEventChannel.RaiseEvent();
        }

        public void HandleTakeNewTile()
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
                    continue;
                }

                if (gridCellResearchPoint.targetCharacterType != pair.Key.CharacterType) continue;

                TilePlacer.PlaceTile(tileStackController.GameTileStacks.Pop(),
                    attachPoint.position,
                    Quaternion.LookRotation(attachPoint.forward));
                gridCellResearchPoint.hasResearched = true;
                Physics.SyncTransforms();
            }
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
    }
}