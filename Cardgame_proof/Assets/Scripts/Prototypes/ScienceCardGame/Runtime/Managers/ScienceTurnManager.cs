using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;
using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public enum ScienceTurnStep
    {
        AwaitingCardSelection,
        AwaitingBoardSlot,
        AwaitingPlacementConfirmation,
        ConnectionExplanation,
        ActionResolution,
        TurnResolved
    }

    public sealed class ScienceTurnManager
    {
        private ScienceCardGameState state;
        private ScienceTelemetryManager telemetry;
        private Vector2Int selectedBoardCoordinate;
        private bool hasSelectedBoardCoordinate;

        public int CurrentPlayerIndex { get; private set; }
        public int TurnNumber { get; private set; }
        public ScienceTurnStep CurrentStep { get; private set; } = ScienceTurnStep.AwaitingCardSelection;
        public ScienceCardData SelectedCard { get; private set; }
        public int SelectedRotationDegrees { get; private set; }
        public bool HasSelectedBoardCoordinate => hasSelectedBoardCoordinate;
        public Vector2Int SelectedBoardCoordinate => selectedBoardCoordinate;

        public void Initialize(ScienceCardGameState gameState, ScienceTelemetryManager telemetryManager)
        {
            state = gameState;
            telemetry = telemetryManager;
            ResetForPlayers();
            Debug.Log("[ScienceCardGame] 05 TurnManager initialized");
            telemetry?.LogEvent("science_turn_initialized", $"turn={TurnNumber};player={CurrentPlayerIndex};step={CurrentStep}");
        }

        public void ResetForPlayers()
        {
            CurrentPlayerIndex = 0;
            TurnNumber = state != null && state.Players.Count > 0 ? 1 : 0;
            ClearSelection();
            CurrentStep = ScienceTurnStep.AwaitingCardSelection;
        }

        public bool SelectCard(ScienceCardData card)
        {
            if (card == null || CurrentStep != ScienceTurnStep.AwaitingCardSelection) return false;

            SelectedCard = card;
            hasSelectedBoardCoordinate = false;
            CurrentStep = card.CardType == ScienceCardType.Character
                ? ScienceTurnStep.AwaitingBoardSlot
                : ScienceTurnStep.ActionResolution;
            telemetry?.LogEvent("science_turn_card_selected", $"turn={TurnNumber};player={CurrentPlayerIndex};card={card.Id};step={CurrentStep}");
            return true;
        }

        public bool SelectBoardSlot(Vector2Int coordinate)
        {
            if (CurrentStep != ScienceTurnStep.AwaitingBoardSlot || SelectedCard == null) return false;

            selectedBoardCoordinate = coordinate;
            hasSelectedBoardCoordinate = true;
            CurrentStep = ScienceTurnStep.AwaitingPlacementConfirmation;
            telemetry?.LogEvent("science_turn_board_slot_selected", $"turn={TurnNumber};player={CurrentPlayerIndex};coord={coordinate};rotation={SelectedRotationDegrees}");
            return true;
        }

        public void RotateSelectedCard(int deltaDegrees)
        {
            if (SelectedCard == null) return;
            SelectedRotationDegrees = ScienceBoardSlotState.NormalizeRotation(SelectedRotationDegrees + deltaDegrees);
            telemetry?.LogEvent("science_turn_card_rotated", $"turn={TurnNumber};player={CurrentPlayerIndex};card={SelectedCard.Id};rotation={SelectedRotationDegrees}");
        }

        public void CancelSelection()
        {
            ClearSelection();
            CurrentStep = ScienceTurnStep.AwaitingCardSelection;
            telemetry?.LogEvent("science_turn_selection_cancelled", $"turn={TurnNumber};player={CurrentPlayerIndex}");
        }

        public void StartConnectionExplanation()
        {
            CurrentStep = ScienceTurnStep.ConnectionExplanation;
            telemetry?.LogEvent("science_turn_connection_explanation_started", $"turn={TurnNumber};player={CurrentPlayerIndex}");
        }

        public void MarkTurnResolved()
        {
            ClearSelection();
            CurrentStep = ScienceTurnStep.TurnResolved;
            telemetry?.LogEvent("science_turn_resolved", $"turn={TurnNumber};player={CurrentPlayerIndex}");
        }

        public void AdvanceTurn()
        {
            if (state == null || state.Players.Count == 0) return;
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % state.Players.Count;
            TurnNumber += 1;
            ClearSelection();
            CurrentStep = ScienceTurnStep.AwaitingCardSelection;
            telemetry?.LogEvent("science_turn_advanced", $"turn={TurnNumber};player={CurrentPlayerIndex};step={CurrentStep}");
        }

        public void Cleanup()
        {
            state = null;
            telemetry = null;
            CurrentPlayerIndex = 0;
            TurnNumber = 0;
            ClearSelection();
            CurrentStep = ScienceTurnStep.AwaitingCardSelection;
        }

        private void ClearSelection()
        {
            SelectedCard = null;
            SelectedRotationDegrees = 0;
            hasSelectedBoardCoordinate = false;
            selectedBoardCoordinate = Vector2Int.zero;
        }
    }
}
