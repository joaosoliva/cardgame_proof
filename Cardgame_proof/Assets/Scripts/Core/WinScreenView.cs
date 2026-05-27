using System;
using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.Core
{
    public sealed class WinScreenView : MonoBehaviour
    {
        private GameObject root;
        private Text titleText;
        private Text detailsText;
        private Button reportButton;
        private Button playAgainButton;
        private Button menuButton;

        public void Initialize(RectTransform parent)
        {
            if (root != null || parent == null) return;

            root = new GameObject("WinScreen", typeof(RectTransform), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(VerticalLayoutGroup));
            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.SetParent(rt, false);
            pr.anchorMin = new Vector2(0.1f, 0.2f); pr.anchorMax = new Vector2(0.9f, 0.8f);
            pr.offsetMin = Vector2.zero; pr.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;

            titleText = CreateText(panel.transform, "Vitória", 56, 120f);
            detailsText = CreateText(panel.transform, "Detalhes", 32, 220f);
            reportButton = CreateButton(panel.transform, "Ver Relatório");
            playAgainButton = CreateButton(panel.transform, "Jogar novamente");
            menuButton = CreateButton(panel.transform, "Voltar ao menu");

            root.SetActive(false);
        }

        public void Show(string title, string details, Action onReport, Action onPlayAgain, Action onBackMenu)
        {
            if (root == null) return;
            root.SetActive(true);
            titleText.text = title;
            detailsText.text = details;

            reportButton.onClick.RemoveAllListeners();
            reportButton.onClick.AddListener(() => onReport?.Invoke());
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(() => onPlayAgain?.Invoke());
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => onBackMenu?.Invoke());
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private static Text CreateText(Transform parent, string value, int size, float height)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(LayoutElement), typeof(Text));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        private static Button CreateButton(Transform parent, string label)
        {
            GameObject go = new GameObject(label + "Button", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 96f;
            go.GetComponent<Image>().color = new Color(0.16f, 0.43f, 0.84f, 1f);
            Button b = go.GetComponent<Button>();

            GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            RectTransform tr = textObj.GetComponent<RectTransform>();
            tr.SetParent(go.transform, false); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            Text t = textObj.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 34;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.text = label;
            return b;
        }
    }
}
