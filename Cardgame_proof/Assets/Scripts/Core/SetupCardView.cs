using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace CardgameProof.Core
{
    public sealed class SetupCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private enum CardVisualState { Hand, Selected }
        private Canvas canvas;
        private RectTransform rect;
        private CanvasGroup canvasGroup;
        private Vector2 startAnchoredPosition;

        private Action<SetupCardView, PointerEventData> onDrop;

        public PlacedCardData CardData { get; private set; }

        public void Initialize(Canvas rootCanvas, PlacedCardData cardData, Action<SetupCardView, PointerEventData> dropCallback)
        {
            canvas = rootCanvas;
            CardData = cardData;
            onDrop = dropCallback;

            rect = GetComponent<RectTransform>();
            if (rect == null) rect = gameObject.AddComponent<RectTransform>();

            BuildCardVisual(CardData, CardVisualState.Hand, new Vector2(200f, 300f));

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            BuildLabels();
        }

        public void ResetToTray() => rect.anchoredPosition = startAnchoredPosition;

        public void OnBeginDrag(PointerEventData eventData)
        {
            startAnchoredPosition = rect.anchoredPosition;
            canvasGroup.blocksRaycasts = false;
            BuildCardVisual(CardData, CardVisualState.Selected, new Vector2(200f, 300f));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canvas == null) return;
            rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            BuildCardVisual(CardData, CardVisualState.Hand, new Vector2(200f, 300f));
            onDrop?.Invoke(this, eventData);
        }

        private void BuildLabels()
        {
            CreateLabel("Title", BuildCompactTitle(CardData.CardId), 0.76f, 30);
            CreateLabel("Type", CardData.CardType == CardType.Character ? "Personagem" : "Arquivo", 0.52f, 24);
            CreateLabel("Effect", CardData.CardType == CardType.Character ? "Pista de personagem" : "Efeito de arquivo", 0.26f, 20);
        }

        private void CreateLabel(string name, string value, float yNorm, int size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0.08f, yNorm - 0.2f);
            rt.anchorMax = new Vector2(0.92f, yNorm + 0.2f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            t.text = value;
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            t.enableWordWrapping = true;
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

            if (transform.Find("Title") == null)
            {
                BuildLabels();
            }
        }

        private static string BuildCompactTitle(string raw) => string.IsNullOrEmpty(raw) ? "Carta" : raw.Replace("_", " ");
    }
}
