using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiftStudio.EventChannels
{
    [CreateAssetMenu(fileName = "ReloadLevelEventChannel", menuName = "Events/ReloadLevelEventChannel", order = 0)]
    public class ReloadLevelEventChannel : ScriptableObject
    {
        public void RaiseEvent()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        [ContextMenu("TriggerEvent")]
        public void TriggerEvent()
        {
            RaiseEvent();
        }
    }
}