using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace CardgameProof.Core
{
    public sealed class SetupCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private enum CardVisualState { Hand, Selected }
        private Canvas canvas;
        private RectTransform rect;
        private CanvasGroup canvasGroup;
        private Vector2 startAnchoredPosition;

        private Action<SetupCardView, PointerEventData> onDrop;
        private Action<SetupCardView> onTap;
        private bool dragEnabled;

        public PlacedCardData CardData { get; private set; }

        public void Initialize(Canvas rootCanvas, PlacedCardData cardData, Action<SetupCardView, PointerEventData> dropCallback, Action<SetupCardView> tapCallback = null, bool allowDrag = false)
        {
            canvas = rootCanvas;
            CardData = cardData;
            onDrop = dropCallback;
            onTap = tapCallback;
            dragEnabled = allowDrag;

            rect = GetComponent<RectTransform>();
            if (rect == null) rect = gameObject.AddComponent<RectTransform>();

            BuildCardVisual(CardData, CardVisualState.Hand, new Vector2(170f, 250f));

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            BuildLabels();
        }

        public void SetInteractionEnabled(bool enabled)
        {
            if (canvasGroup == null) canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = enabled;
            canvasGroup.blocksRaycasts = enabled;
            canvasGroup.alpha = enabled ? 1f : 0.45f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.dragging) return;
            onTap?.Invoke(this);
        }

        public void ResetToTray() => rect.anchoredPosition = startAnchoredPosition;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!dragEnabled) return;
            startAnchoredPosition = rect.anchoredPosition;
            canvasGroup.blocksRaycasts = false;
            BuildCardVisual(CardData, CardVisualState.Selected, new Vector2(170f, 250f));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragEnabled || canvas == null) return;
            rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragEnabled) return;
            canvasGroup.blocksRaycasts = true;
            BuildCardVisual(CardData, CardVisualState.Hand, new Vector2(170f, 250f));
            onDrop?.Invoke(this, eventData);
        }

        private void BuildLabels()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            if (CardData.CardType == CardType.SemRegistro)
            {
                return;
            }

            string typeLabel;
            string title;
            Color accentColor;
            if (CardData.CardType == CardType.Character)
            {
                CharacterData character = FindCharacterFromCardId(CardData.CardId);
                typeLabel = "Dossiê";
                title = character?.DisplayName ?? "Dossiê";
                accentColor = new Color(0.58f, 0.33f, 0.08f, 1f);
                if (character == null) Debug.LogWarning($"[Cards] Character placeholder fallback used for cardId={CardData.CardId}");
            }
            else
            {
                ArchiveCardData archive = FindArchiveFromCardId(CardData.CardId);
                typeLabel = "Arquivo";
                title = archive?.Title ?? "Carta de Arquivo";
                accentColor = new Color(0.08f, 0.26f, 0.55f, 1f);
                if (archive == null) Debug.LogWarning($"[Cards] Archive placeholder fallback used for cardId={CardData.CardId}");
            }

            CreateLabel("Type", typeLabel, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.92f), 28, accentColor, TextAlignmentOptions.Center);
            CreateLabel("Title", Shorten(title, 34), new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.70f), 22, new Color(0.10f, 0.10f, 0.10f, 1f), TextAlignmentOptions.Center);
            CreateLabel("Instruction", "Toque para analisar", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.28f), 17, new Color(0.18f, 0.18f, 0.18f, 0.95f), TextAlignmentOptions.Center);
        }

        private void CreateLabel(string name, string value, Vector2 anchorMin, Vector2 anchorMax, int size, Color color, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            t.text = value;
            t.fontSize = size;
            t.alignment = alignment;
            t.color = color;
            t.enableWordWrapping = true;
        }

        private static string Shorten(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength) return value;
            return value.Substring(0, Mathf.Max(0, maxLength - 1)) + "…";
        }

        private void BuildCardVisual(PlacedCardData data, CardVisualState visualState, Vector2 targetSize)
        {
            Image bg = GetComponent<Image>();
            if (bg == null) bg = gameObject.AddComponent<Image>();
            bg.color = data.CardType == CardType.Character
                ? new Color(0.96f, 0.82f, 0.54f, 1f)
                : new Color(0.66f, 0.78f, 0.91f, 1f);
            rect.sizeDelta = targetSize;

            LayoutElement layout = GetComponent<LayoutElement>();
            if (layout == null) layout = gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = targetSize.x;
            layout.preferredHeight = targetSize.y;

            Outline outline = GetComponent<Outline>();
            if (outline == null) outline = gameObject.AddComponent<Outline>();
            outline.effectColor = visualState == CardVisualState.Selected
                ? new Color(0.98f, 0.83f, 0.29f, 1f)
                : new Color(0f, 0f, 0f, 0.22f);
            outline.effectDistance = new Vector2(2f, -2f);


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
