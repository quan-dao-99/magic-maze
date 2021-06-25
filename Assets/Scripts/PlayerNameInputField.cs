using Photon.Pun;
using TMPro;
using UnityEngine;

namespace LiftStudio
{
    [RequireComponent(typeof(TMP_InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {
        private const string PlayerNamePrefKey = "PlayerName";

        private void Start()
        {
            var defaultName = string.Empty;
            var inputField = GetComponent<TMP_InputField>();
            if (inputField != null)
            {
                if (PlayerPrefs.HasKey(PlayerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(PlayerNamePrefKey);
                    inputField.text = defaultName;
                }
            }

            PhotonNetwork.NickName = defaultName;
        }

        public void SetPlayerName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Player name is null or empty");
                return;
            }

            PhotonNetwork.NickName = value;
            PlayerPrefs.SetString(PlayerNamePrefKey, value);
        }
    }
}