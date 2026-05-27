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
            if (CardData.CardType == CardType.Character)
            {
                CharacterData character = FindCharacterFromCardId(CardData.CardId);
                CreateLabel("Title", character?.DisplayName ?? "Dado não cadastrado", 0.78f, 26);
                CreateLabel("Type", character?.Area ?? "Dado não cadastrado", 0.53f, 20);
                CreateLabel("Effect", character?.Era ?? "Dado não cadastrado", 0.30f, 18);
                if (character == null) Debug.LogWarning($"[Cards] Character placeholder fallback used for cardId={CardData.CardId}");
                return;
            }

            if (CardData.CardType == CardType.SemRegistro)
            {
                CreateLabel("Title", "Sem Registro", 0.78f, 26);
                CreateLabel("Type", "Nada encontrado nesta posição.", 0.53f, 20);
                CreateLabel("Effect", "Nenhum efeito.", 0.30f, 18);
                return;
            }

            ArchiveCardData archive = FindArchiveFromCardId(CardData.CardId);
            CreateLabel("Title", archive?.Title ?? "Dado não cadastrado", 0.78f, 26);
            CreateLabel("Type", "Arquivo", 0.53f, 20);
            CreateLabel("Effect", archive?.Description ?? "Dado não cadastrado", 0.30f, 18);
            if (archive == null) Debug.LogWarning($"[Cards] Archive placeholder fallback used for cardId={CardData.CardId}");
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
