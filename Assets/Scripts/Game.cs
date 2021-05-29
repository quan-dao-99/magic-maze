using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private TileStackController tileStackController;
        [SerializeField] private Transform outOfBoardTransform;
        [SerializeField] private LayerMask groundLayerMask;

        public Transform OutOfBoardTransform => outOfBoardTransform;

        public Dictionary<Character, Tile> CharacterOnTileDictionary { get; } = new Dictionary<Character, Tile>();
        public bool HasCharactersBeenOnPickupCells { get; private set; }

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
                    attachPoint.position + attachPoint.forward * 2f, Vector3.one,
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

        public void NotifyCharacterPlacedOnPickupCell()
        {
            if (HasCharactersBeenOnPickupCells) return;

            var allCharacterOnPickupCells = true;
            foreach (var pair in CharacterOnTileDictionary)
            {
                var tile = pair.Value;
                var character = pair.Key;
                var characterGridCell = tile.Grid.GetGridCellObject(character.transform.position);
                if (characterGridCell.Pickup == null ||
                    characterGridCell.Pickup.targetCharacterType != character.CharacterType)
                {
                    allCharacterOnPickupCells = false;
                }
            }

            HasCharactersBeenOnPickupCells = allCharacterOnPickupCells;
        }
    }
}