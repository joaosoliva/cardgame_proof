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

    public enum SciencePlacementConnectionType
    {
        Invalid,
        FirstCard,
        Strong,
        Interpretive
    }

    public sealed class SciencePlacementValidationResult
    {
        public SciencePlacementValidationResult(
            bool isSpatiallyValid,
            SciencePlacementConnectionType connectionType,
            string feedbackTitle,
            string feedbackBody,
            IReadOnlyList<string> matchingSides,
            IReadOnlyList<string> nonMatchingSides,
            bool shouldExpectContestation)
        {
            IsSpatiallyValid = isSpatiallyValid;
            ConnectionType = connectionType;
            FeedbackTitle = feedbackTitle;
            FeedbackBody = feedbackBody;
            MatchingSides = matchingSides ?? new List<string>();
            NonMatchingSides = nonMatchingSides ?? new List<string>();
            ShouldExpectContestation = shouldExpectContestation;
        }

        public bool IsSpatiallyValid { get; }
        public SciencePlacementConnectionType ConnectionType { get; }
        public string FeedbackTitle { get; }
        public string FeedbackBody { get; }
        public IReadOnlyList<string> MatchingSides { get; }
        public IReadOnlyList<string> NonMatchingSides { get; }
        public bool ShouldExpectContestation { get; }

        public bool IsValid => IsSpatiallyValid;
        public string ReasonText => FeedbackBody;
        public IReadOnlyList<string> FailingSides => NonMatchingSides;
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
            return ValidatePlacement(coordinate, card, rotationDegrees).IsSpatiallyValid;
        }

        public string GetPlacementValidationMessage(Vector2Int coordinate, ScienceCardData card, int rotationDegrees = 0)
        {
            SciencePlacementValidationResult result = ValidatePlacement(coordinate, card, rotationDegrees);
            return result.IsSpatiallyValid ? string.Empty : result.FeedbackBody;
        }

        public SciencePlacementValidationResult ValidatePlacement(Vector2Int coordinate, ScienceCardData card, int rotationDegrees = 0)
        {
            List<string> matchingSides = new List<string>();
            List<string> nonMatchingSides = new List<string>();

            if (card == null) return Invalid("Nenhuma carta selecionada.", "Escolha uma carta antes de selecionar o tabuleiro.", matchingSides, nonMatchingSides);
            if (!(card is ScienceCharacterCardData)) return Invalid("Carta inválida", "Apenas cartas de personagem são colocadas no tabuleiro neste protótipo.", matchingSides, nonMatchingSides);
            if (!IsCoordinateInBounds(coordinate)) return Invalid("Posição inválida", "Essa posição está fora do tabuleiro.", matchingSides, nonMatchingSides);
            if (boardCards.ContainsKey(coordinate)) return Invalid("Posição ocupada", "Escolha uma casa vazia para colocar a carta.", matchingSides, nonMatchingSides);

            if (!HasAnyCharacterCards())
            {
                return IsNearCenter(coordinate)
                    ? FirstCard("Primeira carta", "Primeira carta válida perto do centro. A partir dela, as próximas cartas devem tocar a rede.", matchingSides, nonMatchingSides)
                    : Invalid("Posição inválida", "A primeira personagem deve ser colocada perto do centro.", matchingSides, nonMatchingSides);
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
                    nonMatchingSides.Add($"borda {sideName} sem cor direta para comparar");
                    continue;
                }

                if (!neighborColor.HasValue)
                {
                    nonMatchingSides.Add($"vizinha sem cor direta em {neighborSideName} ({sideName})");
                    continue;
                }

                if (cardColor.Value == neighborColor.Value)
                {
                    matchingSides.Add($"{cardColor.Value} com {neighborColor.Value} ({sideName})");
                }
                else
                {
                    nonMatchingSides.Add($"{cardColor.Value} com {neighborColor.Value} ({sideName})");
                }
            }

            if (adjacentCharacterCount == 0)
            {
                return Invalid("Posição inválida", "Coloque a carta adjacente à rede de cartas.", matchingSides, nonMatchingSides);
            }

            if (matchingSides.Count > 0 && nonMatchingSides.Count == 0)
            {
                return Strong($"As cores combinam: {matchingSides[0]}. Essa ligação tende a ser fácil de defender.", matchingSides, nonMatchingSides);
            }

            string interpretiveBody = nonMatchingSides.Count > 0
                ? $"As cores não combinam diretamente ({nonMatchingSides[0]}). Você pode tentar, mas outros jogadores podem contestar. Prepare uma boa explicação."
                : "As cores não criam uma correspondência direta. Você pode tentar, mas outros jogadores podem contestar. Prepare uma boa explicação.";
            return Interpretive(interpretiveBody, matchingSides, nonMatchingSides);
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
            if (!validationResult.IsSpatiallyValid && !overrideValidation)
            {
                telemetry?.LogEvent("science_board_card_rejected", $"card={card?.Id ?? "none"};coord={coordinate};rotation={ScienceBoardSlotState.NormalizeRotation(rotationDegrees)};reason={validationResult.FeedbackBody}");
                return false;
            }

            if (!validationResult.IsSpatiallyValid)
            {
                telemetry?.LogEvent("science_board_card_debug_override", $"card={card?.Id ?? "none"};coord={coordinate};rotation={ScienceBoardSlotState.NormalizeRotation(rotationDegrees)};reason={validationResult.FeedbackBody}");
            }

            int normalizedRotation = ScienceBoardSlotState.NormalizeRotation(rotationDegrees);
            boardCards[coordinate] = card;
            boardSlots[coordinate] = new ScienceBoardSlotState(coordinate, card, normalizedRotation);
            telemetry?.LogEvent("science_board_card_placed", $"card={card.Id};coord={coordinate};rotation={normalizedRotation};occupied={boardCards.Count};matches={validationResult.MatchingSides.Count};nonMatches={validationResult.NonMatchingSides.Count};connectionType={validationResult.ConnectionType};contested={validationResult.ShouldExpectContestation}");
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

        private static SciencePlacementValidationResult FirstCard(string feedbackTitle, string feedbackBody, IReadOnlyList<string> matchingSides, IReadOnlyList<string> nonMatchingSides)
        {
            return new SciencePlacementValidationResult(true, SciencePlacementConnectionType.FirstCard, feedbackTitle, feedbackBody, matchingSides, nonMatchingSides, false);
        }

        private static SciencePlacementValidationResult Strong(string feedbackBody, IReadOnlyList<string> matchingSides, IReadOnlyList<string> nonMatchingSides)
        {
            return new SciencePlacementValidationResult(true, SciencePlacementConnectionType.Strong, "Conexão forte", feedbackBody, matchingSides, nonMatchingSides, false);
        }

        private static SciencePlacementValidationResult Interpretive(string feedbackBody, IReadOnlyList<string> matchingSides, IReadOnlyList<string> nonMatchingSides)
        {
            return new SciencePlacementValidationResult(true, SciencePlacementConnectionType.Interpretive, "Conexão interpretativa", feedbackBody, matchingSides, nonMatchingSides, true);
        }

        private static SciencePlacementValidationResult Invalid(string feedbackTitle, string feedbackBody, IReadOnlyList<string> matchingSides, IReadOnlyList<string> nonMatchingSides)
        {
            return new SciencePlacementValidationResult(false, SciencePlacementConnectionType.Invalid, feedbackTitle, feedbackBody, matchingSides, nonMatchingSides, false);
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
