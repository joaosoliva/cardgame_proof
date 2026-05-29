using System.Collections.Generic;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;
using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public enum ScienceBoardSide
    {
        Up,
        Right,
        Down,
        Left
    }

    public sealed class SciencePlacementValidationResult
    {
        public SciencePlacementValidationResult(bool isValid, string reasonText, IReadOnlyList<string> matchingSides, IReadOnlyList<string> failingSides)
        {
            IsValid = isValid;
            ReasonText = reasonText;
            MatchingSides = matchingSides ?? new List<string>();
            FailingSides = failingSides ?? new List<string>();
        }

        public bool IsValid { get; }
        public string ReasonText { get; }
        public IReadOnlyList<string> MatchingSides { get; }
        public IReadOnlyList<string> FailingSides { get; }
    }

    public sealed class ScienceBoardSlotState
    {
        public ScienceBoardSlotState(Vector2Int coordinate, ScienceCardData card, int rotationDegrees)
        {
            Coordinate = coordinate;
            Card = card;
            RotationDegrees = NormalizeRotation(rotationDegrees);
        }

        public Vector2Int Coordinate { get; }
        public ScienceCardData Card { get; }
        public int RotationDegrees { get; }

        public static int NormalizeRotation(int rotationDegrees)
        {
            int normalized = rotationDegrees % 360;
            if (normalized < 0) normalized += 360;
            return (normalized / 90) * 90;
        }
    }

    public sealed class ScienceBoardManager
    {
        private static readonly ScienceBoardSide[] PlacementSides =
        {
            ScienceBoardSide.Up,
            ScienceBoardSide.Right,
            ScienceBoardSide.Down,
            ScienceBoardSide.Left
        };

        private readonly Dictionary<Vector2Int, ScienceCardData> boardCards = new Dictionary<Vector2Int, ScienceCardData>();
        private readonly Dictionary<Vector2Int, ScienceBoardSlotState> boardSlots = new Dictionary<Vector2Int, ScienceBoardSlotState>();
        private readonly List<Vector2Int> slots = new List<Vector2Int>();
        private ScienceCardGameState state;
        private ScienceTelemetryManager telemetry;

        public IReadOnlyDictionary<Vector2Int, ScienceCardData> BoardCards => boardCards;
        public IReadOnlyDictionary<Vector2Int, ScienceBoardSlotState> BoardSlots => boardSlots;
        public IReadOnlyList<Vector2Int> Slots => slots;
        public Vector2Int BoardSize => state?.BoardSize ?? Vector2Int.zero;
        public Vector2Int CenterCoordinate => new Vector2Int(BoardSize.x / 2, BoardSize.y / 2);

        public void Initialize(ScienceCardGameState gameState, ScienceTelemetryManager telemetryManager)
        {
            state = gameState;
            telemetry = telemetryManager;
            boardCards.Clear();
            boardSlots.Clear();
            BuildLogicalSlots();
            Debug.Log($"[ScienceCardGame] 03 BoardManager initialized boardSize={state.BoardSize.x}x{state.BoardSize.y} slots={slots.Count}");
            telemetry?.LogEvent("science_board_initialized", $"size={state.BoardSize.x}x{state.BoardSize.y};slots={slots.Count};center={CenterCoordinate}");
        }

        public bool CanPlaceCardAt(Vector2Int coordinate, ScienceCardData card, int rotationDegrees = 0)
        {
            return ValidatePlacement(coordinate, card, rotationDegrees).IsValid;
        }

        public string GetPlacementValidationMessage(Vector2Int coordinate, ScienceCardData card, int rotationDegrees = 0)
        {
            SciencePlacementValidationResult result = ValidatePlacement(coordinate, card, rotationDegrees);
            return result.IsValid ? string.Empty : result.ReasonText;
        }

        public SciencePlacementValidationResult ValidatePlacement(Vector2Int coordinate, ScienceCardData card, int rotationDegrees = 0)
        {
            List<string> matchingSides = new List<string>();
            List<string> failingSides = new List<string>();

            if (card == null) return Invalid("Nenhuma carta selecionada.", matchingSides, failingSides);
            if (!(card is ScienceCharacterCardData)) return Invalid("Apenas cartas de personagem são colocadas no tabuleiro neste protótipo.", matchingSides, failingSides);
            if (!IsCoordinateInBounds(coordinate)) return Invalid("Posição fora do tabuleiro.", matchingSides, failingSides);
            if (boardCards.ContainsKey(coordinate)) return Invalid("Posição ocupada.", matchingSides, failingSides);

            if (!HasAnyCharacterCards())
            {
                return IsNearCenter(coordinate)
                    ? Valid("Primeira carta válida perto do centro.", matchingSides, failingSides)
                    : Invalid("A primeira personagem deve ser colocada perto do centro.", matchingSides, failingSides);
            }

            int adjacentCharacterCount = 0;
            foreach (ScienceBoardSide side in PlacementSides)
            {
                Vector2Int neighborCoordinate = coordinate + GetOffset(side);
                if (!boardCards.TryGetValue(neighborCoordinate, out ScienceCardData neighborCard) || !(neighborCard is ScienceCharacterCardData))
                {
                    continue;
                }

                adjacentCharacterCount += 1;
                int neighborRotation = GetPlacedCardRotationDegrees(neighborCoordinate);
                ScienceBoardSide neighborSide = GetOppositeSide(side);
                ScienceFactCategory? cardColor = GetColorOnSide(card, rotationDegrees, side);
                ScienceFactCategory? neighborColor = GetColorOnSide(neighborCard, neighborRotation, neighborSide);
                string sideName = FormatSide(side);
                string neighborSideName = FormatSide(neighborSide);

                if (!cardColor.HasValue)
                {
                    failingSides.Add($"{sideName}: esta carta não expõe cor nessa borda.");
                    continue;
                }

                if (!neighborColor.HasValue)
                {
                    failingSides.Add($"{sideName}: a carta vizinha não expõe cor na borda {neighborSideName}.");
                    continue;
                }

                if (cardColor.Value == neighborColor.Value)
                {
                    matchingSides.Add($"{sideName}: {cardColor.Value} combina com {neighborColor.Value}.");
                }
                else
                {
                    failingSides.Add($"{sideName}: {cardColor.Value} não combina com {neighborColor.Value}.");
                }
            }

            if (adjacentCharacterCount == 0)
            {
                return Invalid("Coloque ao lado de pelo menos uma personagem existente.", matchingSides, failingSides);
            }

            if (failingSides.Count > 0)
            {
                return Invalid($"Conexão de cores inválida. {string.Join(" ", failingSides.ToArray())}", matchingSides, failingSides);
            }

            if (matchingSides.Count == 0)
            {
                return Invalid("Nenhuma borda tocando possui cores compatíveis.", matchingSides, failingSides);
            }

            return Valid($"Conexão válida. {string.Join(" ", matchingSides.ToArray())}", matchingSides, failingSides);
        }

        public ScienceFactCategory? GetColorOnSide(ScienceCardData cardData, int rotationDegrees, ScienceBoardSide side)
        {
            if (!(cardData is ScienceCharacterCardData characterCard)) return null;
            if (characterCard.FactCategoryA == characterCard.FactCategoryB) return characterCard.FactCategoryA;

            ScienceBoardSide unrotatedSide = RotateSideCounterClockwise(side, ScienceBoardSlotState.NormalizeRotation(rotationDegrees) / 90);
            switch (unrotatedSide)
            {
                case ScienceBoardSide.Left:
                    return characterCard.FactCategoryA;
                case ScienceBoardSide.Right:
                    return characterCard.FactCategoryB;
                default:
                    return null;
            }
        }

        public bool TryPlaceCard(Vector2Int coordinate, ScienceCardData card, int rotationDegrees = 0, bool overrideValidation = false)
        {
            SciencePlacementValidationResult validationResult = ValidatePlacement(coordinate, card, rotationDegrees);
            if (!validationResult.IsValid && !overrideValidation)
            {
                telemetry?.LogEvent("science_board_card_rejected", $"card={card?.Id ?? "none"};coord={coordinate};rotation={ScienceBoardSlotState.NormalizeRotation(rotationDegrees)};reason={validationResult.ReasonText}");
                return false;
            }

            if (!validationResult.IsValid)
            {
                telemetry?.LogEvent("science_board_card_debug_override", $"card={card?.Id ?? "none"};coord={coordinate};rotation={ScienceBoardSlotState.NormalizeRotation(rotationDegrees)};reason={validationResult.ReasonText}");
            }

            int normalizedRotation = ScienceBoardSlotState.NormalizeRotation(rotationDegrees);
            boardCards[coordinate] = card;
            boardSlots[coordinate] = new ScienceBoardSlotState(coordinate, card, normalizedRotation);
            telemetry?.LogEvent("science_board_card_placed", $"card={card.Id};coord={coordinate};rotation={normalizedRotation};occupied={boardCards.Count};matches={validationResult.MatchingSides.Count}");
            return true;
        }

        public int GetPlacedCardRotationDegrees(Vector2Int coordinate)
        {
            return boardSlots.TryGetValue(coordinate, out ScienceBoardSlotState slotState) ? slotState.RotationDegrees : 0;
        }

        public bool RemoveCardAt(Vector2Int coordinate)
        {
            bool removedCard = boardCards.Remove(coordinate);
            bool removedSlot = boardSlots.Remove(coordinate);
            if (removedCard || removedSlot)
            {
                telemetry?.LogEvent("science_board_card_removed", $"coord={coordinate}");
                return true;
            }

            return false;
        }

        public void Cleanup()
        {
            boardCards.Clear();
            boardSlots.Clear();
            slots.Clear();
            state = null;
            telemetry = null;
        }

        private void BuildLogicalSlots()
        {
            slots.Clear();
            if (state == null) return;

            for (int y = 0; y < state.BoardSize.y; y++)
            {
                for (int x = 0; x < state.BoardSize.x; x++)
                {
                    slots.Add(new Vector2Int(x, y));
                }
            }
        }

        private bool IsCoordinateInBounds(Vector2Int coordinate)
        {
            Vector2Int size = BoardSize;
            return coordinate.x >= 0 && coordinate.y >= 0 && coordinate.x < size.x && coordinate.y < size.y;
        }

        private bool IsNearCenter(Vector2Int coordinate)
        {
            Vector2Int center = CenterCoordinate;
            return Mathf.Abs(coordinate.x - center.x) <= 1 && Mathf.Abs(coordinate.y - center.y) <= 1;
        }

        private bool HasAnyCharacterCards()
        {
            foreach (ScienceCardData card in boardCards.Values)
            {
                if (card is ScienceCharacterCardData) return true;
            }

            return false;
        }

        private static SciencePlacementValidationResult Valid(string reasonText, IReadOnlyList<string> matchingSides, IReadOnlyList<string> failingSides)
        {
            return new SciencePlacementValidationResult(true, reasonText, matchingSides, failingSides);
        }

        private static SciencePlacementValidationResult Invalid(string reasonText, IReadOnlyList<string> matchingSides, IReadOnlyList<string> failingSides)
        {
            return new SciencePlacementValidationResult(false, reasonText, matchingSides, failingSides);
        }

        private static ScienceBoardSide RotateSideCounterClockwise(ScienceBoardSide side, int quarterTurns)
        {
            int normalizedTurns = ((quarterTurns % 4) + 4) % 4;
            ScienceBoardSide rotated = side;
            for (int i = 0; i < normalizedTurns; i++)
            {
                rotated = RotateSideCounterClockwiseOnce(rotated);
            }

            return rotated;
        }

        private static ScienceBoardSide RotateSideCounterClockwiseOnce(ScienceBoardSide side)
        {
            switch (side)
            {
                case ScienceBoardSide.Up:
                    return ScienceBoardSide.Left;
                case ScienceBoardSide.Left:
                    return ScienceBoardSide.Down;
                case ScienceBoardSide.Down:
                    return ScienceBoardSide.Right;
                default:
                    return ScienceBoardSide.Up;
            }
        }

        private static ScienceBoardSide GetOppositeSide(ScienceBoardSide side)
        {
            switch (side)
            {
                case ScienceBoardSide.Up:
                    return ScienceBoardSide.Down;
                case ScienceBoardSide.Right:
                    return ScienceBoardSide.Left;
                case ScienceBoardSide.Down:
                    return ScienceBoardSide.Up;
                default:
                    return ScienceBoardSide.Right;
            }
        }

        private static Vector2Int GetOffset(ScienceBoardSide side)
        {
            switch (side)
            {
                case ScienceBoardSide.Up:
                    return Vector2Int.up;
                case ScienceBoardSide.Right:
                    return Vector2Int.right;
                case ScienceBoardSide.Down:
                    return Vector2Int.down;
                default:
                    return Vector2Int.left;
            }
        }

        private static string FormatSide(ScienceBoardSide side)
        {
            switch (side)
            {
                case ScienceBoardSide.Up:
                    return "cima";
                case ScienceBoardSide.Right:
                    return "direita";
                case ScienceBoardSide.Down:
                    return "baixo";
                default:
                    return "esquerda";
            }
        }
    }
}
