using UnityEngine;

namespace LiftStudio
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private float maxTime;
        [SerializeField] private TimerUI timerUI;

        private float _currentTime;

        private void Awake()
        {
            _currentTime = maxTime;
            timerUI.UpdateTimer(_currentTime / maxTime);
        }

        private void Update()
        {
            _currentTime = Mathf.Clamp(_currentTime - Time.deltaTime, 0f, maxTime);
            timerUI.UpdateTimer(_currentTime / maxTime);
        }

        public void FlipHourglassTimer()
        {
            _currentTime = maxTime - _currentTime;
            timerUI.UpdateTimer(_currentTime / maxTime);
            timerUI.FlipHourglassTimer();
        }
    }
}