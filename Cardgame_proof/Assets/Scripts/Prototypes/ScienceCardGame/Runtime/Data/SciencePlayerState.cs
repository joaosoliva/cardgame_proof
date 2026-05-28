using System.Collections.Generic;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{
    public sealed class SciencePlayerState
    {
        private readonly List<ScienceCardData> hand = new List<ScienceCardData>();
        private readonly List<ScienceCardData> playedCards = new List<ScienceCardData>();

        public SciencePlayerState(int playerIndex, string displayName)
        {
            PlayerIndex = playerIndex;
            DisplayName = displayName;
        }

        public int PlayerIndex { get; }
        public string DisplayName { get; }
        public int Score { get; private set; }
        public IReadOnlyList<ScienceCardData> Hand => hand;
        public IReadOnlyList<ScienceCardData> PlayedCards => playedCards;

        public void AddToHand(ScienceCardData card)
        {
            if (card != null) hand.Add(card);
        }

        public void MarkPlayed(ScienceCardData card)
        {
            if (card == null) return;
            hand.Remove(card);
            playedCards.Add(card);
        }

        public void SetScore(int score)
        {
            Score = score;
        }

        public void ResetCards()
        {
            hand.Clear();
            playedCards.Clear();
        }
    }
}
