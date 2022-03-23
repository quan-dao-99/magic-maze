using System;
using UnityEngine;

namespace LiftStudio.EventChannels
{
    [CreateAssetMenu(fileName = "CameraRotatedEventChannel", menuName = "Events/CameraRotatedEventChannel")]
    public class CameraRotatedEventChannel : ScriptableObject
    {
        [SerializeField] private int rotationThreshold;

        public event Action<int> CameraRotated;

        public void RaiseEvent(int rotationAmount)
        {
            if (rotationAmount % rotationThreshold != 0) return;
            
            CameraRotated?.Invoke(rotationAmount);
        }
    }
}