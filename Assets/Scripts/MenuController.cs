using UnityEngine;

namespace LiftStudio
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private GameObject endGameMenu;

        [Space]
        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;

        private void Awake()
        {
            gameEndedEventChannel.GameEnded += OnGameEnded;
        }

        private void OnGameEnded()
        {
            endGameMenu.SetActive(true);
        }

        private void OnDestroy()
        {
            gameEndedEventChannel.GameEnded -= OnGameEnded;
        }
    }
}