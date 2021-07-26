using System;
using System.Collections.Generic;

namespace LiftStudio
{
    [Serializable]
    public class MovementCardsSetup
    {
        public int playerCount;
        public List<MovementCardSettings> cardSet;
    }
}