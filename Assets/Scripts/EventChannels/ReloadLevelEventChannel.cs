using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

namespace LiftStudio.EventChannels
{
    [CreateAssetMenu(fileName = "ReloadLevelEventChannel", menuName = "Events/ReloadLevelEventChannel", order = 0)]
    public class ReloadLevelEventChannel : ScriptableObject
    {
        public void RaiseEvent()
        {
            PhotonNetwork.RaiseEvent((int) PhotonEventCodes.RestartGameCode, null, RaiseEventOptionsHelper.All,
                SendOptions.SendReliable);
        }

        [ContextMenu("Raise")]
        public void Raise()
        {
            RaiseEvent();
        }
    }
}