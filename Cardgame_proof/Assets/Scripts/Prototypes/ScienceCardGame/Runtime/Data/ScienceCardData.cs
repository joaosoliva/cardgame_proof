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
        /// <summary>Broad playable macro-category for biology, medicine, ecology, anatomy and health.</summary>
        LifeSciences,
        /// <summary>Broad playable macro-category for physics, chemistry, astronomy, materials and natural laws.</summary>
        PhysicalSciences,
        /// <summary>Broad playable macro-category for mathematics, computation, algorithms, statistics and logic.</summary>
        MathAndComputation,
        /// <summary>Broad playable macro-category for engineering, inventions, tools, machines and applied science.</summary>
        TechnologyAndInvention,
        /// <summary>Broad playable macro-category for education, activism, accessibility, social impact, communication and institutions.</summary>
        SocietyAndEducation
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
