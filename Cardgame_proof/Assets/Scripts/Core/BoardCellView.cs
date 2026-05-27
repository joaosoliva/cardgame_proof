using System;
using UnityEngine;
using UnityEngine.UI;

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
        private Text debugLabel;
        private Button tapButton;

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
                GameObject labelObj = new GameObject("DebugLabel", typeof(RectTransform), typeof(Text));
                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.SetParent(transform, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                debugLabel = labelObj.GetComponent<Text>();
                debugLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                debugLabel.alignment = TextAnchor.MiddleCenter;
                debugLabel.fontSize = 24;
                debugLabel.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
                debugLabel.text = $"{coordinate.x},{coordinate.y}";
            }

            SetState(BoardCellVisualState.Empty, true);
        }

        public void SetState(BoardCellVisualState state, bool showGridLines)
        {
            if (background == null)
            {
                return;
            }

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

            Outline outline = gameObject.GetComponent<Outline>();
            if (showGridLines)
            {
                if (outline == null)
                {
                    outline = gameObject.AddComponent<Outline>();
                }

                outline.effectColor = new Color(0f, 0f, 0f, 0.35f);
                outline.effectDistance = new Vector2(1f, -1f);
            }
            else if (outline != null)
            {
                Destroy(outline);
            }
        }
    }
}
