using System.Collections.Generic;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{

    public sealed class SciencePreparedActionState
    {
        public SciencePreparedActionState(ScienceActionCardData actionCard, int ownerPlayerIndex, bool appliesToNextConnectionOnly)
        {
            ActionCard = actionCard;
            OwnerPlayerIndex = ownerPlayerIndex;
            AppliesToNextConnectionOnly = appliesToNextConnectionOnly;
        }

        public ScienceActionCardData ActionCard { get; }
        public int OwnerPlayerIndex { get; }
        public bool AppliesToNextConnectionOnly { get; }
        public bool Consumed { get; private set; }
        public bool Expired { get; private set; }

        public void MarkConsumed()
        {
            Consumed = true;
        }

        public void MarkExpired()
        {
            Expired = true;
        }
    }
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
        public bool InterdisciplinaryLeapBonusAvailable { get; private set; }
        public SciencePreparedActionState ActivePreparedAction { get; private set; }
        public bool HasActivePreparedAction => ActivePreparedAction != null && !ActivePreparedAction.Consumed && !ActivePreparedAction.Expired;
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

        public void SetInterdisciplinaryLeapBonusAvailable(bool available)
        {
            InterdisciplinaryLeapBonusAvailable = available;
        }

        public bool TryPrepareAction(ScienceActionCardData actionCard)
        {
            if (actionCard == null || HasActivePreparedAction) return false;
            ActivePreparedAction = new SciencePreparedActionState(actionCard, PlayerIndex, true);
            return true;
        }

        public void MarkPreparedActionConsumed()
        {
            ActivePreparedAction?.MarkConsumed();
        }

        public void MarkPreparedActionExpired()
        {
            ActivePreparedAction?.MarkExpired();
        }

        public void ClearPreparedAction()
        {
            ActivePreparedAction = null;
        }

        public void ResetActionModifiers()
        {
            CitationNeededBonusAvailable = false;
            InterdisciplinaryLeapBonusAvailable = false;
            ActivePreparedAction = null;
        }

        public void ResetCards()
        {
            hand.Clear();
            playedCards.Clear();
            ResetActionModifiers();
        }
    }
}
