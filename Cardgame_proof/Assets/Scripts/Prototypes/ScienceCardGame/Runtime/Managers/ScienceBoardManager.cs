using System.Collections.Generic;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;
using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
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

        public bool CanPlaceCardAt(Vector2Int coordinate, ScienceCardData card)
        {
            return string.IsNullOrEmpty(GetPlacementValidationMessage(coordinate, card));
        }

        public string GetPlacementValidationMessage(Vector2Int coordinate, ScienceCardData card)
        {
            if (card == null) return "Nenhuma carta selecionada.";
            if (!(card is ScienceCharacterCardData)) return "Apenas cartas de personagem são colocadas no tabuleiro neste protótipo.";
            if (!IsCoordinateInBounds(coordinate)) return "Posição fora do tabuleiro.";
            if (boardCards.ContainsKey(coordinate)) return "Posição ocupada.";

            if (!HasAnyCharacterCards())
            {
                return IsNearCenter(coordinate) ? string.Empty : "A primeira personagem deve ser colocada perto do centro.";
            }

            return HasAdjacentCharacterCard(coordinate) ? string.Empty : "Coloque ao lado de pelo menos uma personagem existente.";
        }

        public bool TryPlaceCard(Vector2Int coordinate, ScienceCardData card, int rotationDegrees = 0)
        {
            string validationMessage = GetPlacementValidationMessage(coordinate, card);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                telemetry?.LogEvent("science_board_card_rejected", $"card={card?.Id ?? "none"};coord={coordinate};reason={validationMessage}");
                return false;
            }

            int normalizedRotation = ScienceBoardSlotState.NormalizeRotation(rotationDegrees);
            boardCards[coordinate] = card;
            boardSlots[coordinate] = new ScienceBoardSlotState(coordinate, card, normalizedRotation);
            telemetry?.LogEvent("science_board_card_placed", $"card={card.Id};coord={coordinate};rotation={normalizedRotation};occupied={boardCards.Count}");
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

        private bool HasAdjacentCharacterCard(Vector2Int coordinate)
        {
            return IsCharacterAt(coordinate + Vector2Int.up)
                || IsCharacterAt(coordinate + Vector2Int.down)
                || IsCharacterAt(coordinate + Vector2Int.left)
                || IsCharacterAt(coordinate + Vector2Int.right);
        }

        private bool IsCharacterAt(Vector2Int coordinate)
        {
            return boardCards.TryGetValue(coordinate, out ScienceCardData card) && card is ScienceCharacterCardData;
        }
    }
}
