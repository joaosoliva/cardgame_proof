using System;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{
    [Serializable]
    public sealed class ScienceCharacterCardData : ScienceCardData
    {
        public ScienceCharacterCardData(string id, string displayName, string scientificField, string era, string connectionPrompt)
            : base(id, displayName, connectionPrompt, ScienceCardType.Character)
        {
            ScientificField = scientificField;
            Era = era;
            ConnectionPrompt = connectionPrompt;
        }

        public string ScientificField { get; }
        public string Era { get; }
        public string ConnectionPrompt { get; }
    }
}
