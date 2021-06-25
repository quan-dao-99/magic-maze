using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace LiftStudio
{
    public class PlayerListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;

        private Player _player;

        public void Setup(Player player)
        {
            _player = player;
            playerNameText.text = player.NickName;
        }
    }
}