using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LiftStudio
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private Image timerImage;
        [SerializeField] private Transform hourglassIconTransform;
        [SerializeField] private float rotateSpeed;

        private Quaternion HourglassRotation => hourglassIconTransform.rotation;

        public void UpdateTimer(float timerPercentage)
        {
            timerImage.fillAmount = timerPercentage;
        }

        public void FlipHourglassTimer()
        {
            StopAllCoroutines();
            StartCoroutine(RotateHourglass());
        }

        private IEnumerator RotateHourglass()
        {
            var targetRotationDegree = HourglassRotation.eulerAngles.z == 0 ? 180f : 0f;
            while (Math.Abs(HourglassRotation.eulerAngles.z - targetRotationDegree) > 0.001f)
            {
                var step = rotateSpeed * Time.deltaTime;
                hourglassIconTransform.rotation = Quaternion.RotateTowards(HourglassRotation,
                    Quaternion.Euler(0f, 0f, targetRotationDegree), step);
                yield return null;
            }
        }
    }
}