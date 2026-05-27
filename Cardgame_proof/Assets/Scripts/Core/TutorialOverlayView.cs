using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardgameProof.Core
{
    public sealed class TutorialOverlayView : MonoBehaviour
    {
        private readonly HashSet<string> shownStepIds = new HashSet<string>();

        private RectTransform panelRoot;
        private GameObject panelObject;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI bodyText;
        private Button confirmButton;

        private IReadOnlyList<TutorialStep> currentSequence;
        private int currentIndex;
        private Action onClosed;

        public bool IsVisible => panelObject != null && panelObject.activeSelf;

        public void Initialize(RectTransform overlayParent)
        {
            if (overlayParent == null)
            {
                Debug.LogWarning("TutorialOverlayView initialization skipped: overlay parent is null.");
                return;
            }

            if (panelObject != null)
            {
                return;
            }

            BuildUi(overlayParent);
            Hide();
        }

        public void ShowSequence(IReadOnlyList<TutorialStep> sequence, Action onSequenceClosed = null)
        {
            if (sequence == null || sequence.Count == 0)
            {
                return;
            }

            if (panelObject == null)
            {
                Debug.LogWarning("TutorialOverlayView.ShowSequence called before Initialize.");
                return;
            }

            currentSequence = sequence;
            currentIndex = 0;
            onClosed = onSequenceClosed;
            ShowCurrentStep();
        }

        public void Hide()
        {
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }

            currentSequence = null;
            currentIndex = 0;
        }

        private void BuildUi(RectTransform parent)
        {
            panelObject = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(Image));
            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.SetParent(parent, false);
            panelRoot.anchorMin = Vector2.zero;
            panelRoot.anchorMax = Vector2.one;
            panelRoot.offsetMin = Vector2.zero;
            panelRoot.offsetMax = Vector2.zero;

            Image dim = panelObject.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.52f);
            dim.raycastTarget = true;

            GameObject card = new GameObject("TutorialCard", typeof(RectTransform), typeof(Image));
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.SetParent(panelRoot, false);
            cardRect.anchorMin = new Vector2(0.08f, 0.28f);
            cardRect.anchorMax = new Vector2(0.92f, 0.72f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            Image cardImage = card.GetComponent<Image>();
            cardImage.color = new Color(0.94f, 0.96f, 1f, 1f);

            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(36, 36, 36, 36);
            layout.spacing = 24f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            titleText = CreateText(cardRect, "Title", 50, TextAlignmentOptions.Center, Color.black, 130f);
            bodyText = CreateText(cardRect, "Body", 36, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.12f, 0.12f), 330f);

            confirmButton = CreateButton(cardRect, "Entendi", 120f);
            confirmButton.onClick.AddListener(AdvanceOrClose);
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string name, int fontSize, TextAlignmentOptions alignment, Color color, float preferredHeight)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(LayoutElement), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(parent, false);

            LayoutElement layout = textObj.GetComponent<LayoutElement>();
            layout.preferredHeight = preferredHeight;

            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.alignment = alignment;
            text.fontSize = fontSize;
            text.color = color;
            text.horizontalOverflow = TextWrappingModes.Normal;
            text.verticalOverflow = TextOverflowModes.Overflow;
            return text;
        }

        private static Button CreateButton(RectTransform parent, string label, float preferredHeight)
        {
            GameObject buttonObj = new GameObject("ConfirmButton", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(parent, false);

            LayoutElement layout = buttonObj.GetComponent<LayoutElement>();
            layout.preferredHeight = preferredHeight;

            Image image = buttonObj.GetComponent<Image>();
            image.color = new Color(0.19f, 0.46f, 0.88f, 1f);

            Button button = buttonObj.GetComponent<Button>();

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.SetParent(buttonObj.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 40;
            labelText.color = Color.white;

            return button;
        }

        private void ShowCurrentStep()
        {
            TutorialStep step = GetNextVisibleStep();
            if (step == null)
            {
                CloseSequence();
                return;
            }

            panelObject.SetActive(true);
            titleText.text = step.Title;
            bodyText.text = step.Body;
        }

        private TutorialStep GetNextVisibleStep()
        {
            while (currentSequence != null && currentIndex < currentSequence.Count)
            {
                TutorialStep step = currentSequence[currentIndex];
                if (step.OnlyShowOnce && shownStepIds.Contains(step.Id))
                {
                    currentIndex++;
                    continue;
                }

                return step;
            }

            return null;
        }

        private void AdvanceOrClose()
        {
            if (currentSequence == null || currentIndex >= currentSequence.Count)
            {
                CloseSequence();
                return;
            }

            TutorialStep step = currentSequence[currentIndex];
            if (step.OnlyShowOnce)
            {
                shownStepIds.Add(step.Id);
            }

            currentIndex++;
            if (currentIndex >= currentSequence.Count)
            {
                CloseSequence();
                return;
            }

            ShowCurrentStep();
        }

        private void CloseSequence()
        {
            Hide();
            Action closeCallback = onClosed;
            onClosed = null;
            closeCallback?.Invoke();
        }
    }
}
