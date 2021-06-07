using TMPro;
using UnityEngine;

namespace LiftStudio
{
    public class CurrentGoal : MonoBehaviour
    {
        [SerializeField] private TMP_Text currentGoalText;

        private void Awake()
        {
            Game.AllItemsPickedUp += OnAllItemsPickedUp;
        }

        private void OnAllItemsPickedUp()
        {
            currentGoalText.text = "Run to exit tiles";
        }

        private void OnDestroy()
        {
            Game.AllItemsPickedUp -= OnAllItemsPickedUp;
        }
    }
}