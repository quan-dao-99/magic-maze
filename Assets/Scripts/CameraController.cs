using Cinemachine;
using UnityEngine;

namespace LiftStudio
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Collider limitCollider;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        [SerializeField] private GameEndedEventChannel gameEndedEventChannel;

        [Space]
        [SerializeField] private float normalSpeed;
        [SerializeField] private float fastSpeed;
        [SerializeField] private float movementTime;
        [SerializeField] private float rotationAmount;

        private float _movementSpeed;
        private Vector3 _newPosition;
        private Quaternion _newRotation;
        private Vector3 _newZoom;

        private Bounds ColliderBounds => limitCollider.bounds;
        private Transform OwnTransform => transform;
        private Transform VirtualCameraTransform => virtualCamera.transform;

        private void Awake()
        {
            _newPosition = OwnTransform.position;
            _newRotation = OwnTransform.rotation;
            _newZoom = virtualCamera.transform.position;

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
            OwnTransform.rotation = Quaternion.Lerp(OwnTransform.rotation, _newRotation, Time.deltaTime * movementTime);
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
            if (Input.GetKey(KeyCode.Q))
            {
                _newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                _newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
            }
        }

        private void HandleCameraZoom()
        {
            if (Input.mouseScrollDelta.y == 0f) return;

            var scrollDirection = (int) Mathf.Sign(Input.mouseScrollDelta.y);
            var distanceFromBase = (_newZoom - transform.position).sqrMagnitude;

            if (distanceFromBase <= 10f && scrollDirection == 1) return;
            if (distanceFromBase >= 250f && scrollDirection == -1) return;

            _newZoom = VirtualCameraTransform.position;
            _newZoom += VirtualCameraTransform.forward * scrollDirection;
            VirtualCameraTransform.position = _newZoom;
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