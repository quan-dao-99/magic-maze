using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    public class TilePlacer : MonoBehaviour
    {
        public static List<Tile> AllPlacedTiles { get; private set; }

        private void Awake()
        {
            AllPlacedTiles = new List<Tile>();
        }

        public static void PlaceTile(Tile tileToPlace, Vector3 placePosition, Quaternion placeRotation)
        {
            AllPlacedTiles.Add(tileToPlace);
            var tileTransform = tileToPlace.transform;
            tileTransform.position = placePosition;
            tileTransform.rotation = placeRotation;
            tileToPlace.SetupGrid();
        }
    }
}