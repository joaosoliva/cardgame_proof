using System;
using System.Collections.Generic;
using UnityEngine;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime
{
    public enum ScienceCardGamePhase
    {
        Setup,
        CardDistribution
    }

    public sealed class ScienceCardGameState
    {
        private readonly List<SciencePlayerState> players = new List<SciencePlayerState>();

        public string PrototypeTitle { get; } = "Protótipo: Jogo de Cartas Científico";
        public string Description { get; } = "Escolha quantos jogadores participarão desta simulação inicial.";
        public bool GameplayImplemented { get; } = false;
        public Vector2Int BoardSize { get; } = new Vector2Int(5, 3);
        public int InitialHandSize { get; } = 2;
        public DateTime InitializedAtUtc { get; private set; }
        public ScienceCardGamePhase CurrentPhase { get; private set; } = ScienceCardGamePhase.Setup;
        public int SelectedPlayerCount { get; private set; } = 2;
        public IReadOnlyList<SciencePlayerState> Players => players;

        public void InitializeDefaults()
        {
            InitializedAtUtc = DateTime.UtcNow;
            CurrentPhase = ScienceCardGamePhase.Setup;
            SelectedPlayerCount = 2;
            players.Clear();
        }

        public void InitializePlayers(int playerCount)
        {
            SelectedPlayerCount = Mathf.Clamp(playerCount, 2, 4);
            players.Clear();
            for (int i = 0; i < SelectedPlayerCount; i++)
            {
                players.Add(new SciencePlayerState(i, $"Player {i + 1}"));
            }
        }

        public void SetPhase(ScienceCardGamePhase phase)
        {
            CurrentPhase = phase;
        }
    }
}
