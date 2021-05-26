using UnityEngine;

namespace LiftStudio
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterType characterType;

        public CharacterType CharacterType => characterType;
    }
}