using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.UI
{
    public enum ScienceCardViewDisplayMode
    {
        Hand,
        Board,
        ZoomModal
    }

    public sealed class ScienceCardView : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Image background;
        private Button button;
        private ScienceCardData cardData;
        private ScienceCardViewDisplayMode displayMode;
        private Action<ScienceCardData> onSelected;

        public ScienceCardData CardData => cardData;
        public ScienceCardViewDisplayMode DisplayMode => displayMode;

        public static ScienceCardView Create(RectTransform parent, string objectName, ScienceCardData data, ScienceCardViewDisplayMode mode, Action<ScienceCardData> onClick = null)
        {
            GameObject cardObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(ScienceCardView));
            RectTransform rect = cardObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            ScienceCardView view = cardObject.GetComponent<ScienceCardView>();
            view.SetData(data, mode);
            view.SetOnSelected(onClick);
            return view;
        }

        public void SetData(ScienceCardData data, ScienceCardViewDisplayMode mode)
        {
            cardData = data;
            displayMode = mode;
            EnsureReferences();
            ClearChildren(transform);
            ApplyModeSize();
            BuildVisualContent();
            SetOnSelected(onSelected);
        }

        public void SetDisplayMode(ScienceCardViewDisplayMode mode)
        {
            SetData(cardData, mode);
        }

        public void SetOnSelected(Action<ScienceCardData> onClick)
        {
            onSelected = onClick;
            EnsureReferences();

            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.interactable = onSelected != null && cardData != null;
            button.targetGraphic = background;
            if (button.interactable)
            {
                button.onClick.AddListener(() => onSelected?.Invoke(cardData));
            }
        }

        public static Color GetFactCategoryColor(ScienceFactCategory category)
        {
            switch (category)
            {
                case ScienceFactCategory.Observation:
                    return new Color(0.20f, 0.56f, 0.90f, 1f);
                case ScienceFactCategory.Experimentation:
                    return new Color(0.95f, 0.55f, 0.20f, 1f);
                case ScienceFactCategory.Theory:
                    return new Color(0.58f, 0.42f, 0.86f, 1f);
                case ScienceFactCategory.Technology:
                    return new Color(0.20f, 0.74f, 0.70f, 1f);
                case ScienceFactCategory.Society:
                    return new Color(0.93f, 0.38f, 0.55f, 1f);
                case ScienceFactCategory.Environment:
                    return new Color(0.28f, 0.68f, 0.32f, 1f);
                case ScienceFactCategory.Health:
                    return new Color(0.86f, 0.25f, 0.28f, 1f);
                case ScienceFactCategory.Mathematics:
                    return new Color(0.96f, 0.82f, 0.23f, 1f);
                default:
                    return new Color(0.55f, 0.58f, 0.64f, 1f);
            }
        }

        private void EnsureReferences()
        {
            rectTransform = GetComponent<RectTransform>();
            background = GetComponent<Image>();
            button = GetComponent<Button>();
        }

        private void ApplyModeSize()
        {
            if (rectTransform == null) return;

            switch (displayMode)
            {
                case ScienceCardViewDisplayMode.Board:
                    rectTransform.sizeDelta = new Vector2(104f, 90f);
                    break;
                case ScienceCardViewDisplayMode.ZoomModal:
                    rectTransform.sizeDelta = new Vector2(420f, 560f);
                    break;
                default:
                    rectTransform.sizeDelta = new Vector2(190f, 170f);
                    break;
            }
        }

        private void BuildVisualContent()
        {
            if (cardData is ScienceCharacterCardData characterCard)
            {
                BuildCharacterCard(characterCard);
                return;
            }

            if (cardData is ScienceActionCardData actionCard)
            {
                BuildActionCard(actionCard);
                return;
            }

            BuildEmptyCard();
        }

        private void BuildCharacterCard(ScienceCharacterCardData characterCard)
        {
            background.color = new Color(0.17f, 0.29f, 0.39f, 1f);
            CreateText("Name", characterCard.DisplayName, GetTitleSize(), new Vector2(0.07f, 0.75f), new Vector2(0.93f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText("Field", characterCard.Field, GetBodySize(), new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.74f), FontStyles.Italic, TextAlignmentOptions.Center);
            CreateBadge("FactBadgeA", characterCard.FactCategoryA, new Vector2(0.08f, 0.48f), new Vector2(0.48f, 0.60f));
            CreateBadge("FactBadgeB", characterCard.FactCategoryB, new Vector2(0.52f, 0.48f), new Vector2(0.92f, 0.60f));
            CreateText("Description", characterCard.ShortDescription, GetBodySize(), new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.45f), FontStyles.Normal, TextAlignmentOptions.Top);
        }

        private void BuildActionCard(ScienceActionCardData actionCard)
        {
            background.color = new Color(0.31f, 0.20f, 0.43f, 1f);
            CreateText("Name", actionCard.DisplayName, GetTitleSize(), new Vector2(0.07f, 0.76f), new Vector2(0.93f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText("ActionLabel", "AÇÃO", GetBodySize(), new Vector2(0.18f, 0.62f), new Vector2(0.82f, 0.74f), FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.92f, 0.74f, 1f, 1f));
            CreateText("EffectType", actionCard.EffectType.ToString(), GetBodySize(), new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.61f), FontStyles.Italic, TextAlignmentOptions.Center);
            string rules = string.IsNullOrEmpty(actionCard.RulesText) ? actionCard.ShortDescription : actionCard.RulesText;
            CreateText("RulesText", rules, GetBodySize(), new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.47f), FontStyles.Normal, TextAlignmentOptions.Top);
        }

        private void BuildEmptyCard()
        {
            background.color = new Color(0.18f, 0.20f, 0.24f, 1f);
            CreateText("Empty", "Carta", GetTitleSize(), new Vector2(0.08f, 0.35f), new Vector2(0.92f, 0.65f), FontStyles.Bold, TextAlignmentOptions.Center);
        }

        private int GetTitleSize()
        {
            switch (displayMode)
            {
                case ScienceCardViewDisplayMode.ZoomModal:
                    return 34;
                case ScienceCardViewDisplayMode.Board:
                    return 14;
                default:
                    return 22;
            }
        }

        private int GetBodySize()
        {
            switch (displayMode)
            {
                case ScienceCardViewDisplayMode.ZoomModal:
                    return 24;
                case ScienceCardViewDisplayMode.Board:
                    return 10;
                default:
                    return 16;
            }
        }

        private void CreateBadge(string name, ScienceFactCategory category, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject badgeObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
            badgeRect.SetParent(transform, false);
            badgeRect.anchorMin = anchorMin;
            badgeRect.anchorMax = anchorMax;
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;

            Image badgeImage = badgeObject.GetComponent<Image>();
            badgeImage.color = GetFactCategoryColor(category);

            CreateText(badgeRect, "Label", category.ToString(), GetBadgeTextSize(), Vector2.zero, Vector2.one, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);
        }

        private int GetBadgeTextSize()
        {
            switch (displayMode)
            {
                case ScienceCardViewDisplayMode.ZoomModal:
                    return 20;
                case ScienceCardViewDisplayMode.Board:
                    return 7;
                default:
                    return 11;
            }
        }

        private TextMeshProUGUI CreateText(string name, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style, TextAlignmentOptions alignment)
        {
            return CreateText(rectTransform, name, value, size, anchorMin, anchorMax, style, alignment, Color.white);
        }

        private TextMeshProUGUI CreateText(string name, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style, TextAlignmentOptions alignment, Color color)
        {
            return CreateText(rectTransform, name, value, size, anchorMin, anchorMax, style, alignment, color);
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string name, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style, TextAlignmentOptions alignment, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(parent, false);
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }
        }
    }
}
