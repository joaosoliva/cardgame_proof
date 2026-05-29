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
        Scoring,
        ActionResolution,
        TurnResolved
    }

    public sealed class ScienceTurnManager
    {
        private ScienceCardGameState state;
        private ScienceTelemetryManager telemetry;
        private Vector2Int selectedBoardCoordinate;
        private bool hasSelectedBoardCoordinate;
        private ScienceCardData suspendedCharacterCard;
        private Vector2Int suspendedBoardCoordinate;
        private bool hasSuspendedBoardCoordinate;
        private int suspendedRotationDegrees;

        public int CurrentPlayerIndex { get; private set; }
        public int TurnNumber { get; private set; }
        public ScienceTurnStep CurrentStep { get; private set; } = ScienceTurnStep.AwaitingCardSelection;
        public ScienceCardData SelectedCard { get; private set; }
        public int SelectedRotationDegrees { get; private set; }
        public bool HasPlayedActionThisTurn { get; private set; }
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
            HasPlayedActionThisTurn = false;
            ClearSelection();
            ClearSuspendedCharacterSelection();
            CurrentStep = ScienceTurnStep.AwaitingCardSelection;
        }

        public bool SelectCard(ScienceCardData card)
        {
            if (card == null) return false;

            if (card.CardType == ScienceCardType.Action)
            {
                if (HasPlayedActionThisTurn || !CanSelectActionCardNow()) return false;
                SuspendCharacterSelectionIfNeeded();
                SelectedCard = card;
                hasSelectedBoardCoordinate = false;
                CurrentStep = ScienceTurnStep.ActionResolution;
                telemetry?.LogEvent("science_turn_card_selected", $"turn={TurnNumber};player={CurrentPlayerIndex};card={card.Id};step={CurrentStep};supportAction=true;suspendedCharacter={suspendedCharacterCard?.Id ?? "none"}");
                return true;
            }

            if (CurrentStep != ScienceTurnStep.AwaitingCardSelection || card.CardType != ScienceCardType.Character) return false;

            ClearSuspendedCharacterSelection();
            SelectedCard = card;
            hasSelectedBoardCoordinate = false;
            CurrentStep = ScienceTurnStep.AwaitingBoardSlot;
            telemetry?.LogEvent("science_turn_card_selected", $"turn={TurnNumber};player={CurrentPlayerIndex};card={card.Id};step={CurrentStep}");
            return true;
        }

        public bool SelectBoardSlot(Vector2Int coordinate)
        {
            if ((CurrentStep != ScienceTurnStep.AwaitingBoardSlot && CurrentStep != ScienceTurnStep.AwaitingPlacementConfirmation) || SelectedCard == null) return false;

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
            if (CurrentStep == ScienceTurnStep.ActionResolution && RestoreSuspendedCharacterSelection())
            {
                telemetry?.LogEvent("science_turn_action_cancelled_character_restored", $"turn={TurnNumber};player={CurrentPlayerIndex};card={SelectedCard?.Id ?? "none"};step={CurrentStep}");
                return;
            }

            ClearSelection();
            ClearSuspendedCharacterSelection();
            CurrentStep = ScienceTurnStep.AwaitingCardSelection;
            telemetry?.LogEvent("science_turn_selection_cancelled", $"turn={TurnNumber};player={CurrentPlayerIndex}");
        }

        public void StartConnectionExplanation()
        {
            CurrentStep = ScienceTurnStep.ConnectionExplanation;
            telemetry?.LogEvent("science_turn_connection_explanation_started", $"turn={TurnNumber};player={CurrentPlayerIndex}");
        }

        public void StartScoring()
        {
            CurrentStep = ScienceTurnStep.Scoring;
            telemetry?.LogEvent("science_turn_scoring_started", $"turn={TurnNumber};player={CurrentPlayerIndex};card={SelectedCard?.Id ?? "none"}");
        }

        public void MarkActionPlayedAndContinue()
        {
            HasPlayedActionThisTurn = true;
            bool restoredCharacterSelection = RestoreSuspendedCharacterSelection();
            if (!restoredCharacterSelection)
            {
                ClearSelection();
                CurrentStep = ScienceTurnStep.AwaitingCardSelection;
            }

            telemetry?.LogEvent("science_turn_action_played", $"turn={TurnNumber};player={CurrentPlayerIndex};restoredCharacterSelection={restoredCharacterSelection};step={CurrentStep}");
        }

        public void MarkTurnResolved()
        {
            ClearSelection();
            ClearSuspendedCharacterSelection();
            CurrentStep = ScienceTurnStep.TurnResolved;
            telemetry?.LogEvent("science_turn_resolved", $"turn={TurnNumber};player={CurrentPlayerIndex}");
        }

        public void AdvanceTurn()
        {
            if (state == null || state.Players.Count == 0) return;
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % state.Players.Count;
            TurnNumber += 1;
            HasPlayedActionThisTurn = false;
            ClearSelection();
            ClearSuspendedCharacterSelection();
            CurrentStep = ScienceTurnStep.AwaitingCardSelection;
            telemetry?.LogEvent("science_turn_advanced", $"turn={TurnNumber};player={CurrentPlayerIndex};step={CurrentStep}");
        }

        public void Cleanup()
        {
            state = null;
            telemetry = null;
            CurrentPlayerIndex = 0;
            TurnNumber = 0;
            HasPlayedActionThisTurn = false;
            ClearSelection();
            ClearSuspendedCharacterSelection();
            CurrentStep = ScienceTurnStep.AwaitingCardSelection;
        }

        private bool CanSelectActionCardNow()
        {
            return CurrentStep == ScienceTurnStep.AwaitingCardSelection
                || CurrentStep == ScienceTurnStep.AwaitingBoardSlot
                || CurrentStep == ScienceTurnStep.AwaitingPlacementConfirmation;
        }

        private void SuspendCharacterSelectionIfNeeded()
        {
            if (SelectedCard == null || SelectedCard.CardType != ScienceCardType.Character) return;

            suspendedCharacterCard = SelectedCard;
            suspendedRotationDegrees = SelectedRotationDegrees;
            suspendedBoardCoordinate = selectedBoardCoordinate;
            hasSuspendedBoardCoordinate = hasSelectedBoardCoordinate;
        }

        private bool RestoreSuspendedCharacterSelection()
        {
            if (suspendedCharacterCard == null) return false;

            SelectedCard = suspendedCharacterCard;
            SelectedRotationDegrees = suspendedRotationDegrees;
            selectedBoardCoordinate = suspendedBoardCoordinate;
            hasSelectedBoardCoordinate = hasSuspendedBoardCoordinate;
            CurrentStep = hasSelectedBoardCoordinate
                ? ScienceTurnStep.AwaitingPlacementConfirmation
                : ScienceTurnStep.AwaitingBoardSlot;
            ClearSuspendedCharacterSelection();
            return true;
        }

        private void ClearSuspendedCharacterSelection()
        {
            suspendedCharacterCard = null;
            suspendedRotationDegrees = 0;
            suspendedBoardCoordinate = Vector2Int.zero;
            hasSuspendedBoardCoordinate = false;
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
