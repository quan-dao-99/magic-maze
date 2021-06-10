using TMPro;
using UnityEngine;

namespace LiftStudio
{
    public class GoalChecklist : MonoBehaviour
    {
        [SerializeField] private TMP_Text pickupAllItemsText;
        [SerializeField] private TMP_Text runToExitsText;

        private void Awake()
        {
            Game.AllItemsPickedUp += OnAllItemsPickedUp;
            Game.AllCharactersOutOfBoard += OnAllCharactersOutOfBoard;
        }

        private void OnAllItemsPickedUp()
        {
            pickupAllItemsText.fontStyle = FontStyles.Strikethrough;
        }

        private void OnAllCharactersOutOfBoard()
        {
            runToExitsText.fontStyle = FontStyles.Strikethrough;
        }

        private void OnDestroy()
        {
            Game.AllItemsPickedUp -= OnAllItemsPickedUp;
            Game.AllCharactersOutOfBoard -= OnAllCharactersOutOfBoard;
        }
    }
}