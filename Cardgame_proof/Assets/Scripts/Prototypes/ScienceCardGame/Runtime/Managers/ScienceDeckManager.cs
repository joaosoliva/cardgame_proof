using System.Collections.Generic;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;
using UnityEngine;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public sealed class ScienceDeckManager
    {
        private readonly List<ScienceCardData> drawPile = new List<ScienceCardData>();
        private readonly List<ScienceCardData> discardPile = new List<ScienceCardData>();
        private ScienceTelemetryManager telemetry;

        public IReadOnlyList<ScienceCardData> DrawPile => drawPile;
        public IReadOnlyList<ScienceCardData> DiscardPile => discardPile;

        public void Initialize(ScienceCardGameState state, ScienceTelemetryManager telemetryManager)
        {
            telemetry = telemetryManager;
            BuildPrototypeDeck();
            Debug.Log($"[ScienceCardGame] 02 DeckManager initialized cards={drawPile.Count}");
            telemetry?.LogEvent("science_deck_initialized", $"cards={drawPile.Count}");
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
            telemetry = null;
        }

        private void BuildPrototypeDeck()
        {
            drawPile.Clear();
            discardPile.Clear();
            drawPile.Add(new ScienceCharacterCardData("character_curie", "Marie Curie", "Química e Física", "Século XX", "Conecte descobertas sobre radioatividade a impactos científicos e sociais."));
            drawPile.Add(new ScienceCharacterCardData("character_darwin", "Charles Darwin", "Biologia", "Século XIX", "Relacione evidências, observação e teoria científica."));
            drawPile.Add(new ScienceActionCardData("action_research", "Pesquisa Rápida", "Compre uma carta e explique uma possível conexão.", ScienceActionKind.Research));
            drawPile.Add(new ScienceActionCardData("action_connect", "Conexão Cruzada", "Crie uma ligação entre dois personagens ou áreas.", ScienceActionKind.Connect));
        }
    }
}
