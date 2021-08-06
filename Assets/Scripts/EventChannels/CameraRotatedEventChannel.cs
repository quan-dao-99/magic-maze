using System;
using UnityEngine;

namespace LiftStudio.EventChannels
{
    [CreateAssetMenu(fileName = "CameraRotatedEventChannel", menuName = "Events/CameraRotatedEventChannel")]
    public class CameraRotatedEventChannel : ScriptableObject
    {
        public event Action<float, float> CameraRotated;

        public void RaiseEvent(float rotationAmount, float rotateTime)
        {
            CameraRotated?.Invoke(rotationAmount, rotateTime);
        }
    }
}