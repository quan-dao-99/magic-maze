using UnityEngine;

namespace LiftStudio.EventChannels
{
    [CreateAssetMenu(fileName = "QuitGameEventChannel", menuName = "Events/QuitGameEventChannel")]
    public class QuitGameEventChannel : ScriptableObject
    {
        public void RaiseEvent()
        {
            Application.Quit();
        }
    }
}