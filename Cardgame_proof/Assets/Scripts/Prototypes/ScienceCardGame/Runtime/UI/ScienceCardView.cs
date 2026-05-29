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
                case ScienceFactCategory.LifeSciences:
                    return new Color(0.16f, 0.72f, 0.32f, 1f);
                case ScienceFactCategory.PhysicalSciences:
                    return new Color(0.15f, 0.48f, 0.95f, 1f);
                case ScienceFactCategory.MathAndComputation:
                    return new Color(0.96f, 0.78f, 0.16f, 1f);
                case ScienceFactCategory.TechnologyAndInvention:
                    return new Color(0.92f, 0.34f, 0.16f, 1f);
                case ScienceFactCategory.SocietyAndEducation:
                    return new Color(0.70f, 0.34f, 0.92f, 1f);
                default:
                    return new Color(0.55f, 0.58f, 0.64f, 1f);
            }
        }


        public static string GetFactCategoryLabel(ScienceFactCategory category)
        {
            switch (category)
            {
                case ScienceFactCategory.LifeSciences:
                    return "Life";
                case ScienceFactCategory.PhysicalSciences:
                    return "Physical";
                case ScienceFactCategory.MathAndComputation:
                    return "Math/Comp";
                case ScienceFactCategory.TechnologyAndInvention:
                    return "Tech";
                case ScienceFactCategory.SocietyAndEducation:
                    return "Society";
                default:
                    return category.ToString();
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
            background.color = new Color(0.03f, 0.04f, 0.06f, 1f);
            BuildFactColorBase(characterCard.FactCategoryA, characterCard.FactCategoryB);

            if (displayMode == ScienceCardViewDisplayMode.ZoomModal)
            {
                CreateTextBacking("NameBacking", new Vector2(0.06f, 0.74f), new Vector2(0.94f, 0.97f), 0.70f);
                CreateText("Name", characterCard.DisplayName, GetTitleSize(), new Vector2(0.07f, 0.75f), new Vector2(0.93f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
                CreateTextBacking("FieldBacking", new Vector2(0.07f, 0.61f), new Vector2(0.93f, 0.75f), 0.54f);
                CreateText("Field", characterCard.Field, GetBodySize(), new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.74f), FontStyles.Italic, TextAlignmentOptions.Center);
                CreateBadge("FactBadgeA", characterCard.FactCategoryA, new Vector2(0.08f, 0.48f), new Vector2(0.48f, 0.60f));
                CreateBadge("FactBadgeB", characterCard.FactCategoryB, new Vector2(0.52f, 0.48f), new Vector2(0.92f, 0.60f));
                CreateTextBacking("DescriptionBacking", new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.46f), 0.68f);
                CreateText("Description", characterCard.ShortDescription, GetBodySize(), new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.45f), FontStyles.Normal, TextAlignmentOptions.Top);
                return;
            }

            CreateTextBacking("NameBacking", new Vector2(0.05f, 0.56f), new Vector2(0.95f, 0.97f), 0.68f);
            CreateText("Name", characterCard.DisplayName, GetTitleSize(), new Vector2(0.07f, 0.58f), new Vector2(0.93f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateTextBacking("FieldBacking", new Vector2(0.07f, 0.36f), new Vector2(0.93f, 0.57f), 0.48f);
            CreateText("Field", characterCard.Field, GetBodySize(), new Vector2(0.08f, 0.39f), new Vector2(0.92f, 0.56f), FontStyles.Italic, TextAlignmentOptions.Center);
            CreateBadge("FactBadgeA", characterCard.FactCategoryA, new Vector2(0.08f, 0.10f), new Vector2(0.48f, 0.33f));
            CreateBadge("FactBadgeB", characterCard.FactCategoryB, new Vector2(0.52f, 0.10f), new Vector2(0.92f, 0.33f));
        }

        private void BuildActionCard(ScienceActionCardData actionCard)
        {
            background.color = new Color(0.05f, 0.03f, 0.08f, 1f);
            BuildActionColorBase();

            if (displayMode == ScienceCardViewDisplayMode.ZoomModal)
            {
                CreateTextBacking("NameBacking", new Vector2(0.06f, 0.75f), new Vector2(0.94f, 0.97f), 0.72f);
                CreateText("Name", actionCard.DisplayName, GetTitleSize(), new Vector2(0.07f, 0.76f), new Vector2(0.93f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
                CreateText("ActionLabel", "AÇÃO", GetBodySize(), new Vector2(0.18f, 0.62f), new Vector2(0.82f, 0.74f), FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.98f, 0.84f, 1f, 1f));
                CreateText("EffectType", $"{GetActionTimingLabel(actionCard.TimingType)} · {actionCard.EffectType}", GetBodySize(), new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.61f), FontStyles.Italic, TextAlignmentOptions.Center);
                string rules = string.IsNullOrEmpty(actionCard.RulesText) ? actionCard.ShortDescription : actionCard.RulesText;
                CreateTextBacking("RulesBacking", new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.48f), 0.68f);
                CreateText("RulesText", rules, GetBodySize(), new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.47f), FontStyles.Normal, TextAlignmentOptions.Top);
                return;
            }

            CreateTextBacking("NameBacking", new Vector2(0.05f, 0.56f), new Vector2(0.95f, 0.97f), 0.70f);
            CreateText("Name", actionCard.DisplayName, GetTitleSize(), new Vector2(0.07f, 0.58f), new Vector2(0.93f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText("ActionLabel", "AÇÃO", GetBodySize(), new Vector2(0.18f, 0.36f), new Vector2(0.82f, 0.53f), FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.98f, 0.84f, 1f, 1f));
            CreateText("EffectType", $"{GetActionTimingLabel(actionCard.TimingType)} · {actionCard.EffectType}", GetBodySize(), new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.32f), FontStyles.Italic, TextAlignmentOptions.Center);
        }

        private void BuildFactColorBase(ScienceFactCategory categoryA, ScienceFactCategory categoryB)
        {
            Color colorA = GetFactCategoryColor(categoryA);
            Color colorB = GetFactCategoryColor(categoryB);
            bool sameCategory = categoryA == categoryB;

            if (sameCategory)
            {
                CreateColorPanel("FactColorBase", Vector2.zero, Vector2.one, colorA);
                return;
            }

            bool verticalCard = displayMode == ScienceCardViewDisplayMode.ZoomModal;
            if (verticalCard)
            {
                CreateColorPanel("FactColorTop", new Vector2(0f, 0.50f), Vector2.one, colorA);
                CreateColorPanel("FactColorBottom", Vector2.zero, new Vector2(1f, 0.50f), colorB);
            }
            else
            {
                CreateColorPanel("FactColorLeft", Vector2.zero, new Vector2(0.50f, 1f), colorA);
                CreateColorPanel("FactColorRight", new Vector2(0.50f, 0f), Vector2.one, colorB);
            }
        }

        private void BuildActionColorBase()
        {
            CreateColorPanel("ActionBase", Vector2.zero, Vector2.one, new Color(0.25f, 0.12f, 0.38f, 1f));
            CreateColorPanel("ActionAccent", new Vector2(0f, 0.68f), Vector2.one, new Color(0.48f, 0.20f, 0.72f, 0.92f));
            CreateColorPanel("ActionFooter", Vector2.zero, new Vector2(1f, 0.18f), new Color(0.10f, 0.07f, 0.15f, 0.78f));
        }

        private RectTransform CreateColorPanel(string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.SetParent(transform, false);
            panelRect.anchorMin = anchorMin;
            panelRect.anchorMax = anchorMax;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = color;
            panelImage.raycastTarget = false;
            return panelRect;
        }

        private RectTransform CreateTextBacking(string name, Vector2 anchorMin, Vector2 anchorMax, float alpha)
        {
            return CreateColorPanel(name, anchorMin, anchorMax, new Color(0.02f, 0.025f, 0.03f, alpha));
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
                    return 15;
                default:
                    return 24;
            }
        }

        private int GetBodySize()
        {
            switch (displayMode)
            {
                case ScienceCardViewDisplayMode.ZoomModal:
                    return 24;
                case ScienceCardViewDisplayMode.Board:
                    return 11;
                default:
                    return 17;
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
            badgeImage.raycastTarget = false;

            CreateText(badgeRect, "Label", GetFactCategoryLabel(category), GetBadgeTextSize(), Vector2.zero, Vector2.one, FontStyles.Bold, TextAlignmentOptions.Center, Color.black);
        }

        private int GetBadgeTextSize()
        {
            switch (displayMode)
            {
                case ScienceCardViewDisplayMode.ZoomModal:
                    return 20;
                case ScienceCardViewDisplayMode.Board:
                    return 8;
                default:
                    return 12;
            }
        }

        private static string GetActionTimingLabel(ScienceActionTimingType timingType)
        {
            switch (timingType)
            {
                case ScienceActionTimingType.Immediate:
                    return "Imediata";
                case ScienceActionTimingType.Prepared:
                    return "Preparada";
                default:
                    return timingType.ToString();
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
