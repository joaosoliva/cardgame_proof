using UnityEngine;
using CardgameProof.App;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime
{
    public sealed class ScienceCardGameBootstrap : MonoBehaviour
    {
        [SerializeField] private bool debugRevealAllHands;
        [SerializeField] private int targetKnowledgePoints = 7;
        [SerializeField] private bool acceptTiedConnectionVotes = true;
        [SerializeField] private ScienceRejectedConnectionBehavior rejectedConnectionBehavior = ScienceRejectedConnectionBehavior.ReturnCardToHand;

        private PrototypeRuntimeContext context;
        private ScienceCardGameState state;
        private ScienceTelemetryManager telemetryManager;
        private ScienceDeckManager deckManager;
        private ScienceBoardManager boardManager;
        private ScienceScoreManager scoreManager;
        private ScienceTurnManager turnManager;
        private ScienceCardGameUIManager uiManager;

        public void Initialize(PrototypeRuntimeContext runtimeContext, ScienceCardGameState initialState, bool revealAllHandsForDebug = false)
        {
            debugRevealAllHands = revealAllHandsForDebug;
            Debug.Log("[ScienceCardGame] Bootstrap initialize begin");
            context = runtimeContext;
            state = initialState ?? new ScienceCardGameState();

            if (context?.SceneRoot?.FullScreenRoot == null)
            {
                Debug.LogWarning("[ScienceCardGame] Bootstrap initialization failed: missing scene root.");
                return;
            }

            state.InitializeDefaults();
            state.SetDebugRevealAllHands(debugRevealAllHands);
            state.SetTargetKnowledgePoints(targetKnowledgePoints);
            state.SetAcceptTiedConnectionVotes(acceptTiedConnectionVotes);
            state.SetRejectedConnectionBehavior(rejectedConnectionBehavior);
            Debug.Log($"[ScienceCardGame] 00 State initialized for setup debugRevealAllHands={debugRevealAllHands} targetKnowledgePoints={targetKnowledgePoints} acceptTiedConnectionVotes={acceptTiedConnectionVotes} rejectedConnectionBehavior={rejectedConnectionBehavior}");

            telemetryManager = new ScienceTelemetryManager();
            telemetryManager.Initialize(state);

            deckManager = new ScienceDeckManager();
            deckManager.Initialize(ScienceCardLibrary.CreateDemoDeck(), telemetryManager);

            boardManager = new ScienceBoardManager();
            boardManager.Initialize(state, telemetryManager);

            scoreManager = new ScienceScoreManager();
            scoreManager.Initialize(state, telemetryManager);

            turnManager = new ScienceTurnManager();
            turnManager.Initialize(state, telemetryManager);

            uiManager = new ScienceCardGameUIManager();
            uiManager.Initialize(context, state, deckManager, boardManager, scoreManager, turnManager, telemetryManager, ConfirmSetupAndDistributeCards, RestartPrototype);

            telemetryManager.LogEvent("science_bootstrap_complete", "screen=setup");
            Debug.Log("[ScienceCardGame] Bootstrap initialize complete");
        }

        public void Cleanup()
        {
            Debug.Log("[ScienceCardGame] Bootstrap cleanup begin");
            uiManager?.Cleanup();
            turnManager?.Cleanup();
            scoreManager?.Cleanup();
            boardManager?.Cleanup();
            deckManager?.Cleanup();
            telemetryManager?.Cleanup();

            uiManager = null;
            turnManager = null;
            scoreManager = null;
            boardManager = null;
            deckManager = null;
            telemetryManager = null;
            context = null;
            state = null;
            Debug.Log("[ScienceCardGame] Bootstrap cleanup complete");
        }

        private void RestartPrototype()
        {
            PrototypeRuntimeContext restartContext = context;
            bool revealAllHandsForDebug = debugRevealAllHands;
            Cleanup();
            Initialize(restartContext, new ScienceCardGameState(), revealAllHandsForDebug);
        }

        private void ConfirmSetupAndDistributeCards(int playerCount)
        {
            if (state == null || deckManager == null || uiManager == null) return;

            Debug.Log($"[ScienceCardGame] Setup confirmed players={playerCount}");
            state.InitializePlayers(playerCount);
            scoreManager?.ResetScores();
            turnManager?.ResetForPlayers();
            deckManager.DealInitialHands(state.Players, state.InitialHandSize, state.DebugRevealAllHands);
            state.SetPhase(ScienceCardGamePhase.CardDistribution);
            telemetryManager?.LogEvent("science_setup_confirmed", $"players={state.SelectedPlayerCount};phase={state.CurrentPhase}");
            uiManager.ShowCardDistributionScreen();
        }
    }
}
