using UnityEngine;
using UnityEngine.UI;

namespace LiftStudio
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private Image timerImage;

        public void UpdateTimer(float timerPercentage)
        {
            timerImage.fillAmount = timerPercentage;
        }
    }
}