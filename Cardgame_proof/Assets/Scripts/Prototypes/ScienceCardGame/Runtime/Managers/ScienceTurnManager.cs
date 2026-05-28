using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public sealed class ScienceTurnManager
    {
        private ScienceCardGameState state;
        private ScienceTelemetryManager telemetry;

        public int CurrentPlayerIndex { get; private set; }
        public int TurnNumber { get; private set; }

        public void Initialize(ScienceCardGameState gameState, ScienceTelemetryManager telemetryManager)
        {
            state = gameState;
            telemetry = telemetryManager;
            ResetForPlayers();
            Debug.Log("[ScienceCardGame] 05 TurnManager initialized");
            telemetry?.LogEvent("science_turn_initialized", $"turn={TurnNumber};player={CurrentPlayerIndex}");
        }

        public void ResetForPlayers()
        {
            CurrentPlayerIndex = 0;
            TurnNumber = state != null && state.Players.Count > 0 ? 1 : 0;
        }

        public void AdvanceTurn()
        {
            if (state == null || state.Players.Count == 0) return;
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % state.Players.Count;
            TurnNumber += 1;
            telemetry?.LogEvent("science_turn_advanced", $"turn={TurnNumber};player={CurrentPlayerIndex}");
        }

        public void Cleanup()
        {
            state = null;
            telemetry = null;
            CurrentPlayerIndex = 0;
            TurnNumber = 0;
        }
    }
}
