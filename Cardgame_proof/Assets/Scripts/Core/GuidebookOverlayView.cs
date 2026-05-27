using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.Core
{
    public sealed class GuidebookOverlayView : MonoBehaviour
    {
        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.72f);
        private static readonly Color Parchment = new Color(0.95f, 0.89f, 0.78f, 1f);
        private static readonly Color InnerPage = new Color(1f, 0.97f, 0.89f, 1f);
        private static readonly Color Ink = new Color(0.16f, 0.13f, 0.09f, 1f);
        private static readonly Color Divider = new Color(0.54f, 0.41f, 0.24f, 1f);
        private static readonly Color RowNormal = new Color(0.97f, 0.92f, 0.81f, 1f);
        private static readonly Color RowSelected = new Color(0.85f, 0.76f, 0.54f, 1f);

        private GameObject root;
        private RectTransform panelRoot;
        private RectTransform listContentRoot;
        private TextMeshProUGUI detailsText;
        private TextMeshProUGUI hintText;
        private TextMeshProUGUI tokenText;
        private readonly List<CharacterData> currentCharacters = new List<CharacterData>();
        private readonly List<Button> rowButtons = new List<Button>();
        private int selectedIndex = -1;

        public void Initialize(RectTransform parent)
        {
            if (root != null || parent == null) return;
            root = new GameObject("GuidebookOverlay", typeof(RectTransform), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            Image dim = root.GetComponent<Image>();
            dim.color = DimColor;
            dim.raycastTarget = true;

            GameObject panel = new GameObject("GuidebookPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
            panelRoot = panel.GetComponent<RectTransform>();
            panelRoot.SetParent(rt, false);
            panelRoot.anchorMin = panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            panelRoot.sizeDelta = new Vector2(920f, 1500f);
            panelRoot.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().color = Parchment;
            Outline border = panel.GetComponent<Outline>();
            border.effectColor = Divider;
            border.effectDistance = new Vector2(3f, -3f);

            AddRect("Spine", panelRoot, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(46f, 0f), new Color(0.89f, 0.80f, 0.63f, 1f));
            AddRect("InnerPage", panelRoot, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.97f), Vector2.zero, InnerPage);

            RectTransform header = AddRect("Header", panelRoot, new Vector2(0.04f, 0.87f), new Vector2(0.96f, 0.97f), Vector2.zero, new Color(0.93f, 0.85f, 0.69f, 1f));
            CreateText(header, "Guia de Apoio", 48, TextAlignmentOptions.MidlineLeft, Ink, new Vector2(0.03f, 0.5f), new Vector2(0.6f, 1f));
            CreateText(header, "Consulte pistas, áreas e biografias para identificar personagens.", 22, TextAlignmentOptions.BottomLeft, Ink, new Vector2(0.03f, 0f), new Vector2(0.72f, 0.52f));
            tokenText = CreateText(header, "Fichas de Pesquisa restantes: -", 22, TextAlignmentOptions.MidlineLeft, Ink, new Vector2(0.03f, 0.15f), new Vector2(0.72f, 0.48f));

            Button close = CreateButton(header, "Fechar", new Vector2(0.79f, 0.16f), new Vector2(0.97f, 0.84f), new Color(0.66f, 0.29f, 0.22f, 1f));
            close.onClick.AddListener(Hide);

            AddRect("HeaderDivider", panelRoot, new Vector2(0.04f, 0.865f), new Vector2(0.96f, 0.868f), Vector2.zero, Divider);
            AddRect("VerticalDivider", panelRoot, new Vector2(0.39f, 0.08f), new Vector2(0.392f, 0.86f), Vector2.zero, Divider);

            RectTransform listArea = AddRect("ListArea", panelRoot, new Vector2(0.05f, 0.1f), new Vector2(0.37f, 0.85f), Vector2.zero, new Color(0.98f, 0.94f, 0.84f, 1f));
            RectTransform detailsArea = AddRect("DetailsArea", panelRoot, new Vector2(0.41f, 0.1f), new Vector2(0.95f, 0.85f), Vector2.zero, new Color(0.99f, 0.96f, 0.88f, 1f));

            BuildCharacterList(listArea);
            BuildDetailsArea(detailsArea);
            hintText = CreateText(panelRoot, "Use as pistas descobertas para comparar com as informações do guia.", 22, TextAlignmentOptions.Center, Ink, new Vector2(0.08f, 0.035f), new Vector2(0.92f, 0.075f));

            root.SetActive(false);
        }

        public void Show(IReadOnlyList<CharacterData> characters, int remainingResearchTokens)
        {
            if (root == null || characters == null || characters.Count == 0) return;
            root.SetActive(true);
            tokenText.text = $"Fichas de Pesquisa restantes: {remainingResearchTokens}";
            currentCharacters.Clear();
            currentCharacters.AddRange(characters);
            RebuildCharacterRows();
            SelectCharacter(-1);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private void BuildCharacterList(RectTransform parent)
        {
            CreateText(parent, "Personagens", 32, TextAlignmentOptions.MidlineLeft, Ink, new Vector2(0.05f, 0.92f), new Vector2(0.95f, 0.995f));

            GameObject scrollGo = new GameObject("CharacterScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.SetParent(parent, false);
            scrollRt.anchorMin = new Vector2(0.03f, 0.03f); scrollRt.anchorMax = new Vector2(0.97f, 0.9f);
            scrollRt.offsetMin = Vector2.zero; scrollRt.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.SetParent(scrollRt, false);
            viewportRt.anchorMin = Vector2.zero; viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero; viewportRt.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.03f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            listContentRoot = content.GetComponent<RectTransform>();
            listContentRoot.SetParent(viewportRt, false);
            listContentRoot.anchorMin = new Vector2(0f, 1f); listContentRoot.anchorMax = new Vector2(1f, 1f);
            listContentRoot.pivot = new Vector2(0.5f, 1f);
            VerticalLayoutGroup v = content.GetComponent<VerticalLayoutGroup>();
            v.spacing = 8f; v.padding = new RectOffset(8, 8, 8, 8); v.childControlHeight = false; v.childControlWidth = true;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect sr = scrollGo.GetComponent<ScrollRect>();
            sr.viewport = viewportRt;
            sr.content = listContentRoot;
            sr.horizontal = false;
            sr.vertical = true;
        }

        private void BuildDetailsArea(RectTransform parent)
        {
            CreateText(parent, "Registro", 32, TextAlignmentOptions.MidlineLeft, Ink, new Vector2(0.03f, 0.92f), new Vector2(0.95f, 0.995f));

            GameObject detailsScroll = new GameObject("DetailsScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            RectTransform srRt = detailsScroll.GetComponent<RectTransform>();
            srRt.SetParent(parent, false);
            srRt.anchorMin = new Vector2(0.02f, 0.02f); srRt.anchorMax = new Vector2(0.98f, 0.9f);
            srRt.offsetMin = Vector2.zero; srRt.offsetMax = Vector2.zero;
            detailsScroll.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.SetParent(srRt, false);
            viewportRt.anchorMin = Vector2.zero; viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero; viewportRt.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter), typeof(TextMeshProUGUI));
            RectTransform contentRt = content.GetComponent<RectTransform>();
            contentRt.SetParent(viewportRt, false);
            contentRt.anchorMin = new Vector2(0f, 1f); contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.offsetMin = new Vector2(10f, 0f); contentRt.offsetMax = new Vector2(-10f, 0f);
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            detailsText = content.GetComponent<TextMeshProUGUI>();
            detailsText.fontSize = 24;
            detailsText.color = Ink;
            detailsText.alignment = TextAlignmentOptions.TopLeft;
            detailsText.enableWordWrapping = true;
            detailsText.overflowMode = TextOverflowModes.Overflow;

            ScrollRect sr = detailsScroll.GetComponent<ScrollRect>();
            sr.viewport = viewportRt;
            sr.content = contentRt;
            sr.horizontal = false;
            sr.vertical = true;
        }

        private void RebuildCharacterRows()
        {
            foreach (Transform child in listContentRoot) Destroy(child.gameObject);
            rowButtons.Clear();

            for (int i = 0; i < currentCharacters.Count; i++)
            {
                int idx = i;
                CharacterData c = currentCharacters[i];
                GameObject row = new GameObject($"Row_{i}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                row.transform.SetParent(listContentRoot, false);
                row.GetComponent<LayoutElement>().preferredHeight = 130f;
                Image rowImage = row.GetComponent<Image>();
                rowImage.color = RowNormal;
                Button b = row.GetComponent<Button>();
                b.onClick.AddListener(() => SelectCharacter(idx));
                rowButtons.Add(b);

                CreateText(row.GetComponent<RectTransform>(), c.DisplayName, 26, TextAlignmentOptions.TopLeft, Ink, new Vector2(0.04f, 0.55f), new Vector2(0.96f, 0.96f));
                CreateText(row.GetComponent<RectTransform>(), c.Area, 22, TextAlignmentOptions.MidlineLeft, Ink, new Vector2(0.04f, 0.25f), new Vector2(0.96f, 0.56f));
                CreateText(row.GetComponent<RectTransform>(), $"{c.Era} • {c.Region}", 20, TextAlignmentOptions.BottomLeft, Ink, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.28f));
            }
        }

        private void SelectCharacter(int index)
        {
            selectedIndex = index;
            for (int i = 0; i < rowButtons.Count; i++)
            {
                rowButtons[i].GetComponent<Image>().color = i == selectedIndex ? RowSelected : RowNormal;
            }

            if (selectedIndex < 0 || selectedIndex >= currentCharacters.Count)
            {
                detailsText.text = "Selecione um personagem para consultar o registro.\n\nCompare as pistas reveladas com os campos abaixo.";
                return;
            }

            CharacterData c = currentCharacters[selectedIndex];
            detailsText.text =
                $"<b>Compare as pistas reveladas com os campos abaixo.</b>\n\n" +
                $"<b>Nome:</b>\n{c.DisplayName}\n\n" +
                $"<b>Área:</b>\n{c.Area}\n\n" +
                $"<b>Época:</b>\n{c.Era}\n\n" +
                $"<b>Região:</b>\n{c.Region}\n\n" +
                $"<b>Contribuição:</b>\n{c.Contribution}\n\n" +
                $"<b>Contexto/Legado:</b>\n{c.ContextOrLegacy}\n\n" +
                $"<b>Biografia:</b>\n{c.GuidebookBioPtBr}";
        }

        private static RectTransform AddRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            if (sizeDelta != Vector2.zero)
            {
                rt.anchorMax = rt.anchorMin;
                rt.sizeDelta = sizeDelta;
            }
            go.GetComponent<Image>().color = color;
            return rt;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string text, int size, TextAlignmentOptions align, Color color, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            t.enableWordWrapping = true;
            t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 min, Vector2 max, Color bg)
        {
            GameObject go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = bg;
            Button b = go.GetComponent<Button>();

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform lrt = labelObj.GetComponent<RectTransform>();
            lrt.SetParent(rt, false);
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            TextMeshProUGUI t = labelObj.GetComponent<TextMeshProUGUI>();
            t.text = label;
            t.fontSize = 28;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            t.raycastTarget = false;
            return b;
        }
    }
}
