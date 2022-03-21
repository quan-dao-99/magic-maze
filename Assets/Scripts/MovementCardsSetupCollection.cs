using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio
{
    [CreateAssetMenu(fileName = "MovementCardsCollection", menuName = "Movement Card Collection")]
    public class MovementCardsSetupCollection : ScriptableObject
    {
        public List<MovementCardsSetup> allSetups;
    }
}