using LiftStudio.EventChannels;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace LiftStudio
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private GameObject endGameMenu;
        [SerializeField] private Button restartButton;

        [Space]
        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;

        private void Awake()
        {
            gameEndedEventChannel.GameEnded += OnGameEnded;
        }

        private void OnGameEnded()
        {
            endGameMenu.SetActive(true);

            restartButton.interactable = PhotonNetwork.IsMasterClient;
        }

        private void OnDestroy()
        {
            gameEndedEventChannel.GameEnded -= OnGameEnded;
        }
    }
}