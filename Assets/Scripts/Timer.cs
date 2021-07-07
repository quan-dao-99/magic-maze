using LiftStudio.EventChannels;
using UnityEngine;

namespace LiftStudio
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private float maxTime;
        [SerializeField] private TimerUI timerUI;

        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;

        public float CurrentTime { get; private set; }

        private void Awake()
        {
            CurrentTime = maxTime;
            timerUI.UpdateTimer(CurrentTime / maxTime);

            gameEndedEventChannel.GameEnded += OnGameEnded;
        }

        private void Update()
        {
            CurrentTime = Mathf.Clamp(CurrentTime - Time.deltaTime, 0f, maxTime);
            timerUI.UpdateTimer(CurrentTime / maxTime);

            if (CurrentTime == 0)
            {
                gameEndedEventChannel.RaiseEvent();
            }
        }

        public void FlipHourglassTimer()
        {
            CurrentTime = maxTime - CurrentTime;
            FlipHourglassTimerUI();
        }

        public void FlipHourglassTimer(float flippedTime)
        {
            CurrentTime = maxTime - flippedTime;
            FlipHourglassTimerUI();
        }
        
        private void FlipHourglassTimerUI()
        {
            timerUI.UpdateTimer(CurrentTime / maxTime);
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