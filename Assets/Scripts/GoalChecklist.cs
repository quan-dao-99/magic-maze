using TMPro;
using UnityEngine;

namespace LiftStudio
{
    public class GoalChecklist : MonoBehaviour
    {
        [SerializeField] private TMP_Text pickupAllItemsText;
        [SerializeField] private TMP_Text runToExitsText;

        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;
        [SerializeField] private PickedUpAllItemsEventChannel pickedUpAllItemsEventChannel;

        private void Awake()
        {
            pickedUpAllItemsEventChannel.AllItemsPickedUp += OnAllItemsPickedUp;
            gameEndedEventChannel.GameEnded += OnGameEnded;
        }

        private void OnAllItemsPickedUp()
        {
            pickupAllItemsText.fontStyle = FontStyles.Strikethrough;
        }

        private void OnGameEnded()
        {
            runToExitsText.fontStyle = FontStyles.Strikethrough;
        }

        private void OnDestroy()
        {
            pickedUpAllItemsEventChannel.AllItemsPickedUp -= OnAllItemsPickedUp;
            gameEndedEventChannel.GameEnded -= OnGameEnded;
        }
    }
}