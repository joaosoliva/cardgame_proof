using System;
using System.Collections.Generic;
using UnityEngine;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime
{
    public sealed class ScienceCardGameState
    {
        private readonly List<SciencePlayerState> players = new List<SciencePlayerState>();

        public string PrototypeTitle { get; } = "Protótipo: Jogo de Cartas Científico";
        public string Description { get; } = "Módulo inicial reservado para a futura simulação tabletop de conexões entre personagens científicos. A jogabilidade ainda não foi implementada.";
        public bool GameplayImplemented { get; } = false;
        public Vector2Int BoardSize { get; } = new Vector2Int(5, 3);
        public DateTime InitializedAtUtc { get; private set; }
        public IReadOnlyList<SciencePlayerState> Players => players;

        public void InitializeDefaults()
        {
            InitializedAtUtc = DateTime.UtcNow;
            players.Clear();
            players.Add(new SciencePlayerState(0, "Pesquisador 1"));
            players.Add(new SciencePlayerState(1, "Pesquisador 2"));
        }
    }
}
