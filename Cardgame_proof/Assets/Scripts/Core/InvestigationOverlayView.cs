using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.Core
{
    public sealed class InvestigationOverlayView : MonoBehaviour
    {
        private GameObject root;
        private Text titleText;
        private Text bodyText;
        private RectTransform contentRoot;
        private readonly List<GameObject> dynamicRows = new List<GameObject>();

        public void Initialize(RectTransform parent)
        {
            if (root != null || parent == null) return;

            root = new GameObject("InvestigationOverlay", typeof(RectTransform), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.SetParent(rt, false);
            pr.anchorMin = new Vector2(0.1f, 0.08f); pr.anchorMax = new Vector2(0.9f, 0.92f);
            pr.offsetMin = Vector2.zero; pr.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.08f, 0.13f, 0.2f, 1f);

            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 16f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;

            titleText = CreateText(panel.transform, "Título", 48, 100f);
            bodyText = CreateText(panel.transform, string.Empty, 30, 140f);

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            contentRoot = content.GetComponent<RectTransform>();
            contentRoot.SetParent(panel.transform, false);
            VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 10f;
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;

            root.SetActive(false);
        }

        public void Show(string title, string body)
        {
            if (root == null) return;
            root.SetActive(true);
            titleText.text = title;
            bodyText.text = body;
            ClearDynamicRows();
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        public Button AddButton(string label, Action onClick, bool interactable = true)
        {
            GameObject go = new GameObject(label + "Button", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Button));
            go.transform.SetParent(contentRoot, false);
            go.GetComponent<LayoutElement>().preferredHeight = 90f;
            go.GetComponent<Image>().color = interactable ? new Color(0.16f, 0.43f, 0.84f, 1f) : new Color(0.35f, 0.35f, 0.35f, 1f);
            Button button = go.GetComponent<Button>();
            button.interactable = interactable;
            button.onClick.AddListener(() => onClick?.Invoke());

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            RectTransform lr = labelObj.GetComponent<RectTransform>();
            lr.SetParent(go.transform, false); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            Text text = labelObj.GetComponent<Text>();
            text.text = label; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 28; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
            dynamicRows.Add(go);
            return button;
        }

        public void AddLabel(string text)
        {
            Text row = CreateText(contentRoot, text, 28, 70f);
            row.alignment = TextAnchor.MiddleLeft;
            dynamicRows.Add(row.gameObject);
        }

        private void ClearDynamicRows()
        {
            foreach (GameObject row in dynamicRows) if (row != null) Destroy(row);
            dynamicRows.Clear();
        }

        private static Text CreateText(Transform parent, string text, int size, float height)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(LayoutElement), typeof(Text));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            Text t = go.GetComponent<Text>();
            t.text = text; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
            return t;
        }
    }
}
