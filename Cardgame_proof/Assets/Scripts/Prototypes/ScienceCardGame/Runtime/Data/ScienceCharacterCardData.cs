using System;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{
    [Serializable]
    public sealed class ScienceCharacterCardData : ScienceCardData
    {
        public ScienceCharacterCardData(
            string id,
            string displayName,
            string field,
            string shortDescription,
            ScienceFactCategory factCategoryA,
            ScienceFactCategory factCategoryB,
            string miniBio)
            : base(id, displayName, shortDescription, ScienceCardType.Character)
        {
            Field = field;
            FactCategoryA = factCategoryA;
            FactCategoryB = factCategoryB;
            MiniBio = miniBio;
        }

        public string Field { get; }
        public ScienceFactCategory FactCategoryA { get; }
        public ScienceFactCategory FactCategoryB { get; }
        public string MiniBio { get; }
    }
}
