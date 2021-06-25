using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace LiftStudio
{
    public class RoomListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomNameText;
        [SerializeField] private TMP_Text playerCountText;

        private RoomInfo _roomInfo;

        public void Setup(RoomInfo roomInfo)
        {
            _roomInfo = roomInfo;
            roomNameText.text = roomInfo.Name;
            playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        }

        public void JoinRoom()
        {
            if (string.IsNullOrEmpty(PhotonNetwork.NickName)) return;
            
            PhotonNetwork.JoinRoom(_roomInfo.Name);
        }
    }
}