using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiftStudio
{
    [RequireComponent(typeof(CameraWork))]
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The current Health of our player")]
        public float health = 1f;

        [Tooltip("The Player's UI GameObject Prefab")]
        public GameObject PlayerUiPrefab;

        [Tooltip("The Beams GameObject to control")] [SerializeField]
        private GameObject beams;

        private bool _isFiring;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_isFiring);
                stream.SendNext(health);
                return;
            }

            _isFiring = (bool) stream.ReceiveNext();
            health = (float) stream.ReceiveNext();
        }

        private void Awake()
        {
            beams.SetActive(false);

            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            var cameraWork = GetComponent<CameraWork>();

            if (photonView.IsMine)
            {
                cameraWork.OnStartFollowing();
            }
            
            if (PlayerUiPrefab != null)
            {
                var uiGo =  Instantiate(PlayerUiPrefab);
                uiGo.SendMessage ("SetTarget", this, SendMessageOptions.RequireReceiver);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                ProcessInputs();
                if (health <= 0)
                {
                    GameManager.Instance.LeaveRoom();
                }
            }

            if (_isFiring != beams.activeInHierarchy)
            {
                beams.SetActive(_isFiring);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine) return;

            if (!other.name.Contains("Beam")) return;

            health -= 0.1f;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!photonView.IsMine) return;

            if (!other.name.Contains("Beam")) return;

            health -= 0.1f * Time.deltaTime;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!_isFiring)
                {
                    _isFiring = true;
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                if (_isFiring)
                {
                    _isFiring = false;
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadingMode)
        {
            CalledOnLevelWasLoaded();
        }

        private void CalledOnLevelWasLoaded()
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
            
            var uiGo = Instantiate(PlayerUiPrefab);
            uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
    }
}