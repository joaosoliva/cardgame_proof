using System;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{
    public enum ScienceCardType
    {
        Character,
        Action
    }

    public enum ScienceFactCategory
    {
        Observation,
        Experimentation,
        Theory,
        Technology,
        Society,
        Environment,
        Health,
        Mathematics
    }

    [Serializable]
    public abstract class ScienceCardData
    {
        protected ScienceCardData(string id, string displayName, string shortDescription, ScienceCardType cardType)
        {
            Id = id;
            DisplayName = displayName;
            ShortDescription = shortDescription;
            CardType = cardType;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string ShortDescription { get; }
        public ScienceCardType CardType { get; }
    }
}
