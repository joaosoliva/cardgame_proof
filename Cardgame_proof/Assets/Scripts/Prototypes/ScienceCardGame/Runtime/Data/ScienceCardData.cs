using System;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{
    public enum ScienceCardType
    {
        Character,
        Action
    }

    [Serializable]
    public abstract class ScienceCardData
    {
        protected ScienceCardData(string id, string displayName, string description, ScienceCardType cardType)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            CardType = cardType;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public ScienceCardType CardType { get; }
    }
}
