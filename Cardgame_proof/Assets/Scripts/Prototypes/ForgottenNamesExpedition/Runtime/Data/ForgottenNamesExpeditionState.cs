using System.Collections.Generic;

namespace CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime.Data
{
    public sealed class ForgottenNamesExpeditionState
    {
        public int SelectedPremiseIndex { get; private set; }
        public int PlayerCount { get; private set; } = 2;
        public int CurrentDeckIndex { get; private set; }
        public List<int> PartyScientistIndexes { get; } = new List<int>();
        public List<int> ArchiveScientistIndexes { get; } = new List<int>();
        public List<string> SessionLog { get; } = new List<string>();

        public ForgottenNamesPremise SelectedPremise => ForgottenNamesExpeditionContent.Premises[SelectedPremiseIndex];
        public int TotalCards => ForgottenNamesExpeditionContent.DemoDeck.Count;
        public bool IsComplete => CurrentDeckIndex >= TotalCards;

        public ForgottenNamesDeckCard CurrentCard => IsComplete
            ? new ForgottenNamesDeckCard(ForgottenNamesCardType.Final, 0)
            : ForgottenNamesExpeditionContent.DemoDeck[CurrentDeckIndex];

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

        public void AddScientistToParty(int scientistIndex)
        {
            if (!PartyScientistIndexes.Contains(scientistIndex))
            {
                PartyScientistIndexes.Add(scientistIndex);
            }

            AddLog($"Party: {ForgottenNamesExpeditionContent.Scientists[scientistIndex].Name}");
            AdvanceCard();
        }

        public void AddScientistToArchive(int scientistIndex)
        {
            if (!ArchiveScientistIndexes.Contains(scientistIndex))
            {
                ArchiveScientistIndexes.Add(scientistIndex);
            }

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

        private void AdvanceCard()
        {
            CurrentDeckIndex++;
        }
    }
}
