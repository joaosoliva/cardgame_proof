using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public sealed class ScienceScoreManager
    {
        private ScienceCardGameState state;
        private ScienceTelemetryManager telemetry;

        public void Initialize(ScienceCardGameState gameState, ScienceTelemetryManager telemetryManager)
        {
            state = gameState;
            telemetry = telemetryManager;
            foreach (var player in state.Players)
            {
                player.SetScore(0);
            }

            Debug.Log("[ScienceCardGame] 04 ScoreManager initialized");
            telemetry?.LogEvent("science_scores_initialized", $"players={state.Players.Count}");
        }

        public void AddScore(int playerIndex, int amount)
        {
            if (state == null || playerIndex < 0 || playerIndex >= state.Players.Count) return;
            var player = state.Players[playerIndex];
            player.SetScore(player.Score + amount);
            telemetry?.LogEvent("science_score_changed", $"player={player.DisplayName};score={player.Score}");
        }

        public void Cleanup()
        {
            state = null;
            telemetry = null;
        }
    }
}
