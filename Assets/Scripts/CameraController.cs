using System;
using Cinemachine;
using UnityEngine;

namespace LiftStudio
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Collider limitCollider;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        [Space] [SerializeField] private float normalSpeed;
        [SerializeField] private float fastSpeed;
        [SerializeField] private float movementTime;
        [SerializeField] private float rotationAmount;

        private float _movementSpeed;
        private Bounds _colliderBounds;
        private Vector3 _newPosition;
        private Quaternion _newRotation;
        private Vector3 _newZoom;
        private CinemachineTransposer _transposer;

        private Transform OwnTransform => transform;

        private void Awake()
        {
            _transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            _colliderBounds = limitCollider.bounds;
            _newPosition = OwnTransform.position;
            _newRotation = OwnTransform.rotation;
            _newZoom = _transposer.m_FollowOffset;
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

            _newPosition.x = Mathf.Clamp(_newPosition.x, _colliderBounds.min.x, _colliderBounds.max.x);
            _newPosition.z = Mathf.Clamp(_newPosition.z, _colliderBounds.min.z, _colliderBounds.max.z);
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

            Debug.Log(virtualCamera.transform.forward);
            _newZoom += virtualCamera.transform.forward * Mathf.Sign(Input.mouseScrollDelta.y);
            Debug.Log($"Target new zoom: {_newZoom}");
            _newZoom.y = Mathf.Clamp(_newZoom.y, _colliderBounds.min.y, _colliderBounds.max.y);
            Debug.Log($"Fixed new zoom: {_newZoom}");
            _transposer.m_FollowOffset =
                Vector3.Lerp(_transposer.m_FollowOffset, _newZoom, Time.deltaTime * movementTime);
        }
    }
}