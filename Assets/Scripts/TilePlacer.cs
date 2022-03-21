using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    public class TilePlacer : MonoBehaviour
    {
        public List<Tile> AllPlacedTiles { get; private set; }

        public const int CellSize = 1;
        
        private const int Width = 4;
        private const int Height = 4;

        private void Awake()
        {
            AllPlacedTiles = new List<Tile>();
        }

        public void PlaceTile(Tile tileToPlace, Vector3 placePosition, Quaternion placeRotation)
        {
            AllPlacedTiles.Add(tileToPlace);
            var tileTransform = tileToPlace.transform;
            tileTransform.position = placePosition;
            tileTransform.rotation = placeRotation;
            tileToPlace.SetupGrid(Width, Height, CellSize);
        }
    }
}