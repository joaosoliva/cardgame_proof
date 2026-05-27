using System;
using UnityEngine;

namespace CardgameProof.Core
{
    public enum GamePhase
    {
        MainMenu,
        Setup,
        Draft,
        TutorialIntro,
        Investigation,
        Resolution,
        End
    }

    public enum CardType
    {
        Character,
        Archive
    }

    public enum PlayerId
    {
        PlayerOne,
        PlayerTwo
    }

    public enum ClueCategory
    {
        Area,
        Era,
        Region,
        Contribution,
        ContextLegacy
    }

    [Serializable]
    public sealed class GameModeConfig
    {
        public string Id;
        public string DisplayName;
        public int DurationMinutes;
        public int TotalCharacters;
        public int CharactersPerPlayer;
        public int ArchiveCardsPerPlayer;
        public int ResearchTokensPerPlayer;
        public Vector2Int BoardSize;
        public int ObjectiveIdentifications;
    }

    [Serializable]
    public sealed class CharacterData
    {
        public string Id;
        public string DisplayName;
        public string Area;
        public string Era;
        public string Region;
        public string Contribution;
        public string ContextOrLegacy;
        [TextArea]
        public string GuidebookBioPtBr;
    }

    [Serializable]
    public class CardData
    {
        public string Id;
        public CardType CardType;
        public string Title;
        [TextArea]
        public string Description;
    }

    [Serializable]
    public sealed class ArchiveCardData : CardData
    {
        public ClueCategory ClueCategory;
        public string Prompt;
    }

    [Serializable]
    public sealed class PlayerState
    {
        public PlayerId PlayerId;
        public int ResearchTokens;
        public CharacterData[] CharactersInHand;
        public ArchiveCardData[] ArchiveCardsInHand;
        public string[] IdentifiedCharacterIds;
    }

    [Serializable]
    public sealed class BoardCellData
    {
        public Vector2Int Coordinate;
        public bool IsOccupied;
        public PlacedCardData Occupant;
    }


    [Serializable]
    public sealed class TutorialStep
    {
        public string Id;
        public string Title;
        [TextArea]
        public string Body;
        public GamePhase? Phase;
        public string TargetKey;
        public bool OnlyShowOnce;
    }

    [Serializable]
    public sealed class PlacedCardData
    {
        public string CardId;
        public CardType CardType;
        public PlayerId Owner;
        public Vector2Int Coordinate;
        public bool IsFaceUp;
    }
}
