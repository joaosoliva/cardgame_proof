using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardgameProof.Core
{
    public sealed class ReadyScreenView : MonoBehaviour
    {
        private GameObject root;
        private TextMeshProUGUI messageText;
        private Button actionButton;

        public void Initialize(RectTransform parent)
        {
            if (root != null || parent == null) return;

            root = new GameObject("ReadyScreen", typeof(RectTransform), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0f,0f,0f,0.82f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(VerticalLayoutGroup));
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.SetParent(rt, false);
            prt.anchorMin = new Vector2(0.08f,0.3f); prt.anchorMax = new Vector2(0.92f,0.7f);
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

            VerticalLayoutGroup v = panel.GetComponent<VerticalLayoutGroup>();
            v.spacing = 24f; v.childControlHeight = false; v.childControlWidth = true;

            messageText = CreateText(panel.transform, "Mensagem", 56);
            actionButton = CreateButton(panel.transform, "Ação");

            root.SetActive(false);
        }

        public void Show(string text, string buttonText, Action onClick)
        {
            if (root == null) return;
            root.SetActive(true);
            messageText.text = text;
            TextMeshProUGUI label = actionButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = buttonText;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => onClick?.Invoke());
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private static TextMeshProUGUI CreateText(Transform parent, string text, int size)
        {
            GameObject go = new GameObject("Message", typeof(RectTransform), typeof(LayoutElement), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 180f;
            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.alignment = TextAlignmentOptions.Center;
            return t;
        }

        private static Button CreateButton(Transform parent, string text)
        {
            GameObject go = new GameObject("Button", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 128f;
            go.GetComponent<Image>().color = new Color(0.16f,0.43f,0.84f,1f);
            Button b = go.GetComponent<Button>();

            GameObject l = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform lr = l.GetComponent<RectTransform>();
            lr.SetParent(go.transform, false); lr.anchorMin=Vector2.zero; lr.anchorMax=Vector2.one;
            TextMeshProUGUI t = l.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = 40;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            return b;
        }
    }
}
