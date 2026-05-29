using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime.Managers
{
    public sealed class ForgottenNamesExpeditionFeedbackAnimator : MonoBehaviour
    {
        public void PlayReveal(GameObject target)
        {
            if (target == null) return;
            StopCoroutineSafe(nameof(RevealRoutine));
            StartCoroutine(RevealRoutine(target));
        }

        public void PulseGraphic(Graphic graphic, Color highlightColor)
        {
            if (graphic == null) return;
            StartCoroutine(PulseGraphicRoutine(graphic, highlightColor));
        }

        public void PulseScale(RectTransform target)
        {
            if (target == null) return;
            StartCoroutine(PulseScaleRoutine(target));
        }

        private void StopCoroutineSafe(string methodName)
        {
            StopCoroutine(methodName);
        }

        private static IEnumerator RevealRoutine(GameObject target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group == null) group = target.AddComponent<CanvasGroup>();

            RectTransform rect = target.GetComponent<RectTransform>();
            Vector3 originalScale = rect != null ? rect.localScale : Vector3.one;
            float duration = 0.14f;
            float elapsed = 0f;
            group.alpha = 0f;
            if (rect != null) rect.localScale = originalScale * 0.985f;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.SmoothStep(0f, 1f, t);
                if (rect != null) rect.localScale = Vector3.LerpUnclamped(originalScale * 0.985f, originalScale, t);
                yield return null;
            }

            if (target != null)
            {
                group.alpha = 1f;
                if (rect != null) rect.localScale = originalScale;
            }
        }

        private static IEnumerator PulseGraphicRoutine(Graphic graphic, Color highlightColor)
        {
            Color originalColor = graphic.color;
            float duration = 0.34f;
            float elapsed = 0f;

            while (elapsed < duration && graphic != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float wave = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
                graphic.color = Color.Lerp(originalColor, highlightColor, wave);
                yield return null;
            }

            if (graphic != null) graphic.color = originalColor;
        }

        private static IEnumerator PulseScaleRoutine(RectTransform target)
        {
            Vector3 originalScale = target.localScale;
            float duration = 0.28f;
            float elapsed = 0f;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float wave = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
                target.localScale = Vector3.LerpUnclamped(originalScale, originalScale * 1.035f, wave);
                yield return null;
            }

            if (target != null) target.localScale = originalScale;
        }
    }
}
