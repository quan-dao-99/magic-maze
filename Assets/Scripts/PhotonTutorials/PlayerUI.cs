using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LiftStudio
{
    public class PlayerUI : MonoBehaviour
    {
        [Tooltip("Pixel offset from the player target")] [SerializeField]
        private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

        [Tooltip("UI Text to display Player's Name")] [SerializeField]
        private TMP_Text playerNameText;

        [Tooltip("UI Slider to display Player's Health")] [SerializeField]
        private Slider playerHealthSlider;

        private PlayerManager _target;
        private float _characterControllerHeight = 0f;
        private Transform _targetTransform;
        private Renderer _targetRenderer;
        private CanvasGroup _canvasGroup;
        private Vector3 _targetPosition;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        }

        public void SetTarget(PlayerManager target)
        {
            if (target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.",
                    this);
                return;
            }

            _target = target;
            _targetTransform = target.GetComponent<Transform>();
            _targetRenderer = target.GetComponent<Renderer>();
            var characterController = _target.GetComponent<CharacterController>();
            // Get data from the Player that won't change during the lifetime of this Component
            if (characterController != null)
            {
                _characterControllerHeight = characterController.height;
            }

            if (playerNameText != null)
            {
                playerNameText.text = target.photonView.Owner.NickName;
            }
        }

        private void Update()
        {
            playerHealthSlider.value = _target.health;

            if (_target == null)
            {
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
// Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
            if (_targetRenderer != null)
            {
                _canvasGroup.alpha = _targetRenderer.isVisible ? 1f : 0f;
            }
            
// #Critical
// Follow the Target GameObject on screen.
            if (_targetTransform == null) return;
            
            _targetPosition = _targetTransform.position;
            _targetPosition.y += _characterControllerHeight;
            transform.position = Camera.main.WorldToScreenPoint(_targetPosition) + screenOffset;
        }
    }
}