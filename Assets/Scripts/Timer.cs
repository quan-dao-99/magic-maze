using LiftStudio.EventChannels;
using UnityEngine;

namespace LiftStudio
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private float maxTime;
        [SerializeField] private TimerUI timerUI;

        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;

        private float _currentTime;

        private void Awake()
        {
            _currentTime = maxTime;
            timerUI.UpdateTimer(_currentTime / maxTime);

            gameEndedEventChannel.GameEnded += OnGameEnded;
        }

        private void Update()
        {
            _currentTime = Mathf.Clamp(_currentTime - Time.deltaTime, 0f, maxTime);
            timerUI.UpdateTimer(_currentTime / maxTime);

            if (_currentTime == 0)
            {
                gameEndedEventChannel.RaiseEvent();
            }
        }

        public void FlipHourglassTimer()
        {
            _currentTime = maxTime - _currentTime;
            timerUI.UpdateTimer(_currentTime / maxTime);
            timerUI.FlipHourglassTimer();
        }

        private void OnGameEnded()
        {
            enabled = false;
        }

        private void OnDestroy()
        {
            gameEndedEventChannel.GameEnded -= OnGameEnded;
        }
    }
}