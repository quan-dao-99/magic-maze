using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace LiftStudio
{
    public class TileStackController : MonoBehaviour, IOnEventCallback
    {
        [SerializeField] private List<Tile> allTiles;
        [SerializeField] private Transform tileStackSpawnPosition;

        public Stack<Tile> GameTileStacks { get; } = new Stack<Tile>();
        
        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void SetupTileStacks()
        {
            var allTilesCopy = new List<Tile>(allTiles);
            var content = new List<int>();
            while (allTilesCopy.Count != 0)
            {
                var randomTileIndex = Random.Range(0, allTilesCopy.Count);
                var targetTileIndex = allTiles.IndexOf(allTilesCopy[randomTileIndex]);
                content.Add(targetTileIndex);
                allTilesCopy.RemoveAt(randomTileIndex);
            }

            PhotonNetwork.RaiseEvent((int) PhotonEventCodes.SetupTileStacksCode, content.ToArray(),
                RaiseEventOptionsHelper.All,
                SendOptions.SendReliable);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code >= 200) return;
            
            if (photonEvent.Code != (int) PhotonEventCodes.SetupTileStacksCode) return;

            var indexesArray = (int[]) photonEvent.CustomData;
            var nextTileVerticalPosition = tileStackSpawnPosition.position;
            foreach (var randomTileIndex in indexesArray)
            {
                var tile = Instantiate(allTiles[randomTileIndex], nextTileVerticalPosition,
                    Quaternion.Euler(180, 0, 0));
                GameTileStacks.Push(tile);
                nextTileVerticalPosition.y += 0.5f;
            }
        }
        
        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }
}