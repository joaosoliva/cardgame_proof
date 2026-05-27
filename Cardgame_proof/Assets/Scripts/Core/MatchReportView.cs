using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardgameProof.Core
{
    public sealed class MatchReportView : MonoBehaviour
    {
        private GameObject root;
        private TextMeshProUGUI bodyText;
        private Button copyButton;
        private Button backButton;
        private string currentSummary;

        public void Initialize(RectTransform parent)
        {
            if (root != null) return;
            root = new GameObject("MatchReport", typeof(RectTransform), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(VerticalLayoutGroup));
            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.SetParent(rt, false); pr.anchorMin = new Vector2(0.05f, 0.05f); pr.anchorMax = new Vector2(0.95f, 0.95f);

            bodyText = CreateText(panel.transform, "Relatório", 24, 1200f, TextAlignmentOptions.TopLeft);
            copyButton = CreateButton(panel.transform, "Copiar resumo");
            backButton = CreateButton(panel.transform, "Voltar ao resultado");
            root.SetActive(false);
        }
        public void Show(string summary, Action onBack)
        {
            root.SetActive(true);
            currentSummary = summary;
            bodyText.text = summary;
            copyButton.onClick.RemoveAllListeners();
            copyButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = currentSummary ?? string.Empty);
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => onBack?.Invoke());
        }
        public void Hide() { if (root != null) root.SetActive(false); }

        private static TextMeshProUGUI CreateText(Transform parent, string value, int size, float height, TextAlignmentOptions anchor)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(LayoutElement), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            return t;
        }
        private static Button CreateButton(Transform parent, string label)
        {
            GameObject go = new GameObject("Button", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false); go.GetComponent<LayoutElement>().preferredHeight = 90f; go.GetComponent<Image>().color = new Color(0.16f, 0.43f, 0.84f, 1f);
            Button b = go.GetComponent<Button>();
            TextMeshProUGUI t = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            t.rectTransform.SetParent(go.transform, false); t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
            t.fontSize = 28;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            t.text = label;
            return b;
        }
    }
}
