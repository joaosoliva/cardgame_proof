using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardgameProof.Core
{
    public enum TutorialTrigger
    {
        ContinueButton,
        CharacterCardPlaced,
        ArchiveCardPlaced,
        AnyCardPlaced,
        AllRequiredCardsPlaced,
        SetupConfirmed,
        AutoNoRecordFillCompleted,
        GuideOpened,
        RulesOpened,
        CellInvestigated,
        CharacterFound,
        ArchiveFound,
        NoRecordFound,
        ClueSelected,
        GuessMade,
        TurnPassed
    }

    public sealed class TutorialManager
    {
        private readonly TutorialOverlayView overlay;
        private readonly Dictionary<string, RectTransform> targets = new Dictionary<string, RectTransform>();
        private readonly HashSet<string> shown = new HashSet<string>();
        private List<TutorialStep> steps = new List<TutorialStep>();
        private int index;

        public TutorialManager(TutorialOverlayView overlayView) { overlay = overlayView; }

        public void RegisterTarget(string key, RectTransform target)
        {
            if (string.IsNullOrWhiteSpace(key) || target == null) return;
            targets[key] = target;
        }

        public void StartSequence(IReadOnlyList<TutorialStep> sequence)
        {
            steps = sequence == null ? new List<TutorialStep>() : new List<TutorialStep>(sequence);
            index = 0;
            ShowCurrent();
        }

        public void Notify(TutorialTrigger trigger)
        {
            if (steps == null || index >= steps.Count) return;
            var step = steps[index];
            if (step.CompleteTrigger != trigger) return;
            Advance();
        }

        private void Advance()
        {
            if (index < steps.Count && steps[index].OnlyShowOnce) shown.Add(steps[index].Id);
            index++;
            ShowCurrent();
        }

        private void ShowCurrent()
        {
            while (index < steps.Count)
            {
                var step = steps[index];
                if (step.OnlyShowOnce && shown.Contains(step.Id)) { index++; continue; }

                RectTransform target = null;
                if (!string.IsNullOrWhiteSpace(step.TargetKey))
                {
                    targets.TryGetValue(step.TargetKey, out target);
                    if (target == null) Debug.LogWarning($"[TUTORIAL] Target not found for step {step.Id}: {step.TargetKey}");
                }

                bool requireContinue = step.ShowContinueButton || target == null;
                overlay.ShowStep(step, requireContinue, requireContinue ? (Action)(() => Notify(TutorialTrigger.ContinueButton)) : null, target, step.BlockOutsideTarget);
                return;
            }

            overlay.Hide();
        }
    }
}
