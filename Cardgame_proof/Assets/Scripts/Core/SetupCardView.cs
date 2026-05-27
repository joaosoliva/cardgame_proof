using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardgameProof.Core
{
    public sealed class SetupCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
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

            if (GetComponent<Image>() == null) gameObject.AddComponent<Image>().color = new Color(0.17f, 0.2f, 0.3f, 1f);
            if (GetComponent<LayoutElement>() == null) gameObject.AddComponent<LayoutElement>().preferredHeight = 148f;

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            BuildLabels();
        }

        public void ResetToTray() => rect.anchoredPosition = startAnchoredPosition;

        public void OnBeginDrag(PointerEventData eventData)
        {
            startAnchoredPosition = rect.anchoredPosition;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canvas == null) return;
            rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            onDrop?.Invoke(this, eventData);
        }

        private void BuildLabels()
        {
            CreateLabel("Title", CardData.CardId, 0.65f, 30);
            CreateLabel("Type", CardData.CardType.ToString(), 0.35f, 24);
            CreateLabel("Size", "1x1", 0.1f, 20);
        }

        private void CreateLabel(string name, string value, float yNorm, int size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0.08f, yNorm - 0.2f);
            rt.anchorMax = new Vector2(0.92f, yNorm + 0.2f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Text t = go.GetComponent<Text>();
            t.text = value;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleLeft;
            t.color = Color.white;
        }
    }
}
