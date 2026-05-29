using System.Collections.Generic;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;
using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public sealed class ScienceBoardManager
    {
        private readonly Dictionary<Vector2Int, ScienceCardData> boardCards = new Dictionary<Vector2Int, ScienceCardData>();
        private readonly List<Vector2Int> slots = new List<Vector2Int>();
        private ScienceCardGameState state;
        private ScienceTelemetryManager telemetry;

        public IReadOnlyDictionary<Vector2Int, ScienceCardData> BoardCards => boardCards;
        public IReadOnlyList<Vector2Int> Slots => slots;
        public Vector2Int BoardSize => state?.BoardSize ?? Vector2Int.zero;
        public Vector2Int CenterCoordinate => new Vector2Int(BoardSize.x / 2, BoardSize.y / 2);

        public void Initialize(ScienceCardGameState gameState, ScienceTelemetryManager telemetryManager)
        {
            state = gameState;
            telemetry = telemetryManager;
            boardCards.Clear();
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

        public bool TryPlaceCard(Vector2Int coordinate, ScienceCardData card)
        {
            string validationMessage = GetPlacementValidationMessage(coordinate, card);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                telemetry?.LogEvent("science_board_card_rejected", $"card={card?.Id ?? "none"};coord={coordinate};reason={validationMessage}");
                return false;
            }

            boardCards[coordinate] = card;
            telemetry?.LogEvent("science_board_card_placed", $"card={card.Id};coord={coordinate};occupied={boardCards.Count}");
            return true;
        }

        public void Cleanup()
        {
            boardCards.Clear();
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
