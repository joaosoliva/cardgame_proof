using System;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{
    public enum ScienceActionEffectType
    {
        PeerReview,
        CitationNeeded,
        InterdisciplinaryLeap
    }

    public enum ScienceActionTimingType
    {
        Immediate,
        Prepared
    }

    [Serializable]
    public sealed class ScienceActionCardData : ScienceCardData
    {
        public ScienceActionCardData(string id, string displayName, string shortDescription, ScienceActionEffectType effectType, ScienceActionTimingType timingType, string rulesText)
            : base(id, displayName, shortDescription, ScienceCardType.Action)
        {
            EffectType = effectType;
            TimingType = timingType;
            RulesText = rulesText;
        }

        public ScienceActionEffectType EffectType { get; }
        public ScienceActionTimingType TimingType { get; }
        public string RulesText { get; }
    }
}
