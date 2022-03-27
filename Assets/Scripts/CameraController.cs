using Cinemachine;
using LiftStudio.EventChannels;
using UnityEngine;

namespace LiftStudio
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Collider limitCollider;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;
        [SerializeField] private CameraRotatedEventChannel cameraRotatedEventChannel;

        [Space]
        [SerializeField] private float normalSpeed;
        [SerializeField] private float fastSpeed;
        [SerializeField] private float movementTime;
        [SerializeField] private float rotationAmount;

        private float _movementSpeed;
        private Vector3 _newPosition;
        private Vector3 _newZoom;
        private float _cameraVerticalAngle;
        private float _cameraHorizontalAngle;

        private Bounds ColliderBounds => limitCollider.bounds;
        private Transform OwnTransform => transform;
        private Transform VirtualCameraTransform => virtualCamera.transform;

        private void Awake()
        {
            _newPosition = OwnTransform.position;
            _newZoom = virtualCamera.transform.position;
            _cameraVerticalAngle = Quaternion.Angle(VirtualCameraTransform.localRotation, Quaternion.identity);

            gameEndedEventChannel.GameEnded += OnGameEnded;
        }

        private void Update()
        {
            HandleMovementInput();

            HandleCameraZoom();
        }

        private void HandleMovementInput()
        {
            HandleCameraMovement();

            HandleCameraRotation();

            OwnTransform.position = Vector3.Lerp(OwnTransform.position, _newPosition, Time.deltaTime * movementTime);
        }

        private void HandleCameraMovement()
        {
            _movementSpeed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : normalSpeed;

            if (Input.GetAxisRaw("Horizontal") > 0f)
            {
                _newPosition += OwnTransform.right * (Time.deltaTime * _movementSpeed);
            }
            else if (Input.GetAxisRaw("Horizontal") < 0f)
            {
                _newPosition += -OwnTransform.right * (Time.deltaTime * _movementSpeed);
            }

            if (Input.GetAxisRaw("Vertical") > 0f)
            {
                _newPosition += OwnTransform.forward * (Time.deltaTime * _movementSpeed);
            }
            else if (Input.GetAxisRaw("Vertical") < 0f)
            {
                _newPosition -= OwnTransform.forward * (Time.deltaTime * _movementSpeed);
            }

            _newPosition.x = Mathf.Clamp(_newPosition.x, ColliderBounds.min.x, ColliderBounds.max.x);
            _newPosition.z = Mathf.Clamp(_newPosition.z, ColliderBounds.min.z, ColliderBounds.max.z);
        }

        private void HandleCameraRotation()
        {
            if (!Input.GetMouseButton(1)) return;

            var mouseHorizontal = Input.GetAxis("Mouse X");
            var mouseVertical = Input.GetAxis("Mouse Y");

            var rotation = mouseHorizontal * rotationAmount;
            _cameraHorizontalAngle += rotation;
            OwnTransform.Rotate(new Vector3(0f, rotation, 0f), Space.Self);
            cameraRotatedEventChannel.RaiseEvent((int) _cameraHorizontalAngle);

            _cameraVerticalAngle += mouseVertical * rotationAmount;
            _cameraVerticalAngle = Mathf.Clamp(_cameraVerticalAngle, 30, 89f);
            VirtualCameraTransform.localEulerAngles = new Vector3(_cameraVerticalAngle, 0, 0);
        }

        private void HandleCameraZoom()
        {
            if (Input.mouseScrollDelta.y == 0f) return;

            var scrollDirection = (int)Mathf.Sign(Input.mouseScrollDelta.y);
            var newZoomPosition = VirtualCameraTransform.position + VirtualCameraTransform.forward * scrollDirection;
            newZoomPosition.y = Mathf.Clamp(newZoomPosition.y, 2f, 10f);
            Debug.Log($"GAME: New zoom position {newZoomPosition}");
            if (Mathf.Approximately(newZoomPosition.y, 2f) || Mathf.Approximately(newZoomPosition.y, 10f)) return;

            VirtualCameraTransform.position = newZoomPosition;
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