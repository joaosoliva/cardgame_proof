using System;
using System.Collections.Generic;
using UnityEngine;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime
{
    public enum ScienceRejectedConnectionBehavior
    {
        ReturnCardToHand,
        RetryExplanation
    }

    public enum ScienceCardGamePhase
    {
        Setup,
        CardDistribution,
        GameOver
    }

    public enum ScienceWinCondition
    {
        None,
        TargetKnowledgePoints,
        EmptyHand
    }

    public sealed class ScienceCardGameState
    {
        private readonly List<SciencePlayerState> players = new List<SciencePlayerState>();

        public string PrototypeTitle { get; } = "Protótipo: Jogo de Cartas Científico";
        public string Description { get; } = "Escolha quantos jogadores participarão desta simulação inicial.";
        public bool GameplayImplemented { get; } = false;
        public Vector2Int BoardSize { get; } = new Vector2Int(7, 7);
        public int InitialHandSize { get; private set; } = 4;
        public int TargetKnowledgePoints { get; private set; } = 7;
        public bool DebugRevealAllHands { get; private set; }
        public bool AcceptTiedConnectionVotes { get; private set; } = true;
        public ScienceRejectedConnectionBehavior RejectedConnectionBehavior { get; private set; } = ScienceRejectedConnectionBehavior.ReturnCardToHand;
        public DateTime InitializedAtUtc { get; private set; }
        public ScienceCardGamePhase CurrentPhase { get; private set; } = ScienceCardGamePhase.Setup;
        public int SelectedPlayerCount { get; private set; } = 2;
        public int AcceptedConnections { get; private set; }
        public int RejectedConnections { get; private set; }
        public IReadOnlyList<SciencePlayerState> Players => players;

        public void InitializeDefaults()
        {
            InitializedAtUtc = DateTime.UtcNow;
            CurrentPhase = ScienceCardGamePhase.Setup;
            SelectedPlayerCount = 2;
            InitialHandSize = GetInitialHandSizeForPlayerCount(SelectedPlayerCount);
            AcceptedConnections = 0;
            RejectedConnections = 0;
            players.Clear();
        }

        public void InitializePlayers(int playerCount)
        {
            SelectedPlayerCount = Mathf.Clamp(playerCount, 2, 4);
            InitialHandSize = GetInitialHandSizeForPlayerCount(SelectedPlayerCount);
            AcceptedConnections = 0;
            RejectedConnections = 0;
            players.Clear();
            for (int i = 0; i < SelectedPlayerCount; i++)
            {
                players.Add(new SciencePlayerState(i, $"Player {i + 1}"));
            }
        }

        public void SetDebugRevealAllHands(bool enabled)
        {
            DebugRevealAllHands = enabled;
        }

        public void SetTargetKnowledgePoints(int targetKnowledgePoints)
        {
            TargetKnowledgePoints = Mathf.Max(1, targetKnowledgePoints);
        }

        public void SetAcceptTiedConnectionVotes(bool acceptTies)
        {
            AcceptTiedConnectionVotes = acceptTies;
        }

        public void SetRejectedConnectionBehavior(ScienceRejectedConnectionBehavior behavior)
        {
            RejectedConnectionBehavior = behavior;
        }

        public void SetPhase(ScienceCardGamePhase phase)
        {
            CurrentPhase = phase;
        }

        public void RecordAcceptedConnection()
        {
            AcceptedConnections += 1;
        }

        public void RecordRejectedConnection()
        {
            RejectedConnections += 1;
        }

        public static int GetInitialHandSizeForPlayerCount(int playerCount)
        {
            return Mathf.Clamp(playerCount, 2, 4) == 2 ? 4 : 3;
        }
    }
}
