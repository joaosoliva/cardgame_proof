using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardgameProof.Core
{
    public enum BoardCellVisualState
    {
        Empty,
        OccupiedSetup,
        Hidden,
        Revealed
    }

    public sealed class BoardCellView : MonoBehaviour
    {
        private Image background;
        private TextMeshProUGUI debugLabel;
        private TextMeshProUGUI hiddenLabel;
        private Button tapButton;
        private bool isSelected;
        private BoardCellVisualState currentState = BoardCellVisualState.Empty;

        public Vector2Int Coordinate { get; private set; }

        public void Initialize(Vector2Int coordinate, bool showDebugLabels, Action<Vector2Int> onTap)
        {
            Coordinate = coordinate;

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = gameObject.AddComponent<RectTransform>();
            }

            background = gameObject.GetComponent<Image>();
            if (background == null)
            {
                background = gameObject.AddComponent<Image>();
            }

            tapButton = gameObject.GetComponent<Button>();
            if (tapButton == null)
            {
                tapButton = gameObject.AddComponent<Button>();
            }

            tapButton.onClick.RemoveAllListeners();
            tapButton.onClick.AddListener(() => onTap?.Invoke(Coordinate));

            if (showDebugLabels)
            {
                GameObject labelObj = new GameObject("DebugLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.SetParent(transform, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                debugLabel = labelObj.GetComponent<TextMeshProUGUI>();
                debugLabel.alignment = TextAlignmentOptions.Center;
                debugLabel.fontSize = 24;
                debugLabel.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
                debugLabel.text = $"{coordinate.x},{coordinate.y}";
            }

            GameObject hiddenObj = new GameObject("HiddenLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform hiddenRect = hiddenObj.GetComponent<RectTransform>();
            hiddenRect.SetParent(transform, false);
            hiddenRect.anchorMin = Vector2.zero; hiddenRect.anchorMax = Vector2.one;
            hiddenRect.offsetMin = Vector2.zero; hiddenRect.offsetMax = Vector2.zero;
            hiddenLabel = hiddenObj.GetComponent<TextMeshProUGUI>();
            hiddenLabel.text = "?";
            hiddenLabel.alignment = TextAlignmentOptions.Center;
            hiddenLabel.fontSize = 40;
            hiddenLabel.color = new Color(0.95f, 0.95f, 0.98f, 1f);
            hiddenLabel.gameObject.SetActive(false);

            SetState(BoardCellVisualState.Empty, true);
        }

        public void SetState(BoardCellVisualState state, bool showGridLines)
        {
            if (background == null)
            {
                return;
            }
            currentState = state;

            Color color;
            switch (state)
            {
                case BoardCellVisualState.OccupiedSetup:
                    color = new Color(0.41f, 0.75f, 0.45f, 1f);
                    break;
                case BoardCellVisualState.Hidden:
                    color = new Color(0.2f, 0.24f, 0.35f, 1f);
                    break;
                case BoardCellVisualState.Revealed:
                    color = new Color(0.94f, 0.84f, 0.4f, 1f);
                    break;
                default:
                    color = new Color(0.93f, 0.94f, 0.97f, 1f);
                    break;
            }

            background.color = color;
            if (hiddenLabel != null)
            {
                hiddenLabel.gameObject.SetActive(state == BoardCellVisualState.Hidden);
            }

            Outline outline = gameObject.GetComponent<Outline>();
            if (showGridLines)
            {
                if (outline == null)
                {
                    outline = gameObject.AddComponent<Outline>();
                }

                outline.effectColor = isSelected ? new Color(0.98f, 0.83f, 0.29f, 1f) : new Color(0f, 0f, 0f, 0.35f);
                outline.effectDistance = new Vector2(1f, -1f);
            }
            else if (outline != null)
            {
                Destroy(outline);
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            SetState(currentState, true);
        }
    }
}
