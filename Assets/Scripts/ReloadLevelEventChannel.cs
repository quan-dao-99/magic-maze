using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiftStudio
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