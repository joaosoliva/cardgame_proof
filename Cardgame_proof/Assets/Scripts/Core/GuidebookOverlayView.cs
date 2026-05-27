using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardgameProof.Core
{
    public sealed class GuidebookOverlayView : MonoBehaviour
    {
        private GameObject root;
        private RectTransform contentRoot;
        private readonly List<GameObject> entries = new List<GameObject>();

        public void Initialize(RectTransform parent)
        {
            if (root != null || parent == null) return;
            Debug.Log("[UI] Creating GuidebookOverlayView");

            root = new GameObject("GuidebookOverlay", typeof(RectTransform), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.SetParent(rt, false);
            pr.anchorMin = new Vector2(0.06f, 0.06f); pr.anchorMax = new Vector2(0.94f, 0.94f);
            pr.offsetMin = Vector2.zero; pr.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.08f, 0.13f, 0.2f, 1f);
            VerticalLayoutGroup panelLayout = panel.GetComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(20, 20, 20, 20);
            panelLayout.spacing = 12f;
            panelLayout.childControlHeight = false;
            panelLayout.childControlWidth = true;

            CreateText(panel.transform, "Guia de Apoio", 44, 80f, TextAlignmentOptions.Center);

            GameObject scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
            scrollGo.transform.SetParent(panel.transform, false);
            scrollGo.GetComponent<LayoutElement>().preferredHeight = 900f;
            scrollGo.GetComponent<Image>().color = new Color(0.03f, 0.05f, 0.08f, 0.8f);
            RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.SetParent(scrollRt, false);
            viewportRt.anchorMin = Vector2.zero; viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero; viewportRt.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.03f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentRoot = content.GetComponent<RectTransform>();
            contentRoot.SetParent(viewportRt, false);
            contentRoot.anchorMin = new Vector2(0f, 1f); contentRoot.anchorMax = new Vector2(1f, 1f);
            contentRoot.pivot = new Vector2(0.5f, 1f);
            contentRoot.anchoredPosition = Vector2.zero;
            VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(12, 12, 12, 12);
            contentLayout.spacing = 10f;
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollGo.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRt;
            scrollRect.content = contentRoot;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            Button closeButton = CreateButton(panel.transform, "Fechar");
            closeButton.onClick.AddListener(Hide);

            root.SetActive(false);
        }

        public void Show(IReadOnlyList<CharacterData> characters)
        {
            Debug.Log("[Guidebook] Show requested");
            if (root == null)
            {
                Debug.LogWarning("[Guidebook] Show ignored: view not initialized.");
                return;
            }
            if (characters == null || characters.Count == 0)
            {
                Debug.LogWarning("[Guidebook] Show ignored: no character data available.");
                root.SetActive(false);
                return;
            }
            root.SetActive(true);
            foreach (GameObject entry in entries) if (entry != null) Destroy(entry);
            entries.Clear();

            foreach (CharacterData character in characters)
            {
                GameObject box = new GameObject("Entry", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
                box.transform.SetParent(contentRoot, false);
                box.GetComponent<Image>().color = new Color(0.15f, 0.2f, 0.3f, 1f);
                box.GetComponent<LayoutElement>().preferredHeight = 240f;
                VerticalLayoutGroup boxLayout = box.GetComponent<VerticalLayoutGroup>();
                boxLayout.padding = new RectOffset(12, 12, 10, 10);
                boxLayout.spacing = 6f;
                boxLayout.childControlHeight = false;
                boxLayout.childControlWidth = true;

                CreateText(box.transform, character.DisplayName, 30, 52f, TextAlignmentOptions.MidlineLeft);
                CreateText(box.transform, $"Área: {character.Area}", 26, 44f, TextAlignmentOptions.MidlineLeft);
                CreateText(box.transform, character.GuidebookBioPtBr, 24, 132f, TextAlignmentOptions.TopLeft);
                entries.Add(box);
            }
        }

        public void Hide()
        {
            Debug.Log("[Guidebook] Hide requested");
            if (root != null) root.SetActive(false);
        }

        private static TextMeshProUGUI CreateText(Transform parent, string value, int size, float height, TextAlignmentOptions anchor)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(LayoutElement), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        private static Button CreateButton(Transform parent, string label)
        {
            GameObject go = new GameObject("Button", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 92f;
            go.GetComponent<Image>().color = new Color(0.16f, 0.43f, 0.84f, 1f);
            Button button = go.GetComponent<Button>();

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform lr = labelObj.GetComponent<RectTransform>();
            lr.SetParent(go.transform, false); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            TextMeshProUGUI text = labelObj.GetComponent<TextMeshProUGUI>();
            text.fontSize = 32;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.text = label;
            return button;
        }
    }
}
