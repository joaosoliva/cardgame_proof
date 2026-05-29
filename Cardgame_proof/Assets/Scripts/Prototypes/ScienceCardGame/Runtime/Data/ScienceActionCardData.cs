using System;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{
    public enum ScienceActionEffectType
    {
        PeerReview,
        CitationNeeded,
        InterdisciplinaryLeap
    }

    [Serializable]
    public sealed class ScienceActionCardData : ScienceCardData
    {
        public ScienceActionCardData(string id, string displayName, string shortDescription, ScienceActionEffectType effectType, string rulesText)
            : base(id, displayName, shortDescription, ScienceCardType.Action)
        {
            EffectType = effectType;
            RulesText = rulesText;
        }

        public ScienceActionEffectType EffectType { get; }
        public string RulesText { get; }
    }
}
