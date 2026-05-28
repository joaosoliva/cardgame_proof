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

        public void Show(RectTransform parent, IReadOnlyList<PrototypeDefinition> prototypes, Action<PrototypeDefinition> onSelected)
        {
            if (parent == null) return;
            if (root == null)
            {
                Build(parent);
            }

            ClearPrototypeRows();
            Populate(prototypes, onSelected);
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

            CreateText(root, "Selecionar Protótipo", 58, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.92f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(root, "Escolha qual protótipo deseja abrir. O protótipo existente permanece disponível como um módulo separado.", 28, new Vector2(0.10f, 0.70f), new Vector2(0.90f, 0.80f), FontStyles.Normal, TextAlignmentOptions.Center);
        }

        private void Populate(IReadOnlyList<PrototypeDefinition> prototypes, Action<PrototypeDefinition> onSelected)
        {
            if (prototypes == null) return;

            float startY = 0.58f;
            float step = 0.18f;
            for (int i = 0; i < prototypes.Count; i++)
            {
                PrototypeDefinition definition = prototypes[i];
                CreatePrototypeButton(root, definition, new Vector2(0.5f, startY - (i * step)), onSelected);
            }
        }

        private void ClearPrototypeRows()
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (child.name.StartsWith("PrototypeButton_", StringComparison.Ordinal))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private static void CreatePrototypeButton(RectTransform parent, PrototypeDefinition definition, Vector2 anchor, Action<PrototypeDefinition> onSelected)
        {
            GameObject buttonObj = new GameObject($"PrototypeButton_{definition.Id}", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(820f, 150f);
            buttonObj.GetComponent<Image>().color = new Color(0.17f, 0.36f, 0.65f, 1f);

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => onSelected?.Invoke(definition));

            CreateText(rect, definition.DisplayName, 34, new Vector2(0.05f, 0.50f), new Vector2(0.95f, 0.88f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            CreateText(rect, definition.ShortDescription, 23, new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.50f), FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
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
            return text;
        }
    }
}
