using LiftStudio.EventChannels;
using UnityEngine;

namespace LiftStudio
{
    public class MovementDirectionArrow : MonoBehaviour
    {
        [SerializeField] private CameraRotatedEventChannel cameraRotatedEventChannel;

        private int _lastRotationAmount;

        private void Awake()
        {
            cameraRotatedEventChannel.CameraRotated += OnCameraRotated;
        }

        private void OnCameraRotated(int rotationAmount)
        {
            if (_lastRotationAmount == rotationAmount) return;

            transform.Rotate(new Vector3(0f, 0f, rotationAmount - _lastRotationAmount), Space.Self);
            _lastRotationAmount = rotationAmount;
        }

        private void OnDestroy()
        {
            cameraRotatedEventChannel.CameraRotated -= OnCameraRotated;
        }
    }
}