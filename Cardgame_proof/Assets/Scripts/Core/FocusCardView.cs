using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.Core
{
    public sealed class FocusCardView : MonoBehaviour
    {
        private RectTransform root;
        private RectTransform panel;
        private TextMeshProUGUI cardTypeLabel;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI subtitleText;
        private TextMeshProUGUI bodyText;
        private TextMeshProUGUI infoFieldsText;
        private Button placeButton;
        private Button backButton;
        private Coroutine animationCoroutine;

        public bool IsVisible => gameObject != null && gameObject.activeSelf;

        public void Initialize(RectTransform overlayParent)
        {
            root = gameObject.GetComponent<RectTransform>();
            if (root == null) root = gameObject.AddComponent<RectTransform>();
            root.SetParent(overlayParent, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            CanvasGroup rootGroup = gameObject.GetComponent<CanvasGroup>();
            if (rootGroup == null) rootGroup = gameObject.AddComponent<CanvasGroup>();
            rootGroup.alpha = 1f;
            rootGroup.interactable = true;
            rootGroup.blocksRaycasts = true;

            GameObject dim = new GameObject("DimBackground", typeof(RectTransform), typeof(Image));
            RectTransform dimRect = dim.GetComponent<RectTransform>();
            dimRect.SetParent(root, false);
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.offsetMin = Vector2.zero;
            dimRect.offsetMax = Vector2.zero;
            Image dimImage = dim.GetComponent<Image>();
            dimImage.color = new Color(0f, 0f, 0f, 0.62f);
            dimImage.raycastTarget = true;

            GameObject panelObj = new GameObject("FocusCardPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
            panel = panelObj.GetComponent<RectTransform>();
            panel.SetParent(root, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(720f, 1020f);
            panel.localRotation = Quaternion.Euler(0f, 0f, -2f);
            panelObj.GetComponent<Image>().color = new Color(0.97f, 0.94f, 0.86f, 1f);
            Outline outline = panelObj.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
            outline.effectDistance = new Vector2(5f, -5f);

            cardTypeLabel = CreateText(panel, "CardTypeLabel", new Vector2(0.08f, 0.87f), new Vector2(0.92f, 0.95f), 32, FontStyles.Bold, TextAlignmentOptions.Center);
            titleText = CreateText(panel, "TitleText", new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.87f), 44, FontStyles.Bold, TextAlignmentOptions.Center);
            subtitleText = CreateText(panel, "SubtitleText", new Vector2(0.08f, 0.63f), new Vector2(0.92f, 0.70f), 26, FontStyles.Normal, TextAlignmentOptions.Center);
            infoFieldsText = CreateText(panel, "InfoFields", new Vector2(0.10f, 0.43f), new Vector2(0.90f, 0.62f), 27, FontStyles.Normal, TextAlignmentOptions.Left);
            bodyText = CreateText(panel, "BodyText", new Vector2(0.10f, 0.20f), new Vector2(0.90f, 0.42f), 28, FontStyles.Normal, TextAlignmentOptions.TopLeft);

            placeButton = CreateButton(panel, "PlaceButton", "Posicionar no Arquivo", new Vector2(0.08f, 0.09f), new Vector2(0.92f, 0.17f), new Color(0.15f, 0.47f, 0.24f, 1f));
            backButton = CreateButton(panel, "BackButton", "Voltar", new Vector2(0.08f, 0.015f), new Vector2(0.92f, 0.075f), new Color(0.28f, 0.31f, 0.36f, 1f));

            gameObject.SetActive(false);
        }

        public void Show(PlacedCardData cardData, Action onPlace, Action onBack)
        {
            if (cardData == null) return;
            transform.SetAsLastSibling();
            Populate(cardData);
            placeButton.onClick.RemoveAllListeners();
            placeButton.onClick.AddListener(() => onPlace?.Invoke());
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => onBack?.Invoke());
            gameObject.SetActive(true);

            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(PlayOpenAnimation());
        }

        public void Hide()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            gameObject.SetActive(false);
        }

        private void Populate(PlacedCardData cardData)
        {
            if (cardData.CardType == CardType.Character)
            {
                CharacterData character = FindCharacterFromCardId(cardData.CardId);
                cardTypeLabel.text = "Dossiê de Personagem";
                titleText.text = character?.DisplayName ?? "Dossiê";
                subtitleText.text = "Analise a carta antes de posicionar.";
                infoFieldsText.text = $"Área: {character?.Area ?? "Dado não cadastrado"}\nÉpoca: {character?.Era ?? "Dado não cadastrado"}\nRegião: {character?.Region ?? "Dado não cadastrado"}";
                bodyText.text = "Posicione este dossiê no seu arquivo. O outro jogador não verá o nome durante a investigação.";
                return;
            }

            ArchiveCardData archive = FindArchiveFromCardId(cardData.CardId);
            cardTypeLabel.text = "Carta de Arquivo";
            titleText.text = archive?.Title ?? "Carta de Arquivo";
            subtitleText.text = "Analise a carta antes de posicionar.";
            infoFieldsText.text = archive?.Prompt ?? "Efeito de investigação";
            bodyText.text = $"{archive?.Description ?? "Efeito não cadastrado."}\n\nEsta carta ativa um efeito quando for investigada pelo outro jogador.";
        }

        private IEnumerator PlayOpenAnimation()
        {
            float duration = 0.12f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = Mathf.Lerp(0.85f, 1f, 1f - Mathf.Pow(1f - t, 2f));
                panel.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            panel.localScale = Vector3.one;
            animationCoroutine = null;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = new Color(0.10f, 0.10f, 0.10f, 1f);
            text.enableWordWrapping = true;
            return text;
        }

        private static Button CreateButton(RectTransform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject buttonObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            buttonObj.GetComponent<Image>().color = color;

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = labelObj.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 30;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            return buttonObj.GetComponent<Button>();
        }

        private static CharacterData FindCharacterFromCardId(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return null;
            foreach (CharacterData character in PrototypeDatabase.Characters)
            {
                if (cardId.Contains(character.Id, StringComparison.OrdinalIgnoreCase)) return character;
            }
            return null;
        }

        private static ArchiveCardData FindArchiveFromCardId(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return null;
            foreach (ArchiveCardData archive in PrototypeDatabase.ArchiveCards)
            {
                if (cardId.Contains(archive.Id, StringComparison.OrdinalIgnoreCase)) return archive;
            }
            return null;
        }
    }
}
