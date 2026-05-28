using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.Core
{
    [Serializable]
    public sealed class HowToPlayPageData
    {
        public string PageId;
        public string Title;
        [TextArea(4, 12)] public string Body;
        public Sprite Illustration;
    }

    public sealed class HowToPlayView : MonoBehaviour
    {
        private RectTransform root;
        private CanvasGroup canvasGroup;
        private Image illustrationImage;
        private GameObject placeholderRoot;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI bodyText;
        private TextMeshProUGUI pageCounterText;
        private Button previousButton;
        private Button nextButton;
        private Button closeButton;
        private Button playPrototypeButton;

        private readonly List<HowToPlayPageData> pages = new List<HowToPlayPageData>();
        private int currentIndex;
        private Action onClose;
        private Action onPlayPrototype;

        public void Initialize(RectTransform parent)
        {
            root = CreateRect("HowToPlayRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            canvasGroup = root.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = root.gameObject.AddComponent<CanvasGroup>();
            root.SetAsLastSibling();

            var dim = CreateImage("DimBackground", root, new Color(0f, 0f, 0f, 0.62f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            dim.raycastTarget = true;

            RectTransform panel = CreateRect("MainPanel", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(920f, 1600f));
            panel.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.96f, 0.94f, 0.88f, 1f);

            closeButton = CreateButton("CloseButton", panel, "X Fechar", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(190f, 90f), new Vector2(-16f, -16f), 34f);
            closeButton.onClick.AddListener(() => onClose?.Invoke());

            RectTransform imageArea = CreateRect("ImageArea", panel, new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.9f), new Vector2(0.5f, 0.5f), Vector2.zero);
            imageArea.GetComponent<Image>().color = new Color(0.88f, 0.86f, 0.78f, 1f);

            illustrationImage = CreateImage("Illustration", imageArea, Color.white, new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.97f), Vector2.zero, Vector2.zero);
            illustrationImage.preserveAspect = true;

            placeholderRoot = new GameObject("Placeholder", typeof(RectTransform), typeof(Image));
            RectTransform ph = placeholderRoot.GetComponent<RectTransform>();
            ph.SetParent(imageArea, false);
            ph.anchorMin = new Vector2(0.03f, 0.03f); ph.anchorMax = new Vector2(0.97f, 0.97f); ph.offsetMin = Vector2.zero; ph.offsetMax = Vector2.zero;
            placeholderRoot.GetComponent<Image>().color = new Color(0.78f, 0.76f, 0.7f, 1f);
            CreateTMP("PlaceholderText", ph, "Imagem ilustrativa", 42f, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(1f, 1f));

            titleText = CreateTMP("TitleText", panel, "", 56f, TextAlignmentOptions.TopLeft, new Vector2(0.08f, 0.39f), new Vector2(0.92f, 0.47f));
            bodyText = CreateTMP("BodyText", panel, "", 35f, TextAlignmentOptions.TopLeft, new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.37f));
            bodyText.enableWordWrapping = true;

            previousButton = CreateButton("PreviousButton", panel, "◀ Anterior", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(230f, 90f), new Vector2(28f, 128f), 32f);
            nextButton = CreateButton("NextButton", panel, "Próxima ▶", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(230f, 90f), new Vector2(-28f, 128f), 32f);
            pageCounterText = CreateTMP("PageCounterText", panel, "1 / 1", 34f, TextAlignmentOptions.Center, new Vector2(0.35f, 0.07f), new Vector2(0.65f, 0.13f));
            playPrototypeButton = CreateButton("PlayPrototypeButton", panel, "Jogar Protótipo", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(500f, 100f), new Vector2(0f, 20f), 36f);

            previousButton.onClick.AddListener(() => GoToPage(currentIndex - 1));
            nextButton.onClick.AddListener(() => GoToPage(currentIndex + 1));
            playPrototypeButton.onClick.AddListener(() => onPlayPrototype?.Invoke());

            Debug.Log("[HOW_TO_PLAY] Created");
            Hide();
        }

        public void Show(IReadOnlyList<HowToPlayPageData> allPages, Action closeAction, Action playPrototypeAction)
        {
            Debug.Log("[HOW_TO_PLAY] Show called");
            pages.Clear();
            if (allPages != null) pages.AddRange(allPages);
            onClose = closeAction;
            onPlayPrototype = playPrototypeAction;
            currentIndex = 0;
            root.gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            root.SetAsLastSibling();
            GoToPage(0);
            Debug.Log($"[HOW_TO_PLAY] Active: {root.gameObject.activeSelf}");
            Debug.Log($"[HOW_TO_PLAY] Alpha: {(canvasGroup != null ? canvasGroup.alpha : -1f)}");
            Debug.Log($"[HOW_TO_PLAY] Parent: {(root.parent != null ? root.parent.name : "null")}");
            Debug.Log($"[HOW_TO_PLAY] Sibling index: {root.GetSiblingIndex()}");
            Debug.Log($"[HOW_TO_PLAY] Page count: {pages.Count}");
        }

        public void Hide()
        {
            if (root == null) return;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            root.gameObject.SetActive(false);
        }

        private void GoToPage(int index)
        {
            if (pages.Count == 0)
            {
                titleText.text = "Como Funciona";
                bodyText.text = "Sem páginas cadastradas.";
                illustrationImage.sprite = null;
                illustrationImage.enabled = false;
                placeholderRoot.SetActive(true);
                pageCounterText.text = "0 / 0";
                previousButton.interactable = false;
                nextButton.interactable = false;
                return;
            }

            currentIndex = Mathf.Clamp(index, 0, pages.Count - 1);
            HowToPlayPageData page = pages[currentIndex];
            titleText.text = page.Title;
            bodyText.text = page.Body;
            bool hasSprite = page.Illustration != null;
            illustrationImage.enabled = hasSprite;
            illustrationImage.sprite = page.Illustration;
            placeholderRoot.SetActive(!hasSprite);
            pageCounterText.text = $"{currentIndex + 1} / {pages.Count}";
            previousButton.interactable = currentIndex > 0;
            nextButton.interactable = currentIndex < pages.Count - 1;
        }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = pivot;
            if (anchorMin == Vector2.zero && anchorMax == Vector2.one)
            {
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.sizeDelta = size;
                rect.anchoredPosition = Vector2.zero;
            }
            return rect;
        }

        private static Image CreateImage(string name, RectTransform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TextMeshProUGUI CreateTMP(string name, RectTransform parent, string text, float fontSize, TextAlignmentOptions align, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize; tmp.alignment = align; tmp.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            return tmp;
        }

        private static Button CreateButton(string name, RectTransform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 anchoredPos, float fontSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = pivot; rect.sizeDelta = size; rect.anchoredPosition = anchoredPos;
            go.GetComponent<Image>().color = new Color(0.18f, 0.36f, 0.68f, 1f);
            Button button = go.GetComponent<Button>();
            CreateTMP("Label", rect, label, fontSize, TextAlignmentOptions.Center, Vector2.zero, Vector2.one).color = Color.white;
            return button;
        }
    }
}
