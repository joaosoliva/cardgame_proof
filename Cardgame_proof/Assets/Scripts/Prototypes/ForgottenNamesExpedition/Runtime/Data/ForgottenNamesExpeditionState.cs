using System;
using System.Collections.Generic;

namespace CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime.Data
{
    public sealed class ForgottenNamesExpeditionState
    {
        public const int PartyLimit = 3;
        public const int MinQuestionsPerBlock = 1;
        public const int MaxQuestionsPerBlock = 3;
        public const int MaxTotalQuestionCards = 6;

        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        private readonly List<ForgottenNamesDeckCard> sessionDeck = new List<ForgottenNamesDeckCard>();

        public int SelectedPremiseIndex { get; private set; }
        public int PlayerCount { get; private set; } = 2;
        public int CurrentDeckIndex { get; private set; }
        public List<int> PartyScientistIndexes { get; } = new List<int>();
        public List<int> ArchiveScientistIndexes { get; } = new List<int>();
        public List<string> SessionLog { get; } = new List<string>();

        public ForgottenNamesPremise SelectedPremise => ForgottenNamesExpeditionContent.Premises[SelectedPremiseIndex];
        public int TotalCards => sessionDeck.Count;
        public bool IsComplete => sessionDeck.Count > 0 && CurrentDeckIndex >= TotalCards;
        public bool IsPartyFull => PartyScientistIndexes.Count >= PartyLimit;

        public ForgottenNamesDeckCard CurrentCard => IsComplete || sessionDeck.Count == 0
            ? new ForgottenNamesDeckCard(ForgottenNamesCardType.Final, 0)
            : sessionDeck[CurrentDeckIndex];

        public ForgottenNamesRole GetRoleForPlayer(int playerIndex)
        {
            int roleIndex = playerIndex % ForgottenNamesExpeditionContent.Roles.Count;
            return ForgottenNamesExpeditionContent.Roles[roleIndex];
        }

        public int CurrentPlayerIndex => PlayerCount <= 0 ? 0 : CurrentDeckIndex % PlayerCount;
        public ForgottenNamesRole CurrentRole => GetRoleForPlayer(CurrentPlayerIndex);

        public void SelectPremise(int premiseIndex)
        {
            SelectedPremiseIndex = premiseIndex;
        }

        public void SetPlayerCount(int playerCount)
        {
            PlayerCount = playerCount < 1 ? 1 : playerCount;
        }

        public void GenerateSessionDeck()
        {
            sessionDeck.Clear();
            PartyScientistIndexes.Clear();
            ArchiveScientistIndexes.Clear();
            SessionLog.Clear();
            CurrentDeckIndex = 0;

            List<int> questionPool = BuildShuffledIndexPool(ForgottenNamesExpeditionContent.Questions.Count);
            List<int> scientistPool = BuildShuffledIndexPool(ForgottenNamesExpeditionContent.Scientists.Count);
            List<int> challengePool = BuildShuffledIndexPool(ForgottenNamesExpeditionContent.Challenges.Count);
            int questionCardsAdded = 0;

            AddQuestionBlock(questionPool, ref questionCardsAdded);
            AddNextCard(scientistPool, ForgottenNamesCardType.Scientist);
            AddQuestionBlock(questionPool, ref questionCardsAdded);
            AddNextCard(scientistPool, ForgottenNamesCardType.Scientist);
            AddNextCard(challengePool, ForgottenNamesCardType.Challenge);
            AddQuestionBlock(questionPool, ref questionCardsAdded);
            AddNextCard(scientistPool, ForgottenNamesCardType.Scientist);
            AddNextCard(challengePool, ForgottenNamesCardType.Challenge);
            sessionDeck.Add(new ForgottenNamesDeckCard(ForgottenNamesCardType.Final, 0));
        }

        public bool TryAddScientistToParty(int scientistIndex)
        {
            if (PartyScientistIndexes.Contains(scientistIndex))
            {
                AddLog($"Party manteve: {ForgottenNamesExpeditionContent.Scientists[scientistIndex].Name}");
                AdvanceCard();
                return true;
            }

            if (IsPartyFull)
            {
                return false;
            }

            AddScientistToPartyOnly(scientistIndex);
            AddLog($"Party: {ForgottenNamesExpeditionContent.Scientists[scientistIndex].Name}");
            AdvanceCard();
            return true;
        }

        public void ReplacePartyScientist(int scientistToMoveToArchiveIndex, int newPartyScientistIndex)
        {
            PartyScientistIndexes.Remove(scientistToMoveToArchiveIndex);
            AddScientistToArchiveOnly(scientistToMoveToArchiveIndex);
            AddScientistToPartyOnly(newPartyScientistIndex);
            AddLog($"Party: {ForgottenNamesExpeditionContent.Scientists[newPartyScientistIndex].Name}; Archive: {ForgottenNamesExpeditionContent.Scientists[scientistToMoveToArchiveIndex].Name}");
            AdvanceCard();
        }

        public void AddScientistToArchive(int scientistIndex)
        {
            PartyScientistIndexes.Remove(scientistIndex);
            AddScientistToArchiveOnly(scientistIndex);
            AddLog($"Archive: {ForgottenNamesExpeditionContent.Scientists[scientistIndex].Name}");
        }

        public void CompletePromptCard(string summary)
        {
            AddLog(summary);
            AdvanceCard();
        }

        public void AddLog(string entry)
        {
            if (!string.IsNullOrWhiteSpace(entry))
            {
                SessionLog.Add(entry);
            }
        }

        private void AddQuestionBlock(List<int> questionPool, ref int questionCardsAdded)
        {
            int remainingQuestionSlots = MaxTotalQuestionCards - questionCardsAdded;
            if (remainingQuestionSlots <= 0) return;

            int targetCount = random.Next(MinQuestionsPerBlock, MaxQuestionsPerBlock + 1);
            targetCount = Math.Min(targetCount, remainingQuestionSlots);

            for (int i = 0; i < targetCount; i++)
            {
                if (questionPool.Count == 0)
                {
                    questionPool.AddRange(BuildShuffledIndexPool(ForgottenNamesExpeditionContent.Questions.Count));
                }

                AddNextCard(questionPool, ForgottenNamesCardType.Question);
                questionCardsAdded++;
            }
        }

        private void AddNextCard(List<int> pool, ForgottenNamesCardType type)
        {
            if (pool.Count == 0) return;

            int contentIndex = pool[0];
            pool.RemoveAt(0);
            sessionDeck.Add(new ForgottenNamesDeckCard(type, contentIndex));
        }

        private List<int> BuildShuffledIndexPool(int count)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < count; i++)
            {
                indexes.Add(i);
            }

            for (int i = indexes.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(0, i + 1);
                int temp = indexes[i];
                indexes[i] = indexes[swapIndex];
                indexes[swapIndex] = temp;
            }

            return indexes;
        }

        private void AddScientistToPartyOnly(int scientistIndex)
        {
            ArchiveScientistIndexes.Remove(scientistIndex);
            if (!PartyScientistIndexes.Contains(scientistIndex))
            {
                PartyScientistIndexes.Add(scientistIndex);
            }
        }

        private void AddScientistToArchiveOnly(int scientistIndex)
        {
            if (!ArchiveScientistIndexes.Contains(scientistIndex))
            {
                ArchiveScientistIndexes.Add(scientistIndex);
            }
        }

        private void AdvanceCard()
        {
            CurrentDeckIndex++;
        }
    }
}
