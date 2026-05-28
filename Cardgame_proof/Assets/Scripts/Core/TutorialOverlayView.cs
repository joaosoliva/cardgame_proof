using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace CardgameProof.Core
{
    public sealed class TutorialOverlayView : MonoBehaviour
    {
        private RectTransform panelRoot;
        private GameObject panelObject;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI bodyText;
        private Button confirmButton;
        private RectTransform highlightRoot;
        private Image highlightTop;
        private Image highlightBottom;
        private Image highlightLeft;
        private Image highlightRight;
        private CanvasGroup highlightCanvasGroup;
        private CanvasGroup blocker;
        private string currentStepId;
        private RectTransform cardRoot;
        private CanvasGroup cardCanvasGroup;
        private Coroutine fadeCoroutine;
        private bool debugWelcomeStepActive;
        private RectTransform activeHighlightTarget;
        private Coroutine pulseCoroutine;

        private readonly Color outlineColor = new Color(1f, 0.82f, 0.22f, 1f);
        private const float outlineThickness = 8f;
        private const float outlinePadding = 16f;
        private const bool pulseEnabled = true;
        private const float pulseSpeed = 2.2f;

        public bool IsVisible => panelObject != null && panelObject.activeSelf;

        public void Initialize(RectTransform overlayParent)
        {
            if (overlayParent == null) { Debug.LogWarning("TutorialOverlayView initialization skipped: overlay parent is null."); return; }
            if (panelObject != null) return;
            BuildUi(overlayParent);
            Hide();
        }

        public void ShowStep(TutorialStep step, bool showContinueButton, Action onContinue, RectTransform target, bool blockOutsideTarget)
        {
            if (panelObject == null || step == null) return;
            panelObject.SetActive(true);
            panelRoot.SetAsLastSibling();
            currentStepId = step.Id;
            titleText.text = step.Title;
            bodyText.text = step.Body;
            confirmButton.gameObject.SetActive(showContinueButton);
            confirmButton.onClick.RemoveAllListeners();
            if (showContinueButton && onContinue != null)
            {
                confirmButton.onClick.AddListener(() =>
                {
                    Debug.Log("[TUTORIAL_CLICK_TEST] Entendi clicked");
                    Debug.Log($"[TUTORIAL] Continue clicked: {currentStepId}");
                    Debug.Log($"[TUTORIAL] Selected GO: {(EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.name : "null")}");
                    onContinue();
                });
            }

            SetCompact(step.CompactMode || !showContinueButton);
            SetPlacement(step.PreferredPlacement, target, step.AvoidTargetOverlap);
            cardRoot.SetAsLastSibling();

            blocker.alpha = 1f;
            blocker.interactable = blockOutsideTarget;
            blocker.blocksRaycasts = blockOutsideTarget;
            Image blockerImage = panelObject.GetComponent<Image>();
            if (blockerImage != null) blockerImage.raycastTarget = blockOutsideTarget;
            cardCanvasGroup.alpha = 1f;
            cardCanvasGroup.interactable = true;
            cardCanvasGroup.blocksRaycasts = true;
            UpdateHighlight(target);
            LogTutorialUiState();
            debugWelcomeStepActive = string.Equals(step.Id, "setup_intro", StringComparison.OrdinalIgnoreCase) ||
                                     step.Title.Contains("Bem-vindo", StringComparison.OrdinalIgnoreCase);
            if (debugWelcomeStepActive)
            {
                ForceWelcomeInputSafety();
                DumpWelcomeDiagnostics();
            }
        }

        public void Hide()
        {
            if (panelObject == null) return;
            blocker.alpha = 0f; blocker.interactable = false; blocker.blocksRaycasts = false;
            Image blockerImage = panelObject.GetComponent<Image>();
            if (blockerImage != null) blockerImage.raycastTarget = false;
            cardCanvasGroup.alpha = 0f; cardCanvasGroup.interactable = false; cardCanvasGroup.blocksRaycasts = false;
            SetHighlightActive(false);
            panelObject.SetActive(false);
            debugWelcomeStepActive = false;
        }

        private void LateUpdate()
        {
            if (activeHighlightTarget != null && highlightRoot != null && highlightRoot.gameObject.activeSelf)
            {
                PositionOutlineAroundTarget(activeHighlightTarget);
            }
        }

        public void SetPlacement(TutorialPanelPlacement placement, RectTransform target = null, bool avoidTargetOverlap = true)
        {
            if (cardRoot == null) return;
            TutorialPanelPlacement resolved = placement;
            if (placement == TutorialPanelPlacement.Auto || placement == TutorialPanelPlacement.NearTarget)
            {
                resolved = ResolveAutoPlacement(target);
            }

            ApplyPlacement(resolved);
            if (avoidTargetOverlap && target != null && DoesOverlapTarget(target))
            {
                ApplyPlacement(TutorialPanelPlacement.Top);
            }
        }

        public void FadeTo(float alpha, float duration)
        {
            if (cardCanvasGroup == null) return;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(alpha, duration));
        }

        public void SetCompact(bool compact)
        {
            if (cardRoot == null) return;
            cardRoot.sizeDelta = compact ? new Vector2(760f, 260f) : new Vector2(920f, 420f);
            titleText.fontSize = compact ? 34 : 42;
            bodyText.fontSize = compact ? 24 : 30;
        }

        public void TemporarilyFadeDuringAction()
        {
            FadeTo(0.32f, 0.15f);
            cardCanvasGroup.interactable = false;
            cardCanvasGroup.blocksRaycasts = false;
        }

        public void RestoreAfterActionIfStillActive()
        {
            if (!IsVisible) return;
            FadeTo(1f, 0.15f);
            cardCanvasGroup.interactable = confirmButton.gameObject.activeSelf;
            cardCanvasGroup.blocksRaycasts = confirmButton.gameObject.activeSelf;
        }

        private IEnumerator FadeRoutine(float alpha, float duration)
        {
            float from = cardCanvasGroup.alpha;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                cardCanvasGroup.alpha = Mathf.Lerp(from, alpha, t / duration);
                yield return null;
            }
            cardCanvasGroup.alpha = alpha;
            if (alpha < 0.4f)
            {
                cardCanvasGroup.interactable = false;
                cardCanvasGroup.blocksRaycasts = false;
            }
        }

        private TutorialPanelPlacement ResolveAutoPlacement(RectTransform target)
        {
            if (target == null) return TutorialPanelPlacement.Top;
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            float centerY = (corners[0].y + corners[2].y) * 0.5f;
            float screenH = Screen.height;
            if (centerY < screenH * 0.35f) return TutorialPanelPlacement.Top;
            if (centerY > screenH * 0.65f) return TutorialPanelPlacement.Bottom;
            return TutorialPanelPlacement.Top;
        }

        private void ApplyPlacement(TutorialPanelPlacement placement)
        {
            cardRoot.anchorMin = cardRoot.anchorMax = new Vector2(0.5f, 0.5f);
            switch (placement)
            {
                case TutorialPanelPlacement.Center: cardRoot.anchoredPosition = new Vector2(0f, 0f); break;
                case TutorialPanelPlacement.Bottom: cardRoot.anchoredPosition = new Vector2(0f, -560f); break;
                case TutorialPanelPlacement.LowerBoard: cardRoot.anchoredPosition = new Vector2(0f, -330f); break;
                case TutorialPanelPlacement.UpperBoard: cardRoot.anchoredPosition = new Vector2(0f, 250f); break;
                case TutorialPanelPlacement.Top: default: cardRoot.anchoredPosition = new Vector2(0f, 540f); break;
            }
        }

        private bool DoesOverlapTarget(RectTransform target)
        {
            Vector3[] tc = new Vector3[4]; target.GetWorldCorners(tc);
            Vector3[] cc = new Vector3[4]; cardRoot.GetWorldCorners(cc);
            Rect tr = new Rect(tc[0], tc[2] - tc[0]);
            Rect cr = new Rect(cc[0], cc[2] - cc[0]);
            return tr.Overlaps(cr);
        }

        private void BuildUi(RectTransform parent)
        {
            panelObject = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.SetParent(parent, false);
            panelRoot.anchorMin = Vector2.zero; panelRoot.anchorMax = Vector2.one; panelRoot.offsetMin = Vector2.zero; panelRoot.offsetMax = Vector2.zero;
            panelObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.42f);
            blocker = panelObject.GetComponent<CanvasGroup>();

            var highlightObj = new GameObject("TutorialHighlightRoot", typeof(RectTransform), typeof(CanvasGroup));
            highlightRoot = highlightObj.GetComponent<RectTransform>();
            highlightRoot.SetParent(panelRoot, false);
            highlightRoot.anchorMin = highlightRoot.anchorMax = new Vector2(0.5f, 0.5f);
            highlightCanvasGroup = highlightObj.GetComponent<CanvasGroup>();
            highlightCanvasGroup.interactable = false;
            highlightCanvasGroup.blocksRaycasts = false;
            highlightTop = CreateHighlightBorder("TopBorder", highlightRoot);
            highlightBottom = CreateHighlightBorder("BottomBorder", highlightRoot);
            highlightLeft = CreateHighlightBorder("LeftBorder", highlightRoot);
            highlightRight = CreateHighlightBorder("RightBorder", highlightRoot);

            GameObject card = new GameObject("TutorialCard", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            cardRoot = card.GetComponent<RectTransform>();
            cardRoot.SetParent(panelRoot, false);
            cardRoot.sizeDelta = new Vector2(920f, 420f);
            Image cardImage = card.GetComponent<Image>();
            cardImage.color = new Color(0.94f, 0.96f, 1f, 1f);
            cardImage.raycastTarget = true;
            cardCanvasGroup = card.GetComponent<CanvasGroup>();
            cardCanvasGroup.interactable = true;
            cardCanvasGroup.blocksRaycasts = true;
            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 18, 18); layout.spacing = 12f; layout.childControlHeight = false; layout.childControlWidth = true;

            titleText = CreateText(cardRoot, "Title", 42, TextAlignmentOptions.Center, Color.black, 72f);
            bodyText = CreateText(cardRoot, "Body", 30, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.12f, 0.12f), 150f);
            confirmButton = CreateButton(cardRoot, "Entendi", 86f);
        }

        private void UpdateHighlight(RectTransform target)
        {
            activeHighlightTarget = target;
            if (target == null) { SetHighlightActive(false); return; }
            SetHighlightActive(true);
            PositionOutlineAroundTarget(target);
        }

        private void PositionOutlineAroundTarget(RectTransform target)
        {
            if (target == null || panelRoot == null || highlightRoot == null) return;
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRoot, min, null, out Vector2 localMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRoot, max, null, out Vector2 localMax);
            float width = Mathf.Abs(localMax.x - localMin.x) + (outlinePadding * 2f);
            float height = Mathf.Abs(localMax.y - localMin.y) + (outlinePadding * 2f);
            Vector2 center = (localMin + localMax) * 0.5f;
            highlightRoot.sizeDelta = new Vector2(width, height);
            highlightRoot.anchoredPosition = center;
            PositionBorder(highlightTop.rectTransform, new Vector2(0f, (height * 0.5f) - (outlineThickness * 0.5f)), new Vector2(width, outlineThickness));
            PositionBorder(highlightBottom.rectTransform, new Vector2(0f, (-height * 0.5f) + (outlineThickness * 0.5f)), new Vector2(width, outlineThickness));
            PositionBorder(highlightLeft.rectTransform, new Vector2((-width * 0.5f) + (outlineThickness * 0.5f), 0f), new Vector2(outlineThickness, height));
            PositionBorder(highlightRight.rectTransform, new Vector2((width * 0.5f) - (outlineThickness * 0.5f), 0f), new Vector2(outlineThickness, height));
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string name, int fontSize, TextAlignmentOptions alignment, Color color, float preferredHeight)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(LayoutElement), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(parent, false);
            textObj.GetComponent<LayoutElement>().preferredHeight = preferredHeight;
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.alignment = alignment; text.fontSize = fontSize; text.color = color; text.enableWordWrapping = true; text.overflowMode = TextOverflowModes.Overflow; return text;
        }

        private static Button CreateButton(RectTransform parent, string label, float preferredHeight)
        {
            GameObject buttonObj = new GameObject("ConfirmButton", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(parent, false);
            buttonObj.GetComponent<LayoutElement>().preferredHeight = preferredHeight;
            Image buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.color = new Color(0.19f, 0.46f, 0.88f, 1f);
            buttonImage.raycastTarget = true;
            Button button = buttonObj.GetComponent<Button>();
            button.interactable = true;

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>(); labelRect.SetParent(buttonObj.transform, false); labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one; labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>(); labelText.text = label; labelText.alignment = TextAlignmentOptions.Center; labelText.fontSize = 32; labelText.color = Color.white;
            labelText.raycastTarget = false;
            return button;
        }

        private void LogTutorialUiState()
        {
            if (panelRoot == null || confirmButton == null) return;
            Image dimImage = panelObject.GetComponent<Image>();
            Image buttonImage = confirmButton.GetComponent<Image>();
            Debug.Log($"[TUTORIAL_UI] dim sibling={panelRoot.GetSiblingIndex()} raycast={(dimImage != null && dimImage.raycastTarget)} blocks={blocker.blocksRaycasts}");
            Debug.Log($"[TUTORIAL_UI] highlight sibling={(highlightRoot != null ? highlightRoot.GetSiblingIndex() : -1)} active={(highlightRoot != null && highlightRoot.gameObject.activeSelf)} blocks={(highlightCanvasGroup != null && highlightCanvasGroup.blocksRaycasts)}");
            Debug.Log($"[TUTORIAL_UI] card sibling={cardRoot.GetSiblingIndex()} raycast={cardRoot.GetComponent<Image>().raycastTarget}");
            Debug.Log($"[TUTORIAL_UI] continue sibling={confirmButton.transform.GetSiblingIndex()} interactable={confirmButton.interactable} raycast={(buttonImage != null && buttonImage.raycastTarget)}");
            Debug.Log($"[TUTORIAL_UI] blocker alpha={blocker.alpha} interactable={blocker.interactable} blocks={blocker.blocksRaycasts}");
        }

        private void ForceWelcomeInputSafety()
        {
            // Welcome/continue step must block gameplay behind the tutorial.
            blocker.interactable = true;
            blocker.blocksRaycasts = true;
            Image blockerImage = panelObject.GetComponent<Image>();
            if (blockerImage != null) blockerImage.raycastTarget = true;
            if (highlightCanvasGroup != null) { highlightCanvasGroup.interactable = false; highlightCanvasGroup.blocksRaycasts = false; }
            confirmButton.interactable = true;
            Image bi = confirmButton.GetComponent<Image>();
            if (bi != null) bi.raycastTarget = true;
            TextMeshProUGUI lbl = confirmButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (lbl != null) lbl.raycastTarget = false;
            cardCanvasGroup.alpha = 1f;
            cardCanvasGroup.interactable = true;
            cardCanvasGroup.blocksRaycasts = true;
        }

        private void DumpWelcomeDiagnostics()
        {
            Debug.Log($"[TUTORIAL_DIAG] Button={confirmButton.name} interactable={confirmButton.interactable} activeSelf={confirmButton.gameObject.activeSelf} activeInHierarchy={confirmButton.gameObject.activeInHierarchy}");
            Image bi = confirmButton.GetComponent<Image>();
            TextMeshProUGUI lbl = confirmButton.GetComponentInChildren<TextMeshProUGUI>(true);
            Debug.Log($"[TUTORIAL_DIAG] ButtonImage.raycastTarget={(bi != null && bi.raycastTarget)} Label.raycastTarget={(lbl != null && lbl.raycastTarget)}");
            Debug.Log($"[TUTORIAL_DIAG] Button parent={(confirmButton.transform.parent != null ? confirmButton.transform.parent.name : "null")} sibling={confirmButton.transform.GetSiblingIndex()}");
            DumpUiRaycastTree(confirmButton.gameObject);
            DumpOverlayChildren();
        }

        private void DumpUiRaycastTree(GameObject target)
        {
            Transform t = target != null ? target.transform : null;
            while (t != null)
            {
                CanvasGroup cg = t.GetComponent<CanvasGroup>();
                Image img = t.GetComponent<Image>();
                Canvas canvas = t.GetComponent<Canvas>();
                Debug.Log($"[UI_TREE] name={t.name} activeSelf={t.gameObject.activeSelf} activeInHierarchy={t.gameObject.activeInHierarchy} cg={(cg!=null)} alpha={(cg!=null?cg.alpha:-1)} interactable={(cg!=null&&cg.interactable)} blocks={(cg!=null&&cg.blocksRaycasts)} ignoreParent={(cg!=null&&cg.ignoreParentGroups)} img={(img!=null)} raycast={(img!=null&&img.raycastTarget)} canvas={(canvas!=null)} order={(canvas!=null?canvas.sortingOrder:-1)} sibling={t.GetSiblingIndex()}");
                t = t.parent;
            }
        }

        private void DumpOverlayChildren()
        {
            if (panelRoot == null || panelRoot.parent == null) return;
            Transform parent = panelRoot.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform c = parent.GetChild(i);
                CanvasGroup cg = c.GetComponent<CanvasGroup>();
                Image img = c.GetComponent<Image>();
                RectTransform rt = c as RectTransform;
                Vector2 size = rt != null ? rt.rect.size : Vector2.zero;
                bool large = size.x >= Screen.width * 0.75f && size.y >= Screen.height * 0.75f;
                Debug.Log($"[UI_LAYER] name={c.name} active={c.gameObject.activeInHierarchy} sibling={c.GetSiblingIndex()} alpha={(cg!=null?cg.alpha:-1)} blocks={(cg!=null&&cg.blocksRaycasts)} raycast={(img!=null&&img.raycastTarget)} size={size} large={large}");
            }
        }

        private Image CreateHighlightBorder(string name, RectTransform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            Image img = go.GetComponent<Image>();
            img.color = outlineColor;
            img.raycastTarget = false;
            return img;
        }

        private static void PositionBorder(RectTransform border, Vector2 pos, Vector2 size)
        {
            border.anchoredPosition = pos;
            border.sizeDelta = size;
        }

        private void SetHighlightActive(bool active)
        {
            if (highlightRoot == null) return;
            highlightRoot.gameObject.SetActive(active);
            if (!active)
            {
                activeHighlightTarget = null;
                if (pulseCoroutine != null) { StopCoroutine(pulseCoroutine); pulseCoroutine = null; }
                highlightRoot.localScale = Vector3.one;
                return;
            }
            if (pulseEnabled && pulseCoroutine == null)
            {
                pulseCoroutine = StartCoroutine(PulseOutline());
            }
        }

        private IEnumerator PulseOutline()
        {
            while (highlightRoot != null && highlightRoot.gameObject.activeSelf)
            {
                float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
                float scale = Mathf.Lerp(1f, 1.05f, t);
                highlightRoot.localScale = new Vector3(scale, scale, 1f);
                if (highlightCanvasGroup != null) highlightCanvasGroup.alpha = Mathf.Lerp(0.8f, 1f, t);
                yield return null;
            }
            pulseCoroutine = null;
        }
    }
}
