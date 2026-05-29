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
        public bool CitationNeededBonusAvailable { get; private set; }
        public bool InterdisciplinaryLeapAvailable { get; private set; }
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

        public void ReturnPlayedToHand(ScienceCardData card)
        {
            if (card == null) return;
            playedCards.Remove(card);
            if (!hand.Contains(card)) hand.Add(card);
        }

        public void SetScore(int score)
        {
            Score = score;
        }

        public void SetCitationNeededBonusAvailable(bool available)
        {
            CitationNeededBonusAvailable = available;
        }

        public void SetInterdisciplinaryLeapAvailable(bool available)
        {
            InterdisciplinaryLeapAvailable = available;
        }

        public void ResetActionModifiers()
        {
            CitationNeededBonusAvailable = false;
            InterdisciplinaryLeapAvailable = false;
        }

        public void ResetCards()
        {
            hand.Clear();
            playedCards.Clear();
            ResetActionModifiers();
        }
    }
}
