using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardgameProof.App;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.Data;
using CardgameProof.Prototypes.ScienceCardGame.Runtime.UI;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public sealed class ScienceCardGameUIManager
    {
        private const int MaxLogEntries = 7;

        private readonly List<string> recentActions = new List<string>();
        private readonly Dictionary<int, bool> connectionVotes = new Dictionary<int, bool>();
        private PrototypeRuntimeContext context;
        private ScienceCardGameState state;
        private ScienceDeckManager deckManager;
        private ScienceBoardManager boardManager;
        private ScienceScoreManager scoreManager;
        private ScienceTurnManager turnManager;
        private ScienceTelemetryManager telemetry;
        private GameObject root;
        private GameObject cardDetailModal;
        private TextMeshProUGUI selectedPlayerCountText;
        private int selectedPlayerCount = 2;
        private int activeVotingPlayerIndex = -1;
        private string activeVotingCardId;
        private int activeScoringPlayerIndex = -1;
        private string activeScoringCardId;
        private bool basePointAwarded;
        private bool interestingBonusAwarded;
        private bool guideFactBonusAwarded;
        private Action onRestartPrototype;

        public GameObject Root => root;

        public void Initialize(
            PrototypeRuntimeContext runtimeContext,
            ScienceCardGameState gameState,
            ScienceDeckManager scienceDeckManager,
            ScienceBoardManager scienceBoardManager,
            ScienceScoreManager scienceScoreManager,
            ScienceTurnManager scienceTurnManager,
            ScienceTelemetryManager telemetryManager,
            Action<int> onStartGame,
            Action restartPrototype)
        {
            context = runtimeContext;
            state = gameState;
            deckManager = scienceDeckManager;
            boardManager = scienceBoardManager;
            scoreManager = scienceScoreManager;
            turnManager = scienceTurnManager;
            telemetry = telemetryManager;
            onRestartPrototype = restartPrototype;
            selectedPlayerCount = state?.SelectedPlayerCount ?? 2;
            recentActions.Clear();
            ResetConnectionVoting();
            ResetScoringState();

            if (context?.SceneRoot?.FullScreenRoot == null)
            {
                Debug.LogWarning("[ScienceCardGame] UIManager initialization failed: missing scene root.");
                return;
            }

            BuildSetupScreen(context.SceneRoot.FullScreenRoot, onStartGame);
            Debug.Log("[ScienceCardGame] 06 UIManager initialized setup screen");
            telemetry?.LogEvent("science_ui_initialized", "screen=setup");
        }

        public void ShowCardDistributionScreen()
        {
            if (root == null || state == null) return;

            AddLog($"Distribuição concluída: {state.Players.Count} jogadores com {state.InitialHandSize} cartas cada.");
            AddLog($"Turno {turnManager?.TurnNumber ?? 0}: {GetCurrentPlayerName()} começa.");
            BuildGameplayScreen();
            telemetry?.LogEvent("science_ui_screen_changed", "screen=gameplay_layout");
        }

        public void Cleanup()
        {
            CloseCardDetailsModal();
            if (root != null)
            {
                root.SetActive(false);
                UnityEngine.Object.Destroy(root);
                root = null;
            }

            recentActions.Clear();
            ResetConnectionVoting();
            ResetScoringState();
            selectedPlayerCountText = null;
            onRestartPrototype = null;
            context = null;
            state = null;
            deckManager = null;
            boardManager = null;
            scoreManager = null;
            turnManager = null;
            telemetry = null;
        }

        private void BuildSetupScreen(RectTransform parent, Action<int> onStartGame)
        {
            root = new GameObject("ScienceCardGameRoot", typeof(RectTransform), typeof(Image));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.07f, 0.09f, 0.13f, 1f);

            CreateText(rect, state.PrototypeTitle, 54, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.90f), FontStyles.Bold);
            CreateText(rect, state.Description, 30, new Vector2(0.10f, 0.64f), new Vector2(0.90f, 0.76f), FontStyles.Normal);
            selectedPlayerCountText = CreateText(rect, string.Empty, 28, new Vector2(0.12f, 0.56f), new Vector2(0.88f, 0.63f), FontStyles.Bold);
            UpdateSelectedPlayerCountText();

            CreateButton(rect, "2 jogadores", new Vector2(0.25f, 0.47f), () => SelectPlayerCount(2), new Vector2(250f, 92f));
            CreateButton(rect, "3 jogadores", new Vector2(0.50f, 0.47f), () => SelectPlayerCount(3), new Vector2(250f, 92f));
            CreateButton(rect, "4 jogadores", new Vector2(0.75f, 0.47f), () => SelectPlayerCount(4), new Vector2(250f, 92f));

            CreateButton(rect, "Start Game", new Vector2(0.5f, 0.32f), () => onStartGame?.Invoke(selectedPlayerCount));
            CreateButton(rect, "Back to Prototype Selection", new Vector2(0.5f, 0.20f), () => context?.ReturnToSelector?.Invoke());
        }

        private void BuildGameplayScreen()
        {
            CloseCardDetailsModal();
            RectTransform screen = root.GetComponent<RectTransform>();
            ClearChildren(root.transform);

            if (state != null && state.CurrentPhase == ScienceCardGamePhase.GameOver)
            {
                BuildEndGameScreen(screen, ResolveWinners());
                return;
            }

            RectTransform topBar = CreatePanel(screen, "TopBar", new Vector2(0.02f, 0.88f), new Vector2(0.98f, 0.98f), new Color(0.10f, 0.13f, 0.18f, 0.96f));
            RectTransform logPanel = CreatePanel(screen, "LogPanel", new Vector2(0.02f, 0.02f), new Vector2(0.20f, 0.86f), new Color(0.08f, 0.10f, 0.14f, 0.96f));
            RectTransform boardPanel = CreatePanel(screen, "BoardPanel", new Vector2(0.22f, 0.28f), new Vector2(0.78f, 0.86f), new Color(0.12f, 0.20f, 0.17f, 0.96f));
            RectTransform handPanel = CreatePanel(screen, "CurrentPlayerHandPanel", new Vector2(0.22f, 0.02f), new Vector2(0.78f, 0.25f), new Color(0.11f, 0.12f, 0.18f, 0.96f));
            RectTransform actionPanel = CreatePanel(screen, "TurnActionPanel", new Vector2(0.80f, 0.02f), new Vector2(0.98f, 0.86f), new Color(0.09f, 0.11f, 0.16f, 0.96f));

            BuildTopBar(topBar);
            BuildLogPanel(logPanel);
            BuildBoardPanel(boardPanel);
            BuildHandPanel(handPanel);
            BuildActionPanel(actionPanel);
        }

        private void BuildTopBar(RectTransform parent)
        {
            CreateText(parent, "Protótipo: Jogo de Cartas Científico", 28, new Vector2(0.02f, 0.10f), new Vector2(0.32f, 0.90f), FontStyles.Bold, TextAlignmentOptions.Left);
            CreateText(parent, $"Jogador atual: {GetCurrentPlayerName()}  |  Turno {turnManager?.TurnNumber ?? 0}", 25, new Vector2(0.33f, 0.34f), new Vector2(0.58f, 0.90f), FontStyles.Bold);
            CreateText(parent, BuildTurnInstruction(), 18, new Vector2(0.33f, 0.08f), new Vector2(0.58f, 0.34f), FontStyles.Italic);
            CreateText(parent, BuildScoreLine(), 22, new Vector2(0.59f, 0.10f), new Vector2(0.82f, 0.90f), FontStyles.Normal, TextAlignmentOptions.Left);
            CreateButton(parent, "Back to Prototype Selection", new Vector2(0.91f, 0.50f), () => context?.ReturnToSelector?.Invoke(), new Vector2(280f, 62f));
        }

        private void BuildLogPanel(RectTransform parent)
        {
            CreateText(parent, "Log", 26, new Vector2(0.08f, 0.90f), new Vector2(0.92f, 0.98f), FontStyles.Bold);
            CreateText(parent, BuildRecentActionLog(), 20, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.88f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        }

        private void BuildBoardPanel(RectTransform parent)
        {
            CreateText(parent, "Área Central do Tabuleiro", 28, new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.99f), FontStyles.Bold);
            CreateText(parent, "Primeira personagem: perto do centro. Depois: adjacente a outra personagem.", 17, new Vector2(0.04f, 0.85f), new Vector2(0.96f, 0.91f), FontStyles.Italic);

            RectTransform grid = CreatePanel(parent, "BoardGrid", new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.83f), new Color(0.06f, 0.12f, 0.10f, 0.92f));
            int columns = Mathf.Max(1, state?.BoardSize.x ?? 7);
            int rows = Mathf.Max(1, state?.BoardSize.y ?? 7);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    float minX = x / (float)columns;
                    float maxX = (x + 1) / (float)columns;
                    float minY = 1f - ((y + 1) / (float)rows);
                    float maxY = 1f - (y / (float)rows);
                    Vector2Int coordinate = new Vector2Int(x, y);
                    bool isSelectedSlot = turnManager != null && turnManager.HasSelectedBoardCoordinate && turnManager.SelectedBoardCoordinate == coordinate;
                    ScienceCardData selectedCard = turnManager?.SelectedCard;
                    Color slotColor = GetBoardSlotColor(coordinate, selectedCard, isSelectedSlot);
                    RectTransform slot = CreatePanel(grid, $"BoardSlot_{x}_{y}", new Vector2(minX, minY), new Vector2(maxX, maxY), slotColor);
                    slot.offsetMin = new Vector2(slot.offsetMin.x + 3f, slot.offsetMin.y + 3f);
                    slot.offsetMax = new Vector2(slot.offsetMax.x - 3f, slot.offsetMax.y - 3f);
                    ScienceCardData boardCard = GetBoardCardAt(coordinate);
                    if (boardCard != null)
                    {
                        ScienceCardView boardCardView = ScienceCardView.Create(slot, $"BoardCard_{x}_{y}", boardCard, ScienceCardViewDisplayMode.Board, OpenCardDetailsModal);
                        RectTransform boardCardRect = boardCardView.GetComponent<RectTransform>();
                        boardCardRect.anchorMin = new Vector2(0.5f, 0.5f);
                        boardCardRect.anchorMax = new Vector2(0.5f, 0.5f);
                        boardCardRect.anchoredPosition = Vector2.zero;
                        ApplyBoardCardRotation(boardCardRect, boardManager?.GetPlacedCardRotationDegrees(coordinate) ?? 0);
                    }
                    else
                    {
                        if (turnManager != null && turnManager.CurrentStep == ScienceTurnStep.AwaitingBoardSlot)
                        {
                            ConfigureBoardSlotButton(slot, coordinate);
                        }

                        if (isSelectedSlot && selectedCard != null)
                        {
                            ScienceCardView previewCardView = ScienceCardView.Create(slot, $"BoardPreview_{x}_{y}", selectedCard, ScienceCardViewDisplayMode.Board);
                            RectTransform previewRect = previewCardView.GetComponent<RectTransform>();
                            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
                            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
                            previewRect.anchoredPosition = Vector2.zero;
                            ApplyBoardCardRotation(previewRect, turnManager?.SelectedRotationDegrees ?? 0);
                        }
                        else
                        {
                            string label = FormatBoardCoordinate(coordinate);
                            CreateText(slot, label, 12, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f), FontStyles.Normal);
                        }
                    }
                }
            }
        }

        private static void ApplyBoardCardRotation(RectTransform cardRect, int rotationDegrees)
        {
            if (cardRect == null) return;
            cardRect.localEulerAngles = new Vector3(0f, 0f, -ScienceBoardSlotState.NormalizeRotation(rotationDegrees));
        }

        private Color GetBoardSlotColor(Vector2Int coordinate, ScienceCardData selectedCard, bool isSelectedSlot)
        {
            if (isSelectedSlot) return new Color(0.82f, 0.68f, 0.24f, 0.95f);
            if (GetBoardCardAt(coordinate) != null) return new Color(0.18f, 0.24f, 0.30f, 0.88f);

            if (turnManager != null && turnManager.CurrentStep == ScienceTurnStep.AwaitingBoardSlot && selectedCard != null)
            {
                bool isValid = boardManager != null && boardManager.CanPlaceCardAt(coordinate, selectedCard);
                return isValid ? new Color(0.18f, 0.46f, 0.25f, 0.90f) : new Color(0.40f, 0.18f, 0.18f, 0.70f);
            }

            return new Color(0.15f, 0.28f, 0.23f, 0.82f);
        }

        private static string FormatBoardCoordinate(Vector2Int coordinate)
        {
            return $"{coordinate.x + 1},{coordinate.y + 1}";
        }

        private void BuildHandPanel(RectTransform parent)
        {
            SciencePlayerState currentPlayer = GetCurrentPlayer();
            string title = currentPlayer == null ? "Mão do jogador" : $"Mão de {currentPlayer.DisplayName}";
            CreateText(parent, title, 25, new Vector2(0.03f, 0.78f), new Vector2(0.34f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Left);

            if (currentPlayer == null || currentPlayer.Hand.Count == 0)
            {
                CreateText(parent, "Nenhuma carta na mão atual.", 22, new Vector2(0.04f, 0.20f), new Vector2(0.96f, 0.70f), FontStyles.Italic);
                return;
            }

            int characterCount = CountCardsOfType(currentPlayer, ScienceCardType.Character);
            int actionCount = CountCardsOfType(currentPlayer, ScienceCardType.Action);
            CreateText(parent, $"{currentPlayer.Hand.Count} cartas: {characterCount} personagens, {actionCount} ações", 19, new Vector2(0.36f, 0.80f), new Vector2(0.96f, 0.95f), FontStyles.Normal, TextAlignmentOptions.Right);

            RectTransform cardRow = CreatePanel(parent, "CurrentPlayerHandCards", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.76f), new Color(0.06f, 0.07f, 0.11f, 0.55f));
            int cardCount = currentPlayer.Hand.Count;
            for (int i = 0; i < cardCount; i++)
            {
                ScienceCardData card = currentPlayer.Hand[i];
                float center = (i + 0.5f) / cardCount;
                RectTransform cardRect = CreateCardView(cardRow, card, $"HandCard_{i}");
                cardRect.anchorMin = new Vector2(center, 0.50f);
                cardRect.anchorMax = new Vector2(center, 0.50f);
                cardRect.pivot = new Vector2(0.5f, 0.5f);
                cardRect.sizeDelta = new Vector2(150f, 142f);
                cardRect.anchoredPosition = Vector2.zero;
            }
        }

        private void BuildActionPanel(RectTransform parent)
        {
            CreateText(parent, "Ações do Turno", 27, new Vector2(0.08f, 0.91f), new Vector2(0.92f, 0.98f), FontStyles.Bold);
            CreateText(parent, $"Deck: {deckManager?.DrawPile?.Count ?? 0}\nDescarte: {deckManager?.DiscardPile?.Count ?? 0}", 21, new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.89f), FontStyles.Normal);
            CreateText(parent, BuildSelectedCardText(), 18, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.78f), FontStyles.Normal, TextAlignmentOptions.Top);

            ScienceTurnStep step = turnManager?.CurrentStep ?? ScienceTurnStep.AwaitingCardSelection;
            if (step == ScienceTurnStep.AwaitingBoardSlot || step == ScienceTurnStep.AwaitingPlacementConfirmation)
            {
                CreateButton(parent, "Girar esquerda", new Vector2(0.50f, 0.58f), RotateSelectedCardLeft, new Vector2(260f, 58f));
                CreateButton(parent, "Girar direita", new Vector2(0.50f, 0.49f), RotateSelectedCardRight, new Vector2(260f, 58f));
            }

            if (step == ScienceTurnStep.AwaitingPlacementConfirmation)
            {
                CreateButton(parent, "Confirmar posição", new Vector2(0.50f, 0.38f), ConfirmPlacement, new Vector2(260f, 68f));
            }

            if (step == ScienceTurnStep.ConnectionExplanation)
            {
                BuildConnectionVotingPanel(parent);
            }

            if (step == ScienceTurnStep.Scoring)
            {
                BuildScoringPanel(parent);
            }

            if (step == ScienceTurnStep.AwaitingBoardSlot || step == ScienceTurnStep.AwaitingPlacementConfirmation)
            {
                CreateButton(parent, "Cancelar seleção", new Vector2(0.50f, 0.28f), CancelSelection, new Vector2(260f, 58f));
            }

            if (step == ScienceTurnStep.TurnResolved)
            {
                CreateButton(parent, "Encerrar turno", new Vector2(0.50f, 0.42f), EndTurn, new Vector2(260f, 78f));
            }

            CreateText(parent, BuildTurnHelpText(), 18, new Vector2(0.10f, 0.06f), new Vector2(0.90f, 0.30f), FontStyles.Italic, TextAlignmentOptions.Top);
        }

        private RectTransform CreateCardView(RectTransform parent, ScienceCardData card, string name)
        {
            Action<ScienceCardData> onClick = turnManager != null && turnManager.CurrentStep == ScienceTurnStep.AwaitingCardSelection
                ? SelectCardForTurn
                : null;
            ScienceCardView view = ScienceCardView.Create(parent, name, card, ScienceCardViewDisplayMode.Hand, onClick);
            return view.GetComponent<RectTransform>();
        }

        private void ConfigureBoardSlotButton(RectTransform slot, Vector2Int coordinate)
        {
            Button button = slot.gameObject.AddComponent<Button>();
            button.targetGraphic = slot.GetComponent<Image>();
            button.onClick.AddListener(() => SelectBoardSlot(coordinate));
        }

        private void SelectCardForTurn(ScienceCardData card)
        {
            SciencePlayerState currentPlayer = GetCurrentPlayer();
            if (card == null || currentPlayer == null || !ContainsCard(currentPlayer.Hand, card))
            {
                AddLog("Apenas cartas da mão do jogador atual podem ser selecionadas.");
                BuildGameplayScreen();
                return;
            }

            if (turnManager == null || !turnManager.SelectCard(card))
            {
                OpenCardDetailsModal(card);
                return;
            }

            if (card is ScienceActionCardData actionCard)
            {
                ResolveActionCard(currentPlayer, actionCard);
                return;
            }

            AddLog($"{currentPlayer.DisplayName} selecionou {card.DisplayName}. Escolha um espaço livre no tabuleiro.");
            BuildGameplayScreen();
            OpenCardDetailsModal(card);
        }

        private void RotateSelectedCardLeft()
        {
            RotateSelectedCard(-90);
        }

        private void RotateSelectedCardRight()
        {
            RotateSelectedCard(90);
        }

        private void RotateSelectedCard(int deltaDegrees)
        {
            if (turnManager == null || turnManager.SelectedCard == null)
            {
                AddLog("Selecione uma carta antes de girar.");
                BuildGameplayScreen();
                return;
            }

            turnManager.RotateSelectedCard(deltaDegrees);
            AddLog($"Rotação da carta selecionada: {turnManager.SelectedRotationDegrees}°.");
            BuildGameplayScreen();
        }

        private void SelectBoardSlot(Vector2Int coordinate)
        {
            if (turnManager == null)
            {
                AddLog("Selecione uma carta de personagem antes de escolher o tabuleiro.");
                BuildGameplayScreen();
                return;
            }

            string validationMessage = boardManager?.GetPlacementValidationMessage(coordinate, turnManager.SelectedCard);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                AddLog($"Posição {FormatBoardCoordinate(coordinate)} inválida: {validationMessage}");
                BuildGameplayScreen();
                return;
            }

            if (!turnManager.SelectBoardSlot(coordinate))
            {
                AddLog("Selecione uma carta de personagem antes de escolher o tabuleiro.");
                BuildGameplayScreen();
                return;
            }

            AddLog($"Posição {FormatBoardCoordinate(coordinate)} selecionada. Confirme para colocar a carta.");
            BuildGameplayScreen();
        }

        private void ConfirmPlacement()
        {
            if (turnManager == null || turnManager.CurrentStep != ScienceTurnStep.AwaitingPlacementConfirmation || !turnManager.HasSelectedBoardCoordinate)
            {
                AddLog("Nenhuma posição aguardando confirmação.");
                BuildGameplayScreen();
                return;
            }

            SciencePlayerState currentPlayer = GetCurrentPlayer();
            ScienceCardData selectedCard = turnManager.SelectedCard;
            if (!(selectedCard is ScienceCharacterCardData) || currentPlayer == null)
            {
                AddLog("Somente cartas de personagem podem ser colocadas no tabuleiro nesta etapa.");
                turnManager.CancelSelection();
                BuildGameplayScreen();
                return;
            }

            Vector2Int coordinate = turnManager.SelectedBoardCoordinate;
            string validationMessage = boardManager?.GetPlacementValidationMessage(coordinate, selectedCard);
            if (!string.IsNullOrEmpty(validationMessage) || boardManager == null || !boardManager.TryPlaceCard(coordinate, selectedCard, turnManager.SelectedRotationDegrees))
            {
                AddLog($"Não foi possível colocar a carta em {FormatBoardCoordinate(coordinate)}: {validationMessage ?? "posição inválida"}");
                turnManager.CancelSelection();
                BuildGameplayScreen();
                return;
            }

            currentPlayer.MarkPlayed(selectedCard);
            turnManager.StartConnectionExplanation();
            StartConnectionVoting(currentPlayer, selectedCard);
            AddLog($"{currentPlayer.DisplayName} colocou {selectedCard.DisplayName} em {FormatBoardCoordinate(coordinate)} com rotação {turnManager.SelectedRotationDegrees}°.");
            AddLog("Explique a conexão científica proposta; os demais jogadores votam para aceitar a conexão.");
            BuildGameplayScreen();
        }

        private void StartConnectionVoting(SciencePlayerState activePlayer, ScienceCardData placedCard)
        {
            connectionVotes.Clear();
            activeVotingPlayerIndex = activePlayer?.PlayerIndex ?? -1;
            activeVotingCardId = placedCard?.Id;
            telemetry?.LogEvent("science_connection_vote_started", $"turn={turnManager?.TurnNumber ?? 0};activePlayer={activeVotingPlayerIndex};card={activeVotingCardId};acceptTies={state?.AcceptTiedConnectionVotes ?? true}");
        }

        private void BuildConnectionVotingPanel(RectTransform parent)
        {
            SciencePlayerState activePlayer = GetCurrentPlayer();
            ScienceCardData selectedCard = turnManager?.SelectedCard;
            string cardName = selectedCard?.DisplayName ?? "carta colocada";

            CreateText(parent, "Votação da conexão", 23, new Vector2(0.08f, 0.60f), new Vector2(0.92f, 0.66f), FontStyles.Bold);
            CreateText(parent, $"{activePlayer?.DisplayName ?? "Jogador ativo"} explicou {cardName}.\nOs jogadores aceitam essa conexão?", 17, new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.60f), FontStyles.Normal);

            if (state == null || state.Players.Count <= 1)
            {
                CreateButton(parent, "Aceitar sem votação", new Vector2(0.50f, 0.36f), ResolveConnectionVoteResult, new Vector2(260f, 58f));
                return;
            }

            int row = 0;
            for (int i = 0; i < state.Players.Count; i++)
            {
                SciencePlayerState player = state.Players[i];
                if (player == null || player.PlayerIndex == activeVotingPlayerIndex) continue;

                float y = 0.38f - (row * 0.105f);
                if (connectionVotes.TryGetValue(player.PlayerIndex, out bool accepted))
                {
                    CreateText(parent, $"{player.DisplayName}: {(accepted ? "Sim" : "Não")}", 17, new Vector2(0.10f, y - 0.035f), new Vector2(0.90f, y + 0.035f), FontStyles.Bold);
                }
                else
                {
                    int voterIndex = player.PlayerIndex;
                    CreateText(parent, player.DisplayName, 16, new Vector2(0.08f, y - 0.035f), new Vector2(0.36f, y + 0.035f), FontStyles.Normal, TextAlignmentOptions.Left);
                    CreateButton(parent, "Sim", new Vector2(0.55f, y), () => SubmitConnectionVote(voterIndex, true), new Vector2(86f, 44f));
                    CreateButton(parent, "Não", new Vector2(0.78f, y), () => SubmitConnectionVote(voterIndex, false), new Vector2(86f, 44f));
                }

                row++;
            }
        }

        private void SubmitConnectionVote(int playerIndex, bool accepted)
        {
            if (turnManager == null || turnManager.CurrentStep != ScienceTurnStep.ConnectionExplanation) return;
            if (playerIndex == activeVotingPlayerIndex)
            {
                AddLog("O jogador ativo não vota na própria conexão.");
                BuildGameplayScreen();
                return;
            }

            connectionVotes[playerIndex] = accepted;
            AddLog($"{GetPlayerDisplayName(playerIndex)} votou {(accepted ? "Sim" : "Não")}.");
            telemetry?.LogEvent("science_connection_vote_submitted", $"turn={turnManager.TurnNumber};activePlayer={activeVotingPlayerIndex};voter={playerIndex};card={activeVotingCardId};accepted={accepted}");

            if (AreAllConnectionVotesSubmitted())
            {
                ResolveConnectionVoteResult();
                return;
            }

            BuildGameplayScreen();
        }

        private bool AreAllConnectionVotesSubmitted()
        {
            if (state == null) return true;
            int expectedVotes = Mathf.Max(0, state.Players.Count - 1);
            return connectionVotes.Count >= expectedVotes;
        }

        private void ResolveConnectionVoteResult()
        {
            int yesVotes = 0;
            int noVotes = 0;
            foreach (bool accepted in connectionVotes.Values)
            {
                if (accepted) yesVotes++;
                else noVotes++;
            }

            bool tied = yesVotes == noVotes;
            bool tieAccepted = state?.AcceptTiedConnectionVotes ?? true;
            bool acceptedByVote = yesVotes > noVotes || (tied && tieAccepted);
            string resultText = acceptedByVote ? "aceita" : "rejeitada";
            string tieText = tied ? $" Empate: {(tieAccepted ? "aceito" : "rejeitado")} pela configuração atual." : string.Empty;

            AddLog($"Conexão {resultText}: {yesVotes} Sim / {noVotes} Não.{tieText}");
            telemetry?.LogEvent("science_connection_vote_resolved", $"turn={turnManager?.TurnNumber ?? 0};activePlayer={activeVotingPlayerIndex};card={activeVotingCardId};yes={yesVotes};no={noVotes};tied={tied};tieAccepted={tieAccepted};accepted={acceptedByVote}");

            if (acceptedByVote)
            {
                state?.RecordAcceptedConnection();
                StartScoringForAcceptedConnection();
                BuildGameplayScreen();
                return;
            }

            ResolveRejectedConnection();
            BuildGameplayScreen();
        }

        private void StartScoringForAcceptedConnection()
        {
            activeScoringPlayerIndex = activeVotingPlayerIndex;
            activeScoringCardId = activeVotingCardId;
            basePointAwarded = false;
            interestingBonusAwarded = false;
            guideFactBonusAwarded = false;
            ResetConnectionVoting();
            turnManager?.StartScoring();
            telemetry?.LogEvent("science_scoring_started", $"turn={turnManager?.TurnNumber ?? 0};player={activeScoringPlayerIndex};card={activeScoringCardId}");
        }

        private void ResolveRejectedConnection()
        {
            SciencePlayerState currentPlayer = GetCurrentPlayer();
            ScienceCardData selectedCard = turnManager?.SelectedCard;
            Vector2Int coordinate = turnManager?.SelectedBoardCoordinate ?? Vector2Int.zero;
            ScienceRejectedConnectionBehavior behavior = state?.RejectedConnectionBehavior ?? ScienceRejectedConnectionBehavior.ReturnCardToHand;

            state?.RecordRejectedConnection();

            if (behavior == ScienceRejectedConnectionBehavior.RetryExplanation)
            {
                AddLog("Conexão rejeitada. O jogador pode tentar explicar novamente.");
                telemetry?.LogEvent("science_connection_rejected", $"turn={turnManager?.TurnNumber ?? 0};player={activeVotingPlayerIndex};card={activeVotingCardId};behavior={behavior}");
                ResetConnectionVoting();
                StartConnectionVoting(currentPlayer, selectedCard);
                return;
            }

            boardManager?.RemoveCardAt(coordinate);
            currentPlayer?.ReturnPlayedToHand(selectedCard);
            AddLog("Conexão rejeitada. A carta voltou para a mão do jogador ativo.");
            telemetry?.LogEvent("science_connection_rejected", $"turn={turnManager?.TurnNumber ?? 0};player={activeVotingPlayerIndex};card={activeVotingCardId};behavior={behavior}");
            ResetConnectionVoting();
            turnManager?.MarkTurnResolved();
        }

        private void BuildScoringPanel(RectTransform parent)
        {
            CreateText(parent, "Pontuação da conexão", 23, new Vector2(0.08f, 0.60f), new Vector2(0.92f, 0.66f), FontStyles.Bold);
            CreateText(parent, $"{GetPlayerDisplayName(activeScoringPlayerIndex)} teve a conexão aceita.\nBônus são decididos manualmente pelo grupo/facilitador.", 17, new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.60f), FontStyles.Normal);

            if (!basePointAwarded)
            {
                CreateButton(parent, "Award base point and continue", new Vector2(0.50f, 0.39f), AwardBaseConnectionPoint, new Vector2(300f, 50f));
            }
            else
            {
                CreateText(parent, "Ponto base concedido (+1).", 16, new Vector2(0.10f, 0.36f), new Vector2(0.90f, 0.42f), FontStyles.Bold);
            }

            if (!interestingBonusAwarded)
            {
                CreateButton(parent, "Add interesting explanation bonus", new Vector2(0.50f, 0.29f), AwardInterestingBonus, new Vector2(300f, 48f));
            }
            else
            {
                CreateText(parent, "Bônus de explicação interessante (+1).", 15, new Vector2(0.10f, 0.26f), new Vector2(0.90f, 0.32f), FontStyles.Bold);
            }

            if (!guideFactBonusAwarded)
            {
                CreateButton(parent, "Add guide/fact bonus", new Vector2(0.50f, 0.20f), AwardGuideFactBonus, new Vector2(300f, 48f));
            }
            else
            {
                CreateText(parent, "Bônus de fato/guia específico (+1).", 15, new Vector2(0.10f, 0.17f), new Vector2(0.90f, 0.23f), FontStyles.Bold);
            }

            CreateButton(parent, "Continue", new Vector2(0.50f, 0.09f), ContinueAfterScoring, new Vector2(220f, 54f));
        }

        private void AwardBaseConnectionPoint()
        {
            if (basePointAwarded) return;
            basePointAwarded = true;
            scoreManager?.AddScore(activeScoringPlayerIndex, 1, "accepted_connection_base");
            AddLog($"{GetPlayerDisplayName(activeScoringPlayerIndex)} recebeu +1 ponto pela conexão aceita.");
            telemetry?.LogEvent("science_connection_score_awarded", $"turn={turnManager?.TurnNumber ?? 0};player={activeScoringPlayerIndex};card={activeScoringCardId};type=base;amount=1;score={GetPlayerScore(activeScoringPlayerIndex)}");
            BuildGameplayScreen();
        }

        private void AwardInterestingBonus()
        {
            if (interestingBonusAwarded) return;
            interestingBonusAwarded = true;
            scoreManager?.AddScore(activeScoringPlayerIndex, 1, "interesting_explanation_bonus");
            AddLog($"{GetPlayerDisplayName(activeScoringPlayerIndex)} recebeu +1 bônus por explicação interessante.");
            telemetry?.LogEvent("science_connection_score_awarded", $"turn={turnManager?.TurnNumber ?? 0};player={activeScoringPlayerIndex};card={activeScoringCardId};type=interesting;amount=1;score={GetPlayerScore(activeScoringPlayerIndex)}");
            BuildGameplayScreen();
        }

        private void AwardGuideFactBonus()
        {
            if (guideFactBonusAwarded) return;
            guideFactBonusAwarded = true;
            scoreManager?.AddScore(activeScoringPlayerIndex, 1, "guide_fact_bonus");
            AddLog($"{GetPlayerDisplayName(activeScoringPlayerIndex)} recebeu +1 bônus por usar um fato específico.");
            telemetry?.LogEvent("science_connection_score_awarded", $"turn={turnManager?.TurnNumber ?? 0};player={activeScoringPlayerIndex};card={activeScoringCardId};type=guide_fact;amount=1;score={GetPlayerScore(activeScoringPlayerIndex)}");
            BuildGameplayScreen();
        }

        private void ContinueAfterScoring()
        {
            if (!basePointAwarded)
            {
                AwardBaseConnectionPoint();
                return;
            }

            telemetry?.LogEvent("science_connection_scoring_completed", $"turn={turnManager?.TurnNumber ?? 0};player={activeScoringPlayerIndex};card={activeScoringCardId};base={basePointAwarded};interesting={interestingBonusAwarded};guideFact={guideFactBonusAwarded};score={GetPlayerScore(activeScoringPlayerIndex)}");
            if (TryShowEndGameIfNeeded(ScienceWinCondition.TargetKnowledgePoints))
            {
                ResetScoringState();
                return;
            }

            ResetScoringState();
            turnManager?.MarkTurnResolved();
            BuildGameplayScreen();
        }

        private void ResolveActionCard(SciencePlayerState currentPlayer, ScienceActionCardData actionCard)
        {
            currentPlayer.MarkPlayed(actionCard);
            deckManager?.Discard(actionCard);
            AddLog($"{currentPlayer.DisplayName} jogou a ação {actionCard.DisplayName}. Resolução completa será implementada depois; a carta foi descartada.");
            if (TryShowEndGameIfNeeded(ScienceWinCondition.EmptyHand))
            {
                return;
            }

            turnManager?.MarkTurnResolved();
            BuildGameplayScreen();
        }

        private void CancelSelection()
        {
            ResetConnectionVoting();
            turnManager?.CancelSelection();
            AddLog("Seleção cancelada. Escolha outra carta da mão atual.");
            BuildGameplayScreen();
        }

        private ScienceCardData GetBoardCardAt(Vector2Int coordinate)
        {
            if (boardManager?.BoardCards == null) return null;
            return boardManager.BoardCards.TryGetValue(coordinate, out ScienceCardData card) ? card : null;
        }

        private void OpenCardDetailsModal(ScienceCardData card)
        {
            if (card == null || root == null) return;

            CloseCardDetailsModal();
            RectTransform parent = root.GetComponent<RectTransform>();
            cardDetailModal = new GameObject("CardDetailModal", typeof(RectTransform), typeof(Image));
            RectTransform overlay = cardDetailModal.GetComponent<RectTransform>();
            overlay.SetParent(parent, false);
            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;

            Image overlayImage = cardDetailModal.GetComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.72f);
            overlayImage.raycastTarget = true;

            RectTransform panel = CreatePanel(overlay, "CardDetailPanel", new Vector2(0.18f, 0.10f), new Vector2(0.82f, 0.90f), new Color(0.08f, 0.10f, 0.14f, 0.98f));
            CreateText(panel, "Detalhes da Carta", 34, new Vector2(0.05f, 0.90f), new Vector2(0.95f, 0.98f), FontStyles.Bold);

            ScienceCardView zoomCard = ScienceCardView.Create(panel, "ZoomCard", card, ScienceCardViewDisplayMode.ZoomModal);
            RectTransform zoomRect = zoomCard.GetComponent<RectTransform>();
            zoomRect.anchorMin = new Vector2(0.30f, 0.48f);
            zoomRect.anchorMax = new Vector2(0.30f, 0.48f);
            zoomRect.anchoredPosition = Vector2.zero;

            CreateText(panel, BuildCardDetailText(card), 24, new Vector2(0.55f, 0.26f), new Vector2(0.92f, 0.82f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
            CreateButton(panel, "Close", new Vector2(0.74f, 0.14f), CloseCardDetailsModal, new Vector2(220f, 70f));

            telemetry?.LogEvent("science_card_zoom_opened", $"card={card.Id};type={card.CardType}");
            if (card is ScienceCharacterCardData characterCard)
            {
                telemetry?.LogEvent("science_card_minibio_viewed", $"card={characterCard.Id};name={characterCard.DisplayName}");
            }
        }

        private void CloseCardDetailsModal()
        {
            if (cardDetailModal == null) return;
            cardDetailModal.SetActive(false);
            UnityEngine.Object.Destroy(cardDetailModal);
            cardDetailModal = null;
        }

        private static string BuildCardDetailText(ScienceCardData card)
        {
            if (card is ScienceCharacterCardData characterCard)
            {
                return $"Área: {characterCard.Field}\nCategorias: {characterCard.FactCategoryA} + {characterCard.FactCategoryB}\n\nDescrição: {characterCard.ShortDescription}\n\nMini bio:\n{characterCard.MiniBio}";
            }

            if (card is ScienceActionCardData actionCard)
            {
                string rules = string.IsNullOrEmpty(actionCard.RulesText) ? actionCard.ShortDescription : actionCard.RulesText;
                return $"Tipo de ação: {actionCard.EffectType}\n\nEfeito curto: {actionCard.ShortDescription}\n\nRegras completas:\n{rules}";
            }

            return card?.ShortDescription ?? "Sem detalhes disponíveis.";
        }

        private bool TryShowEndGameIfNeeded(ScienceWinCondition preferredCondition)
        {
            ScienceWinCondition winCondition = ResolveWinCondition(preferredCondition);
            if (winCondition == ScienceWinCondition.None) return false;

            IReadOnlyList<SciencePlayerState> winners = ResolveWinners(winCondition);
            string winnerNames = FormatWinnerNames(winners);
            state?.SetPhase(ScienceCardGamePhase.GameOver);
            AddLog($"Fim de jogo: {winnerNames} venceu por {FormatWinCondition(winCondition)}.");
            telemetry?.LogEvent("science_game_over", $"turns={turnManager?.TurnNumber ?? 0};condition={winCondition};winners={winnerNames};cardsPlaced={boardManager?.BoardCards?.Count ?? 0};accepted={state?.AcceptedConnections ?? 0};rejected={state?.RejectedConnections ?? 0}");
            BuildEndGameScreen(root.GetComponent<RectTransform>(), winners, winCondition);
            return true;
        }

        private ScienceWinCondition ResolveWinCondition(ScienceWinCondition preferredCondition)
        {
            bool hasTargetScoreWinner = HasTargetScoreWinner();
            bool hasEmptyHandWinner = HasEmptyHandWinner();

            if (preferredCondition == ScienceWinCondition.EmptyHand && hasEmptyHandWinner) return ScienceWinCondition.EmptyHand;
            if (preferredCondition == ScienceWinCondition.TargetKnowledgePoints && hasTargetScoreWinner) return ScienceWinCondition.TargetKnowledgePoints;
            if (hasTargetScoreWinner) return ScienceWinCondition.TargetKnowledgePoints;
            if (hasEmptyHandWinner) return ScienceWinCondition.EmptyHand;
            return ScienceWinCondition.None;
        }

        private bool HasTargetScoreWinner()
        {
            if (state == null) return false;
            for (int i = 0; i < state.Players.Count; i++)
            {
                SciencePlayerState player = state.Players[i];
                if (player != null && player.Score >= state.TargetKnowledgePoints) return true;
            }

            return false;
        }

        private bool HasEmptyHandWinner()
        {
            if (state == null) return false;
            for (int i = 0; i < state.Players.Count; i++)
            {
                SciencePlayerState player = state.Players[i];
                if (player != null && player.Hand.Count == 0) return true;
            }

            return false;
        }

        private IReadOnlyList<SciencePlayerState> ResolveWinners()
        {
            return ResolveWinners(ResolveWinCondition(ScienceWinCondition.None));
        }

        private IReadOnlyList<SciencePlayerState> ResolveWinners(ScienceWinCondition winCondition)
        {
            List<SciencePlayerState> winners = new List<SciencePlayerState>();
            if (state == null) return winners;

            int bestScore = int.MinValue;
            for (int i = 0; i < state.Players.Count; i++)
            {
                SciencePlayerState player = state.Players[i];
                if (player != null && player.Score > bestScore) bestScore = player.Score;
            }

            for (int i = 0; i < state.Players.Count; i++)
            {
                SciencePlayerState player = state.Players[i];
                if (player == null) continue;

                if (winCondition == ScienceWinCondition.TargetKnowledgePoints)
                {
                    if (player.Score >= state.TargetKnowledgePoints && player.Score == bestScore) winners.Add(player);
                    continue;
                }

                if (winCondition == ScienceWinCondition.EmptyHand)
                {
                    if (player.Score == bestScore) winners.Add(player);
                }
            }

            return winners;
        }

        private void BuildEndGameScreen(RectTransform screen, IReadOnlyList<SciencePlayerState> winners)
        {
            BuildEndGameScreen(screen, winners, ResolveWinCondition(ScienceWinCondition.None));
        }

        private void BuildEndGameScreen(RectTransform screen, IReadOnlyList<SciencePlayerState> winners, ScienceWinCondition winCondition)
        {
            if (screen == null) return;
            CloseCardDetailsModal();
            ClearChildren(screen);

            RectTransform panel = CreatePanel(screen, "ScienceCardGameEndGameScreen", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f), new Color(0.08f, 0.10f, 0.14f, 0.98f));
            CreateText(panel, "Fim de Jogo", 56, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.94f), FontStyles.Bold);
            CreateText(panel, $"Vencedor(es): {FormatWinnerNames(winners)}", 34, new Vector2(0.10f, 0.72f), new Vector2(0.90f, 0.80f), FontStyles.Bold);
            CreateText(panel, $"Condição: {FormatWinCondition(winCondition)}", 24, new Vector2(0.10f, 0.65f), new Vector2(0.90f, 0.71f), FontStyles.Italic);

            RectTransform statsPanel = CreatePanel(panel, "FinalStatsPanel", new Vector2(0.10f, 0.28f), new Vector2(0.90f, 0.62f), new Color(0.12f, 0.16f, 0.22f, 0.92f));
            CreateText(statsPanel, BuildFinalStatsText(), 25, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.92f), FontStyles.Normal, TextAlignmentOptions.TopLeft);

            CreateButton(panel, "Restart This Prototype", new Vector2(0.36f, 0.16f), RestartThisPrototype, new Vector2(330f, 82f));
            CreateButton(panel, "Back to Prototype Selection", new Vector2(0.64f, 0.16f), () => context?.ReturnToSelector?.Invoke(), new Vector2(380f, 82f));
        }

        private void RestartThisPrototype()
        {
            telemetry?.LogEvent("science_game_restart_requested", $"turns={turnManager?.TurnNumber ?? 0};phase={state?.CurrentPhase}");
            onRestartPrototype?.Invoke();
        }

        private string BuildFinalStatsText()
        {
            string scores = "Pontuações finais:";
            if (state != null)
            {
                for (int i = 0; i < state.Players.Count; i++)
                {
                    SciencePlayerState player = state.Players[i];
                    if (player == null) continue;
                    scores += $"\n• {player.DisplayName}: {player.Score} pontos, {player.Hand.Count} cartas na mão";
                }
            }

            return $"{scores}\n\nTurnos: {turnManager?.TurnNumber ?? 0}\nCartas no tabuleiro: {boardManager?.BoardCards?.Count ?? 0}\nConexões aceitas: {state?.AcceptedConnections ?? 0}\nConexões rejeitadas: {state?.RejectedConnections ?? 0}\nMeta de conhecimento: {state?.TargetKnowledgePoints ?? 7}";
        }

        private static string FormatWinCondition(ScienceWinCondition winCondition)
        {
            switch (winCondition)
            {
                case ScienceWinCondition.TargetKnowledgePoints:
                    return "meta de Knowledge Points alcançada";
                case ScienceWinCondition.EmptyHand:
                    return "mão esvaziada; empate decidido pela maior pontuação";
                default:
                    return "não definida";
            }
        }

        private static string FormatWinnerNames(IReadOnlyList<SciencePlayerState> winners)
        {
            if (winners == null || winners.Count == 0) return "sem vencedor";

            string text = string.Empty;
            for (int i = 0; i < winners.Count; i++)
            {
                if (winners[i] == null) continue;
                if (!string.IsNullOrEmpty(text)) text += ", ";
                text += winners[i].DisplayName;
            }

            return string.IsNullOrEmpty(text) ? "sem vencedor" : text;
        }

        private void DrawCardForCurrentPlayer()
        {
            SciencePlayerState currentPlayer = GetCurrentPlayer();
            if (currentPlayer == null)
            {
                AddLog("Não há jogador atual para comprar carta.");
                BuildGameplayScreen();
                return;
            }

            ScienceCardData card = deckManager?.DrawCard();
            if (card == null)
            {
                AddLog("Deck vazio: nenhuma carta comprada.");
                BuildGameplayScreen();
                return;
            }

            currentPlayer.AddToHand(card);
            AddLog($"{currentPlayer.DisplayName} comprou {card.DisplayName} ({card.CardType}).");
            BuildGameplayScreen();
        }

        private void EndTurn()
        {
            if (turnManager == null || turnManager.CurrentStep != ScienceTurnStep.TurnResolved)
            {
                AddLog("Resolva a ação atual antes de encerrar o turno.");
                BuildGameplayScreen();
                return;
            }

            ResetConnectionVoting();
            turnManager.AdvanceTurn();
            AddLog($"Turno {turnManager.TurnNumber}: vez de {GetCurrentPlayerName()}.");
            BuildGameplayScreen();
        }

        private void AddPlaceholderAction(string message)
        {
            AddLog(message);
            BuildGameplayScreen();
        }

        private void SelectPlayerCount(int playerCount)
        {
            selectedPlayerCount = Mathf.Clamp(playerCount, 2, 4);
            UpdateSelectedPlayerCountText();
            telemetry?.LogEvent("science_setup_player_count_selected", $"players={selectedPlayerCount}");
        }

        private void UpdateSelectedPlayerCountText()
        {
            if (selectedPlayerCountText != null)
            {
                selectedPlayerCountText.text = $"Jogadores selecionados: {selectedPlayerCount}";
            }
        }

        private string BuildTurnInstruction()
        {
            switch (turnManager?.CurrentStep ?? ScienceTurnStep.AwaitingCardSelection)
            {
                case ScienceTurnStep.AwaitingBoardSlot:
                    return "Escolha um espaço livre no tabuleiro.";
                case ScienceTurnStep.AwaitingPlacementConfirmation:
                    return "Confirme a posição escolhida.";
                case ScienceTurnStep.ConnectionExplanation:
                    return "Os demais jogadores votam a conexão.";
                case ScienceTurnStep.Scoring:
                    return "Atribua pontos e bônus.";
                case ScienceTurnStep.ActionResolution:
                    return "Resolva a ação selecionada.";
                case ScienceTurnStep.TurnResolved:
                    return "Turno resolvido. Encerre para passar ao próximo jogador.";
                default:
                    return "Selecione uma carta da mão atual.";
            }
        }

        private string BuildSelectedCardText()
        {
            ScienceCardData selectedCard = turnManager?.SelectedCard;
            if (selectedCard == null) return "Nenhuma carta selecionada.";

            string text = $"Selecionada: {selectedCard.DisplayName} [{selectedCard.CardType}]\nRotação: {turnManager?.SelectedRotationDegrees ?? 0}°";
            if (turnManager != null && turnManager.HasSelectedBoardCoordinate)
            {
                Vector2Int coord = turnManager.SelectedBoardCoordinate;
                text += $"\nPosição: {coord.x + 1},{coord.y + 1}";
            }

            return text;
        }

        private string BuildTurnHelpText()
        {
            switch (turnManager?.CurrentStep ?? ScienceTurnStep.AwaitingCardSelection)
            {
                case ScienceTurnStep.AwaitingBoardSlot:
                    return "Gire a carta se quiser alinhar fatos/cores; depois escolha um slot verde.";
                case ScienceTurnStep.AwaitingPlacementConfirmation:
                    return "A prévia no tabuleiro mostra a rotação escolhida. Confirme, gire novamente ou cancele.";
                case ScienceTurnStep.ConnectionExplanation:
                    return "O consenso dos jogadores valida a explicação; o sistema não julga automaticamente.";
                case ScienceTurnStep.Scoring:
                    return "A conexão foi aceita. Atribua o ponto base e bônus manuais antes de continuar.";
                case ScienceTurnStep.TurnResolved:
                    return "A colocação/ação foi resolvida. O botão Encerrar turno agora está disponível.";
                default:
                    return "Clique em uma carta da mão do jogador atual para iniciar a jogada.";
            }
        }

        private string BuildScoreLine()
        {
            if (state == null || state.Players.Count == 0) return "Pontuação: sem jogadores";

            string text = "Pontuação:";
            for (int i = 0; i < state.Players.Count; i++)
            {
                SciencePlayerState player = state.Players[i];
                if (player == null) continue;
                text += $"  {player.DisplayName}: {player.Score}";
            }

            return text;
        }

        private string BuildRecentActionLog()
        {
            if (recentActions.Count == 0) return "Nenhuma ação registrada ainda.";

            string text = string.Empty;
            for (int i = recentActions.Count - 1; i >= 0; i--)
            {
                text += $"• {recentActions[i]}";
                if (i > 0) text += "\n\n";
            }

            return text;
        }

        private string GetPlayerDisplayName(int playerIndex)
        {
            if (state != null)
            {
                for (int i = 0; i < state.Players.Count; i++)
                {
                    SciencePlayerState player = state.Players[i];
                    if (player != null && player.PlayerIndex == playerIndex) return player.DisplayName;
                }
            }

            return $"Player {playerIndex + 1}";
        }

        private int GetPlayerScore(int playerIndex)
        {
            if (state != null)
            {
                for (int i = 0; i < state.Players.Count; i++)
                {
                    SciencePlayerState player = state.Players[i];
                    if (player != null && player.PlayerIndex == playerIndex) return player.Score;
                }
            }

            return 0;
        }

        private void ResetConnectionVoting()
        {
            connectionVotes.Clear();
            activeVotingPlayerIndex = -1;
            activeVotingCardId = null;
        }

        private void ResetScoringState()
        {
            activeScoringPlayerIndex = -1;
            activeScoringCardId = null;
            basePointAwarded = false;
            interestingBonusAwarded = false;
            guideFactBonusAwarded = false;
        }

        private string GetCurrentPlayerName()
        {
            SciencePlayerState player = GetCurrentPlayer();
            return player?.DisplayName ?? "sem jogador";
        }

        private SciencePlayerState GetCurrentPlayer()
        {
            if (state == null || state.Players.Count == 0) return null;
            int currentPlayerIndex = Mathf.Clamp(turnManager?.CurrentPlayerIndex ?? 0, 0, state.Players.Count - 1);
            return state.Players[currentPlayerIndex];
        }

        private void AddLog(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            recentActions.Add(message);
            while (recentActions.Count > MaxLogEntries)
            {
                recentActions.RemoveAt(0);
            }

            telemetry?.LogEvent("science_ui_action_log", message);
        }

        private static bool ContainsCard(IReadOnlyList<ScienceCardData> cards, ScienceCardData targetCard)
        {
            if (cards == null || targetCard == null) return false;
            for (int i = 0; i < cards.Count; i++)
            {
                if (ReferenceEquals(cards[i], targetCard)) return true;
            }

            return false;
        }

        private static int CountCardsOfType(SciencePlayerState player, ScienceCardType cardType)
        {
            if (player == null) return 0;
            int count = 0;
            foreach (ScienceCardData card in player.Hand)
            {
                if (card != null && card.CardType == cardType) count++;
            }

            return count;
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }
        }

        private static RectTransform CreatePanel(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = panel.GetComponent<Image>();
            image.color = color;
            return rect;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style)
        {
            return CreateText(parent, value, size, anchorMin, anchorMax, style, TextAlignmentOptions.Center);
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }

        private static void CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            CreateButton(parent, label, anchor, onClick, new Vector2(720f, 112f));
        }

        private static void CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick, Vector2 size)
        {
            GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.45f, 0.82f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(buttonObject.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = size.x < 300f ? 20 : 34;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
        }
    }
}
