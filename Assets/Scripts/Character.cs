using Photon.Pun;
using UnityEngine;

namespace LiftStudio
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterType characterType;
        [SerializeField] private Collider characterCollider;
        [SerializeField] private PhotonView photonView;
        
        public CharacterType CharacterType => characterType;
        public PhotonView PhotonView => photonView;

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