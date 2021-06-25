using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace LiftStudio
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        [SerializeField] private byte maxPlayersPerRoom = 4;
        [SerializeField] private GameObject controlPanel;
        [SerializeField] private GameObject progressLabel;
        [SerializeField] private RoomListItem roomListItemPrefab;
        [SerializeField] private Transform roomInfosContainer;
        [SerializeField] private TMP_InputField roomNameInputField;

        private readonly Dictionary<string, RoomListItem> _cachedRoomList = new Dictionary<string, RoomListItem>();

        private const string GameVersion = "1";
        private bool _isConnecting;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);

            Connect();
        }

        public void CreateRoom()
        {
            if (string.IsNullOrEmpty(PhotonNetwork.NickName) || string.IsNullOrEmpty(roomNameInputField.text)) return;

            PhotonNetwork.CreateRoom(roomNameInputField.text, new RoomOptions {MaxPlayers = maxPlayersPerRoom});
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

            if (!_isConnecting) return;

            _isConnecting = false;
            PhotonNetwork.JoinLobby();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            _isConnecting = false;
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            _cachedRoomList.Clear();
            Debug.LogWarning($"PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {cause}");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        }

        public override void OnJoinedLobby()
        {
            _cachedRoomList.Clear();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            UpdateCachedRoomList(roomList);
        }

        public override void OnLeftLobby()
        {
            _cachedRoomList.Clear();
        }

        private void Connect()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinLobby();
                return;
            }

            _isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = GameVersion;
        }

        private void UpdateCachedRoomList(IEnumerable<RoomInfo> roomList)
        {
            foreach (var info in roomList)
            {
                if (info.RemovedFromList)
                {
                    Destroy(_cachedRoomList[info.Name].gameObject);
                    _cachedRoomList.Remove(info.Name);
                }
                else
                {
                    var spawnedRoomListItem = Instantiate(roomListItemPrefab, roomInfosContainer);
                    spawnedRoomListItem.Setup(info);
                    _cachedRoomList[info.Name] = spawnedRoomListItem;
                }
            }
        }
    }
}