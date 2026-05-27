using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardgameProof.Core
{
    public sealed class TutorialOverlayView : MonoBehaviour
    {
        private RectTransform panelRoot;
        private GameObject panelObject;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI bodyText;
        private Button confirmButton;
        private Image highlightFrame;
        private CanvasGroup blocker;
        private string currentStepId;
        private RectTransform cardRoot;

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
            currentStepId = step.Id;
            titleText.text = step.Title;
            bodyText.text = step.Body;
            confirmButton.gameObject.SetActive(showContinueButton);
            confirmButton.onClick.RemoveAllListeners();
            if (showContinueButton && onContinue != null)
            {
                confirmButton.onClick.AddListener(() =>
                {
                    Debug.Log($"[TUTORIAL] Continue clicked: {currentStepId}");
                    onContinue();
                });
            }
            blocker.alpha = 1f;
            blocker.interactable = blockOutsideTarget;
            blocker.blocksRaycasts = blockOutsideTarget;
            PositionCardNearTarget(target);
            UpdateHighlight(target);
            LogTutorialUiState();
        }

        public void Hide() { if (panelObject != null) { blocker.alpha = 0f; blocker.interactable = false; blocker.blocksRaycasts = false; highlightFrame.gameObject.SetActive(false); panelObject.SetActive(false); } }

        private void BuildUi(RectTransform parent)
        {
            panelObject = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.SetParent(parent, false);
            panelRoot.anchorMin = Vector2.zero; panelRoot.anchorMax = Vector2.one; panelRoot.offsetMin = Vector2.zero; panelRoot.offsetMax = Vector2.zero;
            panelObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.52f);
            blocker = panelObject.GetComponent<CanvasGroup>();

            var frameObj = new GameObject("TargetHighlight", typeof(RectTransform), typeof(Image));
            frameObj.transform.SetParent(panelRoot, false);
            highlightFrame = frameObj.GetComponent<Image>();
            highlightFrame.color = new Color(1f, 0.85f, 0.2f, 0.18f);
            highlightFrame.raycastTarget = false;
            var outline = frameObj.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.2f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);

            GameObject card = new GameObject("TutorialCard", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRoot = cardRect;
            cardRect.SetParent(panelRoot, false);
            cardRect.anchorMin = new Vector2(0.1f, 0.08f); cardRect.anchorMax = new Vector2(0.9f, 0.34f);
            cardRect.offsetMin = Vector2.zero; cardRect.offsetMax = Vector2.zero;
            Image cardImage = card.GetComponent<Image>();
            cardImage.color = new Color(0.94f, 0.96f, 1f, 1f);
            cardImage.raycastTarget = true;
            CanvasGroup cardCg = card.GetComponent<CanvasGroup>();
            cardCg.interactable = true;
            cardCg.blocksRaycasts = true;
            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 18, 18); layout.spacing = 12f; layout.childControlHeight = false; layout.childControlWidth = true;

            titleText = CreateText(cardRect, "Title", 42, TextAlignmentOptions.Center, Color.black, 72f);
            bodyText = CreateText(cardRect, "Body", 30, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.12f, 0.12f), 150f);
            confirmButton = CreateButton(cardRect, "Entendi", 86f);
        }

        private void PositionCardNearTarget(RectTransform target)
        {
            // Keep simple, portrait-safe fixed card position for robustness.
        }

        private void UpdateHighlight(RectTransform target)
        {
            if (target == null) { highlightFrame.gameObject.SetActive(false); return; }
            highlightFrame.gameObject.SetActive(true);
            RectTransform tr = target;
            Vector3[] corners = new Vector3[4];
            tr.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRoot, min, null, out Vector2 localMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRoot, max, null, out Vector2 localMax);
            RectTransform hr = highlightFrame.rectTransform;
            hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0.5f);
            hr.sizeDelta = new Vector2(Mathf.Abs(localMax.x - localMin.x) + 24f, Mathf.Abs(localMax.y - localMin.y) + 24f);
            hr.anchoredPosition = (localMin + localMax) * 0.5f;
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
            buttonObj.GetComponent<Image>().color = new Color(0.19f, 0.46f, 0.88f, 1f);
            Button button = buttonObj.GetComponent<Button>();
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
            Debug.Log($"[TUTORIAL_UI] highlight sibling={highlightFrame.rectTransform.GetSiblingIndex()} active={highlightFrame.gameObject.activeSelf} raycast={highlightFrame.raycastTarget}");
            Debug.Log($"[TUTORIAL_UI] card sibling={cardRoot.GetSiblingIndex()} raycast={cardRoot.GetComponent<Image>().raycastTarget}");
            Debug.Log($"[TUTORIAL_UI] continue sibling={confirmButton.transform.GetSiblingIndex()} interactable={confirmButton.interactable} raycast={(buttonImage != null && buttonImage.raycastTarget)}");
            Debug.Log($"[TUTORIAL_UI] blocker alpha={blocker.alpha} interactable={blocker.interactable} blocks={blocker.blocksRaycasts}");
        }

    }
}
