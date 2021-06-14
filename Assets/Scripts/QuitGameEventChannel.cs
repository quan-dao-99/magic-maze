using UnityEngine;

namespace LiftStudio
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