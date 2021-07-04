using UnityEngine;

namespace LiftStudio
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterType characterType;
        [SerializeField] private Collider characterCollider;

        public CharacterType CharacterType => characterType;

        public void ToggleColliderOff()
        {
            characterCollider.enabled = false;
        }

        public void ToggleColliderOn()
        {
            characterCollider.enabled = true;
        }
    }
}