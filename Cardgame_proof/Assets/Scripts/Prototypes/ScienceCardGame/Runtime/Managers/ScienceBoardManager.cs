using System.Collections.Generic;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;
using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public sealed class ScienceBoardManager
    {
        private readonly Dictionary<Vector2Int, ScienceCardData> boardCards = new Dictionary<Vector2Int, ScienceCardData>();
        private ScienceCardGameState state;
        private ScienceTelemetryManager telemetry;

        public IReadOnlyDictionary<Vector2Int, ScienceCardData> BoardCards => boardCards;

        public void Initialize(ScienceCardGameState gameState, ScienceTelemetryManager telemetryManager)
        {
            state = gameState;
            telemetry = telemetryManager;
            boardCards.Clear();
            Debug.Log($"[ScienceCardGame] 03 BoardManager initialized boardSize={state.BoardSize.x}x{state.BoardSize.y}");
            telemetry?.LogEvent("science_board_initialized", $"size={state.BoardSize.x}x{state.BoardSize.y}");
        }

        public bool TryPlaceCard(Vector2Int coordinate, ScienceCardData card)
        {
            if (card == null || boardCards.ContainsKey(coordinate)) return false;
            if (coordinate.x < 0 || coordinate.y < 0 || coordinate.x >= state.BoardSize.x || coordinate.y >= state.BoardSize.y) return false;
            boardCards[coordinate] = card;
            telemetry?.LogEvent("science_board_card_placed", $"card={card.Id};coord={coordinate}");
            return true;
        }

        public void Cleanup()
        {
            boardCards.Clear();
            state = null;
            telemetry = null;
        }
    }
}
