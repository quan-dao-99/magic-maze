using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiftStudio
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;
        
        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

        private void Start()
        {
            Instance = this;
            
            if (PlayerManager.LocalPlayerInstance != null) return;
            
            Debug.Log($"We are Instantiating LocalPlayer from {SceneManager.GetActiveScene().name}");
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0f,5f,0f), Quaternion.identity);
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("Launcher");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"OnPlayerEnteredRoom() {newPlayer.NickName}"); // not seen if you're the player connecting

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log(
                    $"OnPlayerEnteredRoom IsMasterClient {PhotonNetwork.IsMasterClient}"); // called before OnPlayerLeftRoom
                
                LoadArena();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom() {otherPlayer.NickName}"); // seen when other disconnects

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"OnPlayerLeftRoom IsMasterClient {PhotonNetwork.IsMasterClient}"); // called before OnPlayerLeftRoom
                
                LoadArena();
            }
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        private static void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }

            Debug.Log($"PhotonNetwork : Loading Level : {PhotonNetwork.CurrentRoom.PlayerCount}");
            PhotonNetwork.LoadLevel($"Room for {PhotonNetwork.CurrentRoom.PlayerCount}");
        }
    }
}