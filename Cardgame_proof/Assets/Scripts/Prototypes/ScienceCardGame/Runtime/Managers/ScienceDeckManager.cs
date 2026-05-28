using System;
using System.Collections.Generic;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;
using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public sealed class ScienceDeckManager
    {
        private readonly List<ScienceCardData> drawPile = new List<ScienceCardData>();
        private readonly List<ScienceCardData> discardPile = new List<ScienceCardData>();
        private readonly List<ScienceCharacterCardData> characterCards = new List<ScienceCharacterCardData>();
        private readonly List<ScienceActionCardData> actionCards = new List<ScienceActionCardData>();
        private ScienceTelemetryManager telemetry;
        private System.Random random;

        public IReadOnlyList<ScienceCardData> DrawPile => drawPile;
        public IReadOnlyList<ScienceCardData> DiscardPile => discardPile;
        public IReadOnlyList<ScienceCharacterCardData> CharacterCards => characterCards;
        public IReadOnlyList<ScienceActionCardData> ActionCards => actionCards;

        public void Initialize(IReadOnlyList<ScienceCardData> sourceCards, ScienceTelemetryManager telemetryManager, int? seed = null)
        {
            telemetry = telemetryManager;
            random = seed.HasValue ? new System.Random(seed.Value) : new System.Random(Environment.TickCount);
            LoadDeck(sourceCards);
            Debug.Log($"[ScienceCardGame] 02 DeckManager initialized cards={drawPile.Count} characters={characterCards.Count} actions={actionCards.Count}");
            telemetry?.LogEvent("science_deck_initialized", $"cards={drawPile.Count};characters={characterCards.Count};actions={actionCards.Count}");
        }

        public ScienceCardData DrawCard()
        {
            if (drawPile.Count == 0) return null;
            ScienceCardData card = drawPile[0];
            drawPile.RemoveAt(0);
            telemetry?.LogEvent("science_card_drawn", card.Id);
            return card;
        }

        public void Discard(ScienceCardData card)
        {
            if (card == null) return;
            discardPile.Add(card);
            telemetry?.LogEvent("science_card_discarded", card.Id);
        }

        public void Cleanup()
        {
            drawPile.Clear();
            discardPile.Clear();
            characterCards.Clear();
            actionCards.Clear();
            telemetry = null;
            random = null;
        }

        private void LoadDeck(IReadOnlyList<ScienceCardData> sourceCards)
        {
            drawPile.Clear();
            discardPile.Clear();
            characterCards.Clear();
            actionCards.Clear();

            if (sourceCards != null)
            {
                foreach (ScienceCardData card in sourceCards)
                {
                    if (card == null) continue;
                    drawPile.Add(card);
                    if (card is ScienceCharacterCardData characterCard) characterCards.Add(characterCard);
                    if (card is ScienceActionCardData actionCard) actionCards.Add(actionCard);
                }
            }

            Shuffle(drawPile);
            Shuffle(characterCards);
            Shuffle(actionCards);
        }

        private void Shuffle<T>(IList<T> cards)
        {
            if (cards == null || cards.Count <= 1) return;
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }
    }
}
