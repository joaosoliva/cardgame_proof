using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public sealed class ScienceTelemetryManager
    {
        private ScienceCardGameState state;

        public void Initialize(ScienceCardGameState gameState)
        {
            state = gameState;
            Debug.Log("[ScienceCardGame] 01 TelemetryManager initialized");
            LogEvent("science_prototype_initialized", $"players={state?.Players?.Count ?? 0}");
        }

        public void LogEvent(string eventName, string payload = "")
        {
            Debug.Log($"[ScienceCardGame][Telemetry] {eventName}: {payload}");
        }

        public void Cleanup()
        {
            LogEvent("science_telemetry_cleanup");
            state = null;
        }
    }
}
