using UnityEngine;

namespace LiftStudio
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;

        [Space] [SerializeField] private float normalSpeed;
        [SerializeField] private float fastSpeed;
        [SerializeField] private float movementTime;
        [SerializeField] private float rotationAmount;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector3 zoomAmount;

        private float _movementSpeed;
        private Vector3 _newPosition;
        private Quaternion _newRotation;
        private Vector3 _newZoom;
        private Vector3 _dragStartPosition;
        private Vector3 _dragCurrentPosition;
        private Vector3 _rotateStartPosition;
        private Vector3 _rotateCurrentPosition;

        private Plane _plane = new Plane(Vector3.up, Vector3.zero);

        private Transform OwnTransform => transform;

        private void Awake()
        {
            _newPosition = OwnTransform.position;
            _newRotation = OwnTransform.rotation;
            _newZoom = cameraTransform.localPosition;
        }

        private void Update()
        {
            // HandleMouseInput();

            HandleMovementInput();

            HandleCameraZoom();
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
                if (_plane.Raycast(ray, out var entry))
                {
                    _dragStartPosition = ray.GetPoint(entry);
                }
            }
            else if (Input.GetMouseButton(0))
            {
                var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
                if (_plane.Raycast(ray, out var entry))
                {
                    _dragCurrentPosition = ray.GetPoint(entry);

                    _newPosition = OwnTransform.position + _dragStartPosition - _dragCurrentPosition;
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                _rotateStartPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(1))
            {
                _rotateCurrentPosition = Input.mousePosition;

                var difference = _rotateStartPosition - _rotateCurrentPosition;

                _rotateStartPosition = _rotateCurrentPosition;

                _newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
            }
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

            // if (Input.mousePosition.x > Screen.width - 20f || Input.GetAxisRaw("Horizontal") > 0f)
            // {
            //     _newPosition += OwnTransform.right * (Time.deltaTime * _movementSpeed);
            // }
            // else if (Input.mousePosition.x < 20f || Input.GetAxisRaw("Horizontal") < 0f)
            // {
            //     _newPosition += -OwnTransform.right * (Time.deltaTime * _movementSpeed);
            // }
            //
            // if (Input.mousePosition.y > Screen.height - 20f || Input.GetAxisRaw("Vertical") > 0f)
            // {
            //     _newPosition += OwnTransform.forward * (Time.deltaTime * _movementSpeed);
            // }
            // else if (Input.mousePosition.y < 20f || Input.GetAxisRaw("Vertical") < 0f)
            // {
            //     _newPosition -= OwnTransform.forward * (Time.deltaTime * _movementSpeed);
            // }
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

            _newZoom += zoomAmount * Mathf.Sign(Input.mouseScrollDelta.y);
            cameraTransform.localPosition =
                Vector3.Lerp(cameraTransform.localPosition, _newZoom, Time.deltaTime * movementTime);
        }
    }
}