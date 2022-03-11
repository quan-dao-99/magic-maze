using LiftStudio.EventChannels;
using UnityEngine;

namespace LiftStudio
{
    public class MovementDirectionArrow : MonoBehaviour
    {
        [SerializeField] private CameraRotatedEventChannel cameraRotatedEventChannel;

        private Quaternion _arrowNewRotation;
        private float _rotationSpeed;

        private void Awake()
        {
            cameraRotatedEventChannel.CameraRotated += OnCameraRotated;
            _arrowNewRotation = transform.rotation;
        }

        private void Update()
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, _arrowNewRotation, Time.deltaTime * _rotationSpeed);
        }

        private void OnCameraRotated(float rotationAmount, float rotationSpeed)
        {
            _arrowNewRotation *= Quaternion.Euler(Vector3.forward * rotationAmount);
            _rotationSpeed = rotationSpeed;
        }

        private void OnDestroy()
        {
            cameraRotatedEventChannel.CameraRotated -= OnCameraRotated;
        }
    }
}