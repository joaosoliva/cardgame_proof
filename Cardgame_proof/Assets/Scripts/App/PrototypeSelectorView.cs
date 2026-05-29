using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.App
{
    public sealed class PrototypeSelectorView : MonoBehaviour
    {
        private RectTransform root;

        public void Show(RectTransform parent, IReadOnlyList<PrototypeDefinition> prototypes, Action<PrototypeDefinition> onSelected, Action onQuit)
        {
            if (parent == null) return;
            if (root == null)
            {
                Build(parent);
            }

            ClearDynamicRows();
            Populate(prototypes, onSelected);
            CreateUtilityButton(root, "Sair", new Vector2(0.5f, 0.10f), onQuit);
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
        }

        public void Hide()
        {
            if (root != null) root.gameObject.SetActive(false);
        }

        private void Build(RectTransform parent)
        {
            GameObject rootObj = new GameObject("PrototypeSelectorRoot", typeof(RectTransform), typeof(Image));
            root = rootObj.GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            rootObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 1f);

            CreateText(root, "Selecionar Protótipo", 58, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.93f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(root, "Escolha qual protótipo deseja abrir. Cada opção é um módulo separado e preserva sua própria interface após iniciar.", 28, new Vector2(0.10f, 0.72f), new Vector2(0.90f, 0.82f), FontStyles.Normal, TextAlignmentOptions.Center);
        }

        private void Populate(IReadOnlyList<PrototypeDefinition> prototypes, Action<PrototypeDefinition> onSelected)
        {
            if (prototypes == null) return;

            bool compactCards = prototypes.Count > 2;
            float startY = compactCards ? 0.60f : 0.58f;
            float step = compactCards ? 0.19f : 0.22f;
            for (int i = 0; i < prototypes.Count; i++)
            {
                PrototypeDefinition definition = prototypes[i];
                CreatePrototypeCard(root, definition, new Vector2(0.5f, startY - (i * step)), onSelected, compactCards);
            }
        }

        private void ClearDynamicRows()
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (child.name.StartsWith("PrototypeCard_", StringComparison.Ordinal) || child.name.StartsWith("SelectorUtilityButton_", StringComparison.Ordinal))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private static void CreatePrototypeCard(RectTransform parent, PrototypeDefinition definition, Vector2 anchor, Action<PrototypeDefinition> onSelected, bool compact)
        {
            GameObject cardObj = new GameObject($"PrototypeCard_{definition.Id}", typeof(RectTransform), typeof(Image), typeof(Outline));
            RectTransform rect = cardObj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = compact ? new Vector2(860f, 160f) : new Vector2(860f, 190f);

            Image background = cardObj.GetComponent<Image>();
            background.color = new Color(0.11f, 0.16f, 0.24f, 1f);

            Outline outline = cardObj.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.12f);
            outline.effectDistance = new Vector2(2f, -2f);

            CreateText(rect, definition.DisplayName, compact ? 30 : 34, new Vector2(0.05f, 0.58f), new Vector2(0.68f, 0.88f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            CreateText(rect, definition.ShortDescription, compact ? 21 : 23, new Vector2(0.05f, 0.14f), new Vector2(0.68f, 0.58f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
            CreateStartButton(rect, "Iniciar", new Vector2(0.82f, 0.50f), () => onSelected?.Invoke(definition), compact);
        }

        private static void CreateStartButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick, bool compact)
        {
            GameObject buttonObj = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = compact ? new Vector2(220f, 82f) : new Vector2(230f, 96f);
            buttonObj.GetComponent<Image>().color = new Color(0.18f, 0.48f, 0.86f, 1f);

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            CreateText(rect, label, compact ? 27 : 30, Vector2.zero, Vector2.one, FontStyles.Bold, TextAlignmentOptions.Center);
        }

        private static void CreateUtilityButton(RectTransform parent, string label, Vector2 anchor, Action onClick)
        {
            GameObject buttonObj = new GameObject($"SelectorUtilityButton_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(420f, 84f);
            buttonObj.GetComponent<Image>().color = new Color(0.24f, 0.27f, 0.33f, 1f);

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => onClick?.Invoke());

            CreateText(rect, label, 28, Vector2.zero, Vector2.one, FontStyles.Bold, TextAlignmentOptions.Center);
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }
    }
}
