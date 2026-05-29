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
        private const float ReferenceLandscapeWidth = 1920f;
        private const float ReferenceLandscapeHeight = 1080f;

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
        private GameObject debugLogModal;
        private GameObject handCardContextMenu;
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
            CloseDebugLogModal();
            CloseHandCardContextMenu();
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
            CloseDebugLogModal();
            CloseHandCardContextMenu();
            RectTransform screen = root.GetComponent<RectTransform>();
            ClearChildren(root.transform);

            if (state != null && state.CurrentPhase == ScienceCardGamePhase.GameOver)
            {
                BuildEndGameScreen(screen, ResolveWinners());
                return;
            }

            RectTransform topBar = CreatePanel(screen, "TopBar", new Vector2(0.02f, 0.89f), new Vector2(0.98f, 0.985f), new Color(0.10f, 0.13f, 0.18f, 0.96f));
            RectTransform boardPanel = CreatePanel(screen, "BoardPanel", new Vector2(0.02f, 0.29f), new Vector2(0.98f, 0.875f), new Color(0.12f, 0.20f, 0.17f, 0.96f));
            RectTransform handPanel = CreatePanel(screen, "CurrentPlayerHandPanel", new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.27f), new Color(0.11f, 0.12f, 0.18f, 0.96f));

            BuildTopBar(topBar);
            BuildBoardPanel(boardPanel);
            BuildHandPanel(handPanel);
            BuildContextualTurnPanel(screen);
        }

        private void BuildTopBar(RectTransform parent)
        {
            CreateText(parent, $"{GetCurrentPlayerName()}  |  Turno {turnManager?.TurnNumber ?? 0}", 30, new Vector2(0.02f, 0.42f), new Vector2(0.28f, 0.92f), FontStyles.Bold, TextAlignmentOptions.Left);
            CreateText(parent, BuildTurnInstruction(), 22, new Vector2(0.02f, 0.08f), new Vector2(0.46f, 0.42f), FontStyles.Italic, TextAlignmentOptions.Left);
            CreateText(parent, BuildScoreLine(), 24, new Vector2(0.48f, 0.12f), new Vector2(0.80f, 0.90f), FontStyles.Normal, TextAlignmentOptions.Left);
            CreateButton(parent, "Debug", new Vector2(0.85f, 0.50f), OpenDebugLogModal, new Vector2(150f, 62f));
            CreateButton(parent, "Menu", new Vector2(0.94f, 0.50f), () => context?.ReturnToSelector?.Invoke(), new Vector2(150f, 62f));
        }

        private void BuildLogPanel(RectTransform parent)
        {
            CreateText(parent, "Log", 26, new Vector2(0.08f, 0.90f), new Vector2(0.92f, 0.98f), FontStyles.Bold);
            CreateText(parent, BuildRecentActionLog(), 20, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.88f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        }

        private void BuildBoardPanel(RectTransform parent)
        {
            CreateText(parent, "Tabuleiro", 32, new Vector2(0.03f, 0.91f), new Vector2(0.30f, 0.99f), FontStyles.Bold, TextAlignmentOptions.Left);
            CreateText(parent, "Verde = válido · Vermelho = inválido · Dourado = selecionado", 23, new Vector2(0.32f, 0.90f), new Vector2(0.97f, 0.99f), FontStyles.Italic, TextAlignmentOptions.Right);

            RectTransform grid = CreatePanel(parent, "BoardGrid", new Vector2(0.015f, 0.035f), new Vector2(0.985f, 0.890f), new Color(0.04f, 0.09f, 0.08f, 0.95f));
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
                    float slotPadding = ScaleValue(4f);
                    slot.offsetMin = new Vector2(slot.offsetMin.x + slotPadding, slot.offsetMin.y + slotPadding);
                    slot.offsetMax = new Vector2(slot.offsetMax.x - slotPadding, slot.offsetMax.y - slotPadding);
                    ApplyBoardSlotOutline(slot, coordinate, selectedCard, isSelectedSlot);
                    ScienceCardData boardCard = GetBoardCardAt(coordinate);
                    if (boardCard != null)
                    {
                        ScienceCardView boardCardView = ScienceCardView.Create(slot, $"BoardCard_{x}_{y}", boardCard, ScienceCardViewDisplayMode.Board, OpenCardDetailsModal);
                        RectTransform boardCardRect = boardCardView.GetComponent<RectTransform>();
                        boardCardRect.anchorMin = new Vector2(0.5f, 0.5f);
                        boardCardRect.anchorMax = new Vector2(0.5f, 0.5f);
                        boardCardRect.sizeDelta = GetBoardCardSize(columns, rows);
                        boardCardRect.anchoredPosition = Vector2.zero;
                        ApplyCardOutline(boardCardRect, new Color(0.92f, 0.98f, 1f, 0.95f), 2.5f);
                        ApplyBoardCardRotation(boardCardRect, boardManager?.GetPlacedCardRotationDegrees(coordinate) ?? 0);
                    }
                    else
                    {
                        if (IsPlacementStep())
                        {
                            ConfigureBoardSlotButton(slot, coordinate);
                        }

                        if (isSelectedSlot && selectedCard != null)
                        {
                            ScienceCardView previewCardView = ScienceCardView.Create(slot, $"BoardPreview_{x}_{y}", selectedCard, ScienceCardViewDisplayMode.Board);
                            RectTransform previewRect = previewCardView.GetComponent<RectTransform>();
                            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
                            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
                            previewRect.sizeDelta = GetBoardCardSize(columns, rows);
                            previewRect.anchoredPosition = Vector2.zero;
                            bool previewValid = boardManager != null && boardManager.CanPlaceCardAt(coordinate, selectedCard, turnManager?.SelectedRotationDegrees ?? 0);
                            ApplyCardOutline(previewRect, previewValid ? new Color(0.42f, 1f, 0.52f, 1f) : new Color(1f, 0.25f, 0.22f, 1f), 4f);
                            ApplyBoardCardRotation(previewRect, turnManager?.SelectedRotationDegrees ?? 0);
                        }
                        else
                        {
                            BuildEmptySlotHint(slot, coordinate, selectedCard);
                        }
                    }
                }
            }
        }

        private void ApplyBoardSlotOutline(RectTransform slot, Vector2Int coordinate, ScienceCardData selectedCard, bool isSelectedSlot)
        {
            Outline outline = slot.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(ScaleValue(2.5f), -ScaleValue(2.5f));
            outline.effectColor = GetBoardSlotOutlineColor(coordinate, selectedCard, isSelectedSlot);
        }

        private Color GetBoardSlotOutlineColor(Vector2Int coordinate, ScienceCardData selectedCard, bool isSelectedSlot)
        {
            if (isSelectedSlot && IsPlacementStep() && selectedCard != null)
            {
                bool selectedValid = boardManager != null && boardManager.CanPlaceCardAt(coordinate, selectedCard, turnManager?.SelectedRotationDegrees ?? 0);
                return selectedValid ? new Color(0.35f, 0.95f, 0.48f, 1f) : new Color(1f, 0.24f, 0.20f, 1f);
            }

            if (isSelectedSlot) return new Color(1f, 0.86f, 0.24f, 1f);
            if (GetBoardCardAt(coordinate) != null) return new Color(0.72f, 0.82f, 0.92f, 0.95f);

            if (IsPlacementStep() && selectedCard != null)
            {
                bool isValid = boardManager != null && boardManager.CanPlaceCardAt(coordinate, selectedCard, turnManager?.SelectedRotationDegrees ?? 0);
                return isValid ? new Color(0.34f, 0.92f, 0.46f, 0.92f) : new Color(0.50f, 0.58f, 0.55f, 0.30f);
            }

            return new Color(0.38f, 0.52f, 0.48f, 0.28f);
        }

        private void BuildEmptySlotHint(RectTransform slot, Vector2Int coordinate, ScienceCardData selectedCard)
        {
            if (IsPlacementStep() && selectedCard != null)
            {
                bool isValid = boardManager != null && boardManager.CanPlaceCardAt(coordinate, selectedCard, turnManager?.SelectedRotationDegrees ?? 0);
                if (isValid)
                {
                    CreateText(slot, "✓", 24, new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.82f), FontStyles.Bold, TextAlignmentOptions.Center);
                }
                return;
            }

            CreateText(slot, "·", 20, new Vector2(0.24f, 0.24f), new Vector2(0.76f, 0.76f), FontStyles.Normal, TextAlignmentOptions.Center);
        }

        private Vector2 GetBoardCardSize(int columns, int rows)
        {
            float boardWidth = Mathf.Max(Screen.width, 1) * 0.96f * 0.95f;
            float boardHeight = Mathf.Max(Screen.height, 1) * 0.535f * 0.84f;
            float cellWidth = boardWidth / Mathf.Max(1, columns);
            float cellHeight = boardHeight / Mathf.Max(1, rows);
            float height = Mathf.Clamp(cellHeight - ScaleValue(10f), ScaleValue(56f), ScaleValue(96f));
            float width = Mathf.Clamp(cellWidth - ScaleValue(12f), ScaleValue(74f), ScaleValue(132f));
            return new Vector2(width, height);
        }

        private void ApplyCardOutline(RectTransform cardRect, Color color, float width)
        {
            if (cardRect == null) return;

            Outline outline = cardRect.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(ScaleValue(width), -ScaleValue(width));
        }

        private static void ApplyBoardCardRotation(RectTransform cardRect, int rotationDegrees)
        {
            if (cardRect == null) return;
            cardRect.localEulerAngles = new Vector3(0f, 0f, -ScienceBoardSlotState.NormalizeRotation(rotationDegrees));
        }

        private Color GetBoardSlotColor(Vector2Int coordinate, ScienceCardData selectedCard, bool isSelectedSlot)
        {
            if (isSelectedSlot && IsPlacementStep() && selectedCard != null)
            {
                bool selectedValid = boardManager != null && boardManager.CanPlaceCardAt(coordinate, selectedCard, turnManager?.SelectedRotationDegrees ?? 0);
                return selectedValid ? new Color(0.18f, 0.52f, 0.26f, 0.95f) : new Color(0.58f, 0.14f, 0.14f, 0.95f);
            }

            if (isSelectedSlot) return new Color(0.82f, 0.68f, 0.24f, 0.95f);
            if (GetBoardCardAt(coordinate) != null) return new Color(0.18f, 0.24f, 0.30f, 0.88f);

            if (IsPlacementStep() && selectedCard != null)
            {
                bool isValid = boardManager != null && boardManager.CanPlaceCardAt(coordinate, selectedCard, turnManager?.SelectedRotationDegrees ?? 0);
                return isValid ? new Color(0.14f, 0.38f, 0.22f, 0.78f) : new Color(0.12f, 0.22f, 0.19f, 0.62f);
            }

            return new Color(0.11f, 0.20f, 0.18f, 0.70f);
        }

        private static string FormatBoardCoordinate(Vector2Int coordinate)
        {
            return $"{coordinate.x + 1},{coordinate.y + 1}";
        }

        private void BuildHandPanel(RectTransform parent)
        {
            SciencePlayerState currentPlayer = GetCurrentPlayer();
            string title = currentPlayer == null ? "Mão do jogador" : $"Mão de {currentPlayer.DisplayName}";
            CreateText(parent, title, 32, new Vector2(0.03f, 0.80f), new Vector2(0.34f, 0.97f), FontStyles.Bold, TextAlignmentOptions.Left);

            if (currentPlayer == null || currentPlayer.Hand.Count == 0)
            {
                CreateText(parent, "Nenhuma carta na mão atual.", 26, new Vector2(0.04f, 0.20f), new Vector2(0.96f, 0.70f), FontStyles.Italic);
                return;
            }

            int characterCount = CountCardsOfType(currentPlayer, ScienceCardType.Character);
            int actionCount = CountCardsOfType(currentPlayer, ScienceCardType.Action);
            CreateText(parent, $"{currentPlayer.Hand.Count} cartas: {characterCount} personagens, {actionCount} ações", 25, new Vector2(0.36f, 0.80f), new Vector2(0.96f, 0.96f), FontStyles.Normal, TextAlignmentOptions.Right);

            RectTransform viewport = CreateScrollViewport(parent, "CurrentPlayerHandScroll", new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.78f), new Color(0.06f, 0.07f, 0.11f, 0.55f), out ScrollRect scrollRect);
            RectTransform content = CreateHandContent(viewport, currentPlayer.Hand.Count);
            scrollRect.content = content;

            Vector2 cardSize = GetHandCardSize();
            float spacing = ScaleValue(18f);
            float leftPadding = ScaleValue(18f);
            for (int i = 0; i < currentPlayer.Hand.Count; i++)
            {
                ScienceCardData card = currentPlayer.Hand[i];
                RectTransform cardRect = CreateCardView(content, card, $"HandCard_{i}");
                cardRect.anchorMin = new Vector2(0f, 0.5f);
                cardRect.anchorMax = new Vector2(0f, 0.5f);
                cardRect.pivot = new Vector2(0.5f, 0.5f);
                cardRect.sizeDelta = cardSize;
                cardRect.anchoredPosition = new Vector2(leftPadding + (cardSize.x * 0.5f) + (i * (cardSize.x + spacing)), 0f);
                ApplyHandCardSelectionFeedback(cardRect, card);
            }
        }

        private void ApplyHandCardSelectionFeedback(RectTransform cardRect, ScienceCardData card)
        {
            if (cardRect == null || card == null || turnManager?.SelectedCard != card) return;

            Outline outline = cardRect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.86f, 0.24f, 1f);
            outline.effectDistance = new Vector2(ScaleValue(5f), -ScaleValue(5f));
        }

        private RectTransform CreateScrollViewport(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color, out ScrollRect scrollRect)
        {
            GameObject viewportObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            viewport.SetParent(parent, false);
            viewport.anchorMin = anchorMin;
            viewport.anchorMax = anchorMax;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;

            Image image = viewportObject.GetComponent<Image>();
            image.color = color;

            CreateHandBlankCloseArea(viewport);

            Mask mask = viewportObject.GetComponent<Mask>();
            mask.showMaskGraphic = true;

            scrollRect = viewportObject.GetComponent<ScrollRect>();
            scrollRect.viewport = viewport;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = ScaleValue(30f);
            return viewport;
        }

        private void CreateHandBlankCloseArea(RectTransform viewport)
        {
            GameObject closeObject = new GameObject("HandBlankCloseArea", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform closeRect = closeObject.GetComponent<RectTransform>();
            closeRect.SetParent(viewport, false);
            closeRect.anchorMin = Vector2.zero;
            closeRect.anchorMax = Vector2.one;
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;

            Image closeImage = closeObject.GetComponent<Image>();
            closeImage.color = new Color(0f, 0f, 0f, 0.01f);

            Button closeButton = closeObject.GetComponent<Button>();
            closeButton.targetGraphic = closeImage;
            closeButton.onClick.AddListener(CloseHandCardContextMenu);
        }

        private RectTransform CreateHandContent(RectTransform viewport, int cardCount)
        {
            GameObject contentObject = new GameObject("HandScrollContent", typeof(RectTransform));
            RectTransform content = contentObject.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(0f, 1f);
            content.pivot = new Vector2(0f, 0.5f);

            Vector2 cardSize = GetHandCardSize();
            float spacing = ScaleValue(18f);
            float padding = ScaleValue(36f);
            float contentWidth = Mathf.Max(Screen.width * 0.90f, padding + (cardCount * cardSize.x) + (Mathf.Max(0, cardCount - 1) * spacing));
            content.sizeDelta = new Vector2(contentWidth, 0f);
            content.anchoredPosition = Vector2.zero;
            return content;
        }

        private void BuildContextualTurnPanel(RectTransform screen)
        {
            ScienceTurnStep step = turnManager?.CurrentStep ?? ScienceTurnStep.AwaitingCardSelection;
            if (step == ScienceTurnStep.AwaitingCardSelection) return;

            if (step == ScienceTurnStep.AwaitingBoardSlot || step == ScienceTurnStep.AwaitingPlacementConfirmation)
            {
                RectTransform placementPanel = CreatePanel(screen, "PlacementControlPanel", new Vector2(0.06f, 0.275f), new Vector2(0.94f, 0.445f), new Color(0.06f, 0.08f, 0.12f, 0.98f));
                BuildPlacementControlPanel(placementPanel);
                return;
            }

            RectTransform modalPanel = CreateGameplayModalPanel(screen, GetModalNameForStep(step));
            switch (step)
            {
                case ScienceTurnStep.ConnectionExplanation:
                    BuildConnectionVotingPanel(modalPanel);
                    break;
                case ScienceTurnStep.Scoring:
                    BuildScoringPanel(modalPanel);
                    break;
                case ScienceTurnStep.ActionResolution:
                    BuildActionResolutionPanel(modalPanel);
                    break;
                case ScienceTurnStep.TurnResolved:
                    BuildTurnResolvedModal(modalPanel);
                    break;
            }
        }

        private RectTransform CreateGameplayModalPanel(RectTransform screen, string panelName)
        {
            RectTransform overlay = CreatePanel(screen, panelName + "Overlay", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.62f));
            return CreatePanel(overlay, panelName, new Vector2(0.14f, 0.12f), new Vector2(0.86f, 0.88f), new Color(0.08f, 0.10f, 0.14f, 0.99f));
        }

        private static string GetModalNameForStep(ScienceTurnStep step)
        {
            switch (step)
            {
                case ScienceTurnStep.ConnectionExplanation:
                    return "VotingModal";
                case ScienceTurnStep.Scoring:
                    return "ScoringModal";
                case ScienceTurnStep.ActionResolution:
                    return "ActionResolutionModal";
                case ScienceTurnStep.TurnResolved:
                    return "TurnResolvedModal";
                default:
                    return "GameplayStateModal";
            }
        }

        private void BuildTurnResolvedModal(RectTransform parent)
        {
            CreateText(parent, "Turno resolvido", 42, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.92f), FontStyles.Bold);
            CreateText(parent, "A colocação ou ação foi resolvida. Passe o dispositivo para o próximo jogador quando todos estiverem prontos.", 28, new Vector2(0.10f, 0.42f), new Vector2(0.90f, 0.70f), FontStyles.Normal);
            CreateButton(parent, "Encerrar turno", new Vector2(0.50f, 0.24f), EndTurn, new Vector2(420f, 96f));
        }

        private void BuildPlacementControlPanel(RectTransform parent)
        {
            ScienceCardData selectedCard = turnManager?.SelectedCard;
            bool hasSlot = turnManager != null && turnManager.HasSelectedBoardCoordinate;
            bool isValid = IsSelectedPlacementValid();
            bool canConfirm = CanConfirmSelectedPlacement();
            bool showRotation = IsDualColorCharacterCard(selectedCard);

            CreateText(parent, selectedCard == null ? "Modo de posicionamento" : $"Posicionar: {selectedCard.DisplayName}", 27, new Vector2(0.03f, 0.68f), new Vector2(0.35f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Left);
            CreateText(parent, hasSlot ? $"Slot: {FormatBoardCoordinate(turnManager.SelectedBoardCoordinate)} · Rotação {turnManager.SelectedRotationDegrees}°" : "Toque em um slot vazio destacado no tabuleiro.", 22, new Vector2(0.03f, 0.36f), new Vector2(0.35f, 0.66f), FontStyles.Normal, TextAlignmentOptions.Left);

            string validationText = BuildPlacementValidityText(hasSlot, isValid);
            TextMeshProUGUI validityLabel = CreateText(parent, validationText, 23, new Vector2(0.37f, 0.58f), new Vector2(0.67f, 0.92f), FontStyles.Bold, TextAlignmentOptions.Center);
            validityLabel.color = isValid ? new Color(0.42f, 1f, 0.52f, 1f) : new Color(1f, 0.42f, 0.38f, 1f);

            if (showRotation)
            {
                CreateButton(parent, "⟲ Girar", new Vector2(0.43f, 0.28f), RotateSelectedCardLeft, new Vector2(210f, 70f));
                CreateButton(parent, "Girar ⟳", new Vector2(0.61f, 0.28f), RotateSelectedCardRight, new Vector2(210f, 70f));
            }
            else
            {
                CreateText(parent, "Carta de cor única: rotação é opcional.", 20, new Vector2(0.38f, 0.18f), new Vector2(0.66f, 0.42f), FontStyles.Italic, TextAlignmentOptions.Center);
            }

            CreateButton(parent, "Confirmar", new Vector2(0.80f, 0.62f), ConfirmPlacement, new Vector2(260f, 82f), canConfirm);
            CreateButton(parent, "Cancelar", new Vector2(0.80f, 0.26f), CancelSelection, new Vector2(260f, 76f));
        }

        private void BuildActionPanel(RectTransform parent)
        {
            CreateText(parent, "Próxima ação", 31, new Vector2(0.08f, 0.90f), new Vector2(0.92f, 0.98f), FontStyles.Bold);
            CreateText(parent, $"Deck: {deckManager?.DrawPile?.Count ?? 0}  |  Descarte: {deckManager?.DiscardPile?.Count ?? 0}", 22, new Vector2(0.10f, 0.82f), new Vector2(0.90f, 0.89f), FontStyles.Normal);
            CreateText(parent, BuildSelectedCardText(), 22, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.80f), FontStyles.Normal, TextAlignmentOptions.Top);

            ScienceTurnStep step = turnManager?.CurrentStep ?? ScienceTurnStep.AwaitingCardSelection;
            if (step == ScienceTurnStep.AwaitingBoardSlot || step == ScienceTurnStep.AwaitingPlacementConfirmation)
            {
                CreateButton(parent, "Girar esquerda", new Vector2(0.35f, 0.56f), RotateSelectedCardLeft, new Vector2(280f, 72f));
                CreateButton(parent, "Girar direita", new Vector2(0.65f, 0.56f), RotateSelectedCardRight, new Vector2(280f, 72f));
            }

            if (step == ScienceTurnStep.AwaitingPlacementConfirmation)
            {
                CreateButton(parent, "Confirmar posição", new Vector2(0.50f, 0.40f), ConfirmPlacement, new Vector2(340f, 82f));
            }

            if (step == ScienceTurnStep.ConnectionExplanation)
            {
                BuildConnectionVotingPanel(parent);
            }

            if (step == ScienceTurnStep.Scoring)
            {
                BuildScoringPanel(parent);
            }

            if (step == ScienceTurnStep.ActionResolution)
            {
                BuildActionResolutionPanel(parent);
            }

            if (step == ScienceTurnStep.AwaitingBoardSlot || step == ScienceTurnStep.AwaitingPlacementConfirmation)
            {
                CreateButton(parent, "Cancelar seleção", new Vector2(0.50f, 0.22f), CancelSelection, new Vector2(320f, 72f));
            }

            if (step == ScienceTurnStep.TurnResolved)
            {
                CreateButton(parent, "Encerrar turno", new Vector2(0.50f, 0.45f), EndTurn, new Vector2(360f, 92f));
            }

            CreateText(parent, BuildTurnHelpText(), 21, new Vector2(0.10f, 0.04f), new Vector2(0.90f, 0.18f), FontStyles.Italic, TextAlignmentOptions.Top);
        }

        private RectTransform CreateCardView(RectTransform parent, ScienceCardData card, string name)
        {
            ScienceCardView view = ScienceCardView.Create(parent, name, card, ScienceCardViewDisplayMode.Hand);
            RectTransform cardRect = view.GetComponent<RectTransform>();
            if (turnManager != null && turnManager.CurrentStep == ScienceTurnStep.AwaitingCardSelection)
            {
                view.SetOnSelected(selectedCard => ShowHandCardContextMenu(selectedCard, cardRect));
            }

            return cardRect;
        }

        private void ShowHandCardContextMenu(ScienceCardData card, RectTransform cardRect)
        {
            SciencePlayerState currentPlayer = GetCurrentPlayer();
            if (card == null || cardRect == null || root == null || currentPlayer == null || !ContainsCard(currentPlayer.Hand, card))
            {
                AddLog("Apenas cartas da mão do jogador atual podem abrir ações contextuais.");
                CloseHandCardContextMenu();
                return;
            }

            CloseHandCardContextMenu();

            RectTransform rootRect = root.GetComponent<RectTransform>();
            handCardContextMenu = new GameObject("HandCardContextMenu", typeof(RectTransform));
            RectTransform menuRoot = handCardContextMenu.GetComponent<RectTransform>();
            menuRoot.SetParent(rootRect, false);
            menuRoot.anchorMin = Vector2.zero;
            menuRoot.anchorMax = Vector2.one;
            menuRoot.offsetMin = Vector2.zero;
            menuRoot.offsetMax = Vector2.zero;

            RectTransform closeZone = CreatePanel(menuRoot, "HandCardContextCloseZone", new Vector2(0f, 0.32f), Vector2.one, new Color(0f, 0f, 0f, 0.01f));
            Button closeButton = closeZone.gameObject.AddComponent<Button>();
            closeButton.targetGraphic = closeZone.GetComponent<Image>();
            closeButton.onClick.AddListener(CloseHandCardContextMenu);

            RectTransform menuPanel = CreatePanel(menuRoot, "HandCardContextPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.06f, 0.08f, 0.12f, 0.98f));
            Vector2 menuSize = ScaleVector(new Vector2(460f, 132f));
            menuPanel.pivot = new Vector2(0.5f, 0.5f);
            menuPanel.sizeDelta = menuSize;
            menuPanel.anchoredPosition = ResolveContextMenuPosition(rootRect, cardRect, menuSize);

            CreateText(menuPanel, card.DisplayName, 22, new Vector2(0.05f, 0.66f), new Vector2(0.95f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateButton(menuPanel, "Abrir", new Vector2(0.28f, 0.34f), () => OpenContextCardDetails(card), new Vector2(190f, 76f));
            CreateButton(menuPanel, "Escolher", new Vector2(0.72f, 0.34f), () => ChooseContextCard(card), new Vector2(210f, 76f));
        }

        private Vector2 ResolveContextMenuPosition(RectTransform rootRect, RectTransform cardRect, Vector2 menuSize)
        {
            Vector3[] corners = new Vector3[4];
            cardRect.GetWorldCorners(corners);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, (corners[1] + corners[2]) * 0.5f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, screenPoint, null, out Vector2 localPoint);

            Rect rootBounds = rootRect.rect;
            float verticalOffset = (menuSize.y * 0.5f) + ScaleValue(26f);
            Vector2 position = new Vector2(localPoint.x, localPoint.y + verticalOffset);
            if (position.y + (menuSize.y * 0.5f) > rootBounds.yMax - ScaleValue(12f))
            {
                position.y = localPoint.y - verticalOffset;
            }

            position.x = Mathf.Clamp(position.x, rootBounds.xMin + (menuSize.x * 0.5f) + ScaleValue(12f), rootBounds.xMax - (menuSize.x * 0.5f) - ScaleValue(12f));
            position.y = Mathf.Clamp(position.y, rootBounds.yMin + (menuSize.y * 0.5f) + ScaleValue(12f), rootBounds.yMax - (menuSize.y * 0.5f) - ScaleValue(12f));
            return position;
        }

        private void OpenContextCardDetails(ScienceCardData card)
        {
            CloseHandCardContextMenu();
            OpenCardDetailsModal(card);
        }

        private void ChooseContextCard(ScienceCardData card)
        {
            CloseHandCardContextMenu();
            SelectCardForTurn(card);
        }

        private void CloseHandCardContextMenu()
        {
            if (handCardContextMenu == null) return;
            handCardContextMenu.SetActive(false);
            UnityEngine.Object.Destroy(handCardContextMenu);
            handCardContextMenu = null;
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
                AddLog("Esta carta não pode ser escolhida agora.");
                return;
            }

            if (card is ScienceActionCardData actionCard)
            {
                AddLog($"{currentPlayer.DisplayName} selecionou a ação {actionCard.DisplayName}. Revise o efeito antes de resolver.");
                BuildGameplayScreen();
                return;
            }

            AddLog($"{currentPlayer.DisplayName} selecionou {card.DisplayName}. Escolha um espaço livre no tabuleiro.");
            BuildGameplayScreen();
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

            string validationMessage = boardManager?.GetPlacementValidationMessage(coordinate, turnManager.SelectedCard, turnManager.SelectedRotationDegrees);

            if (!turnManager.SelectBoardSlot(coordinate))
            {
                AddLog("Selecione uma carta de personagem antes de escolher o tabuleiro.");
                BuildGameplayScreen();
                return;
            }

            if (string.IsNullOrEmpty(validationMessage))
            {
                AddLog($"Posição {FormatBoardCoordinate(coordinate)} válida. Confirme para colocar a carta.");
            }
            else
            {
                AddLog($"Prévia em {FormatBoardCoordinate(coordinate)} inválida: {validationMessage}");
            }

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
            string validationMessage = boardManager?.GetPlacementValidationMessage(coordinate, selectedCard, turnManager.SelectedRotationDegrees);
            bool overrideValidation = state != null && state.DebugOverridePlacementValidation;
            if (!string.IsNullOrEmpty(validationMessage) && !overrideValidation)
            {
                AddLog($"Não foi possível colocar a carta em {FormatBoardCoordinate(coordinate)}: {validationMessage}");
                BuildGameplayScreen();
                return;
            }

            if (boardManager == null || !boardManager.TryPlaceCard(coordinate, selectedCard, turnManager.SelectedRotationDegrees, overrideValidation))
            {
                AddLog($"Não foi possível colocar a carta em {FormatBoardCoordinate(coordinate)}: {validationMessage ?? "posição inválida"}");
                BuildGameplayScreen();
                return;
            }

            if (currentPlayer.InterdisciplinaryLeapAvailable)
            {
                currentPlayer.SetInterdisciplinaryLeapAvailable(false);
                AddLog("Interdisciplinary Leap usado: esta colocação pode defender uma conexão entre áreas sem automatizar a validação de cores/categorias.");
                telemetry?.LogEvent("science_action_modifier_consumed", $"player={currentPlayer.PlayerIndex};effect={ScienceActionEffectType.InterdisciplinaryLeap};card={selectedCard.Id};coord={coordinate}");
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

            CreateText(parent, "Votação da conexão", 44, new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.96f), FontStyles.Bold);
            string votePrompt = $"{activePlayer?.DisplayName ?? "Jogador ativo"} explicou {cardName}.\nOs jogadores aceitam essa conexão?";
            if (state != null && state.PeerReviewRequiresUnanimity)
            {
                votePrompt += "\nPeer Review ativo: todos precisam votar Sim.";
            }

            CreateText(parent, votePrompt, 28, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.82f), FontStyles.Normal);

            if (state == null || state.Players.Count <= 1)
            {
                CreateButton(parent, "Aceitar sem votação", new Vector2(0.50f, 0.48f), ResolveConnectionVoteResult, new Vector2(420f, 96f));
                return;
            }

            int row = 0;
            for (int i = 0; i < state.Players.Count; i++)
            {
                SciencePlayerState player = state.Players[i];
                if (player == null || player.PlayerIndex == activeVotingPlayerIndex) continue;

                float y = 0.55f - (row * 0.16f);
                if (connectionVotes.TryGetValue(player.PlayerIndex, out bool accepted))
                {
                    CreateText(parent, $"{player.DisplayName}: {(accepted ? "Sim" : "Não")}", 26, new Vector2(0.12f, y - 0.045f), new Vector2(0.88f, y + 0.045f), FontStyles.Bold);
                }
                else
                {
                    int voterIndex = player.PlayerIndex;
                    CreateText(parent, player.DisplayName, 24, new Vector2(0.10f, y - 0.055f), new Vector2(0.36f, y + 0.055f), FontStyles.Normal, TextAlignmentOptions.Left);
                    CreateButton(parent, "Sim", new Vector2(0.58f, y), () => SubmitConnectionVote(voterIndex, true), new Vector2(180f, 82f));
                    CreateButton(parent, "Não", new Vector2(0.78f, y), () => SubmitConnectionVote(voterIndex, false), new Vector2(180f, 82f));
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

            bool peerReviewActive = state != null && state.PeerReviewRequiresUnanimity;
            bool tied = yesVotes == noVotes;
            bool tieAccepted = state?.AcceptTiedConnectionVotes ?? true;
            bool acceptedByVote = peerReviewActive ? noVotes == 0 : yesVotes > noVotes || (tied && tieAccepted);
            string resultText = acceptedByVote ? "aceita" : "rejeitada";
            string tieText = tied && !peerReviewActive ? $" Empate: {(tieAccepted ? "aceito" : "rejeitado")} pela configuração atual." : string.Empty;
            string peerReviewText = peerReviewActive ? " Peer Review exigiu unanimidade." : string.Empty;

            AddLog($"Conexão {resultText}: {yesVotes} Sim / {noVotes} Não.{tieText}{peerReviewText}");
            telemetry?.LogEvent("science_connection_vote_resolved", $"turn={turnManager?.TurnNumber ?? 0};activePlayer={activeVotingPlayerIndex};card={activeVotingCardId};yes={yesVotes};no={noVotes};tied={tied};tieAccepted={tieAccepted};peerReview={peerReviewActive};accepted={acceptedByVote}");
            if (peerReviewActive)
            {
                state?.SetPeerReviewRequiresUnanimity(false);
                telemetry?.LogEvent("science_action_modifier_cleared", $"effect={ScienceActionEffectType.PeerReview};turn={turnManager?.TurnNumber ?? 0};card={activeVotingCardId}");
            }

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
            CreateText(parent, "Pontuação da conexão", 44, new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.96f), FontStyles.Bold);
            string scoringDescription = $"{GetPlayerDisplayName(activeScoringPlayerIndex)} teve a conexão aceita.\nO grupo decide manualmente os bônus.";
            if (HasCitationNeededBonus(activeScoringPlayerIndex))
            {
                scoringDescription += "\nCitation Needed ativo: considere o bônus de guia/fato (+1).";
            }

            CreateText(parent, scoringDescription, 28, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.82f), FontStyles.Normal);

            if (!basePointAwarded)
            {
                CreateButton(parent, "Ponto base +1", new Vector2(0.50f, 0.54f), AwardBaseConnectionPoint, new Vector2(420f, 82f));
            }
            else
            {
                CreateText(parent, "✓ Ponto base concedido (+1).", 26, new Vector2(0.12f, 0.49f), new Vector2(0.88f, 0.58f), FontStyles.Bold);
            }

            if (!interestingBonusAwarded)
            {
                CreateButton(parent, "Bônus explicação +1", new Vector2(0.50f, 0.40f), AwardInterestingBonus, new Vector2(420f, 78f));
            }
            else
            {
                CreateText(parent, "✓ Bônus de explicação interessante (+1).", 24, new Vector2(0.12f, 0.35f), new Vector2(0.88f, 0.44f), FontStyles.Bold);
            }

            if (!guideFactBonusAwarded)
            {
                CreateButton(parent, "Bônus guia/fato +1", new Vector2(0.50f, 0.27f), AwardGuideFactBonus, new Vector2(420f, 78f));
            }
            else
            {
                CreateText(parent, "✓ Bônus de fato/guia específico (+1).", 24, new Vector2(0.12f, 0.22f), new Vector2(0.88f, 0.31f), FontStyles.Bold);
            }

            CreateButton(parent, "Continuar", new Vector2(0.50f, 0.12f), ContinueAfterScoring, new Vector2(360f, 82f));
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
            ClearCitationNeededBonus(activeScoringPlayerIndex, "guide_fact_bonus_awarded");
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
            ClearCitationNeededBonus(activeScoringPlayerIndex, "accepted_connection_scoring_complete");
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
            if (currentPlayer == null || actionCard == null) return;

            ApplyActionEffect(currentPlayer, actionCard);
            currentPlayer.MarkPlayed(actionCard);
            deckManager?.Discard(actionCard);
            AddLog($"{currentPlayer.DisplayName} usou {actionCard.DisplayName}; a carta foi descartada.");
            telemetry?.LogEvent("science_action_card_resolved", $"turn={turnManager?.TurnNumber ?? 0};player={currentPlayer.PlayerIndex};card={actionCard.Id};effect={actionCard.EffectType}");
            if (TryShowEndGameIfNeeded(ScienceWinCondition.EmptyHand))
            {
                return;
            }

            turnManager?.MarkTurnResolved();
            BuildGameplayScreen();
        }

        private void BuildActionResolutionPanel(RectTransform parent)
        {
            SciencePlayerState currentPlayer = GetCurrentPlayer();
            ScienceActionCardData actionCard = turnManager?.SelectedCard as ScienceActionCardData;
            if (currentPlayer == null || actionCard == null)
            {
                CreateText(parent, "Nenhuma ação selecionada.", 30, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.58f), FontStyles.Italic);
                CreateButton(parent, "Voltar", new Vector2(0.50f, 0.24f), CancelSelection, new Vector2(320f, 82f));
                return;
            }

            CreateText(parent, "Resolver ação", 44, new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.96f), FontStyles.Bold);
            string body = $"{actionCard.DisplayName} [{actionCard.EffectType}]\n\n{actionCard.RulesText}\n\nEfeito temporário: {BuildActionEffectSummary(actionCard.EffectType)}";
            CreateText(parent, body, 28, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.80f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
            CreateButton(parent, "Aplicar ação", new Vector2(0.38f, 0.16f), () => ResolveActionCard(currentPlayer, actionCard), new Vector2(360f, 88f));
            CreateButton(parent, "Cancelar", new Vector2(0.64f, 0.16f), CancelSelection, new Vector2(320f, 88f));
        }

        private void ApplyActionEffect(SciencePlayerState currentPlayer, ScienceActionCardData actionCard)
        {
            switch (actionCard.EffectType)
            {
                case ScienceActionEffectType.PeerReview:
                    state?.SetPeerReviewRequiresUnanimity(true);
                    AddLog("Peer Review ativo: a próxima votação de conexão precisa de unanimidade.");
                    break;
                case ScienceActionEffectType.CitationNeeded:
                    currentPlayer.SetCitationNeededBonusAvailable(true);
                    AddLog($"Citation Needed ativo para {currentPlayer.DisplayName}: a próxima conexão aceita destacará o bônus de guia/fato.");
                    break;
                case ScienceActionEffectType.InterdisciplinaryLeap:
                    currentPlayer.SetInterdisciplinaryLeapAvailable(true);
                    AddLog($"Interdisciplinary Leap ativo para {currentPlayer.DisplayName}: a próxima colocação pode defender uma ligação entre áreas.");
                    break;
            }

            telemetry?.LogEvent("science_action_card_used", $"turn={turnManager?.TurnNumber ?? 0};player={currentPlayer.PlayerIndex};card={actionCard.Id};effect={actionCard.EffectType}");
        }

        private static string BuildActionEffectSummary(ScienceActionEffectType effectType)
        {
            switch (effectType)
            {
                case ScienceActionEffectType.PeerReview:
                    return "a próxima votação exige unanimidade.";
                case ScienceActionEffectType.CitationNeeded:
                    return "o jogador recebe um lembrete de bônus de guia/fato na próxima conexão aceita.";
                case ScienceActionEffectType.InterdisciplinaryLeap:
                    return "a próxima colocação de personagem pode propor uma conexão interdisciplinar ousada.";
                default:
                    return "efeito de teste sem resolução adicional.";
            }
        }

        private bool HasCitationNeededBonus(int playerIndex)
        {
            return GetPlayerByIndex(playerIndex)?.CitationNeededBonusAvailable ?? false;
        }

        private void ClearCitationNeededBonus(int playerIndex, string reason)
        {
            SciencePlayerState player = GetPlayerByIndex(playerIndex);
            if (player == null || !player.CitationNeededBonusAvailable) return;

            player.SetCitationNeededBonusAvailable(false);
            telemetry?.LogEvent("science_action_modifier_cleared", $"player={playerIndex};effect={ScienceActionEffectType.CitationNeeded};reason={reason}");
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

        private void OpenDebugLogModal()
        {
            if (root == null) return;

            CloseDebugLogModal();
            CloseCardDetailsModal();
            CloseHandCardContextMenu();
            RectTransform parent = root.GetComponent<RectTransform>();
            debugLogModal = new GameObject("DebugLogModal", typeof(RectTransform), typeof(Image));
            RectTransform overlay = debugLogModal.GetComponent<RectTransform>();
            overlay.SetParent(parent, false);
            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;

            Image overlayImage = debugLogModal.GetComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.62f);
            overlayImage.raycastTarget = true;

            RectTransform panel = CreatePanel(overlay, "DebugLogPanel", new Vector2(0.20f, 0.14f), new Vector2(0.80f, 0.86f), new Color(0.08f, 0.10f, 0.14f, 0.98f));
            CreateText(panel, "Debug Log", 36, new Vector2(0.06f, 0.88f), new Vector2(0.94f, 0.97f), FontStyles.Bold);
            CreateText(panel, BuildRecentActionLog(), 24, new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.84f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
            CreateButton(panel, "Close", new Vector2(0.50f, 0.10f), CloseDebugLogModal, new Vector2(240f, 76f));
        }

        private void CloseDebugLogModal()
        {
            if (debugLogModal == null) return;
            debugLogModal.SetActive(false);
            UnityEngine.Object.Destroy(debugLogModal);
            debugLogModal = null;
        }

        private void OpenCardDetailsModal(ScienceCardData card)
        {
            if (card == null || root == null) return;

            CloseCardDetailsModal();
            CloseDebugLogModal();
            CloseHandCardContextMenu();
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

            RectTransform overlay = CreatePanel(screen, "EndGameModalOverlay", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.68f));
            RectTransform panel = CreatePanel(overlay, "ScienceCardGameEndGameModal", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f), new Color(0.08f, 0.10f, 0.14f, 0.98f));
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
                    return "Leia o texto da ação e aplique seu efeito temporário.";
                case ScienceTurnStep.TurnResolved:
                    return "Turno resolvido. Encerre para passar ao próximo jogador.";
                default:
                    return "Selecione uma carta da mão atual.";
            }
        }

        private bool IsPlacementStep()
        {
            ScienceTurnStep step = turnManager?.CurrentStep ?? ScienceTurnStep.AwaitingCardSelection;
            return step == ScienceTurnStep.AwaitingBoardSlot || step == ScienceTurnStep.AwaitingPlacementConfirmation;
        }

        private bool IsSelectedPlacementValid()
        {
            if (turnManager == null || !turnManager.HasSelectedBoardCoordinate || boardManager == null) return false;
            return boardManager.CanPlaceCardAt(turnManager.SelectedBoardCoordinate, turnManager.SelectedCard, turnManager.SelectedRotationDegrees);
        }

        private bool CanConfirmSelectedPlacement()
        {
            if (turnManager == null || !turnManager.HasSelectedBoardCoordinate) return false;
            return IsSelectedPlacementValid() || (state != null && state.DebugOverridePlacementValidation);
        }

        private string BuildPlacementValidityText(bool hasSlot, bool isValid)
        {
            if (!hasSlot) return "Escolha uma casa adjacente.";

            SciencePlacementValidationResult result = boardManager?.ValidatePlacement(turnManager.SelectedBoardCoordinate, turnManager.SelectedCard, turnManager.SelectedRotationDegrees);
            string reason = result?.ReasonText;
            if (isValid) return string.IsNullOrEmpty(reason) ? "Conexão válida." : reason;

            if (state != null && state.DebugOverridePlacementValidation)
            {
                return $"⚠ Debug permite confirmar: {reason}";
            }

            return string.IsNullOrEmpty(reason) ? "Inválido: revise o slot escolhido." : reason;
        }

        private static bool IsDualColorCharacterCard(ScienceCardData card)
        {
            return card is ScienceCharacterCardData characterCard && characterCard.FactCategoryA != characterCard.FactCategoryB;
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
                case ScienceTurnStep.ActionResolution:
                    return "Ações criam modificadores simples de teste e são descartadas após aplicar.";
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
            SciencePlayerState player = GetPlayerByIndex(playerIndex);
            return player != null ? player.DisplayName : $"Player {playerIndex + 1}";
        }

        private SciencePlayerState GetPlayerByIndex(int playerIndex)
        {
            if (state != null)
            {
                for (int i = 0; i < state.Players.Count; i++)
                {
                    SciencePlayerState player = state.Players[i];
                    if (player != null && player.PlayerIndex == playerIndex) return player;
                }
            }

            return null;
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

        private static Vector2 GetHandCardSize()
        {
            return ScaleVector(new Vector2(230f, 188f));
        }

        private static float GetUiScale()
        {
            float widthScale = Mathf.Max(Screen.width, 1) / ReferenceLandscapeWidth;
            float heightScale = Mathf.Max(Screen.height, 1) / ReferenceLandscapeHeight;
            return Mathf.Clamp(Mathf.Min(widthScale, heightScale), 0.85f, 1.25f);
        }

        private static float ScaleValue(float value)
        {
            return value * GetUiScale();
        }

        private static Vector2 ScaleVector(Vector2 value)
        {
            return value * GetUiScale();
        }

        private static int ScaleFont(int size)
        {
            return Mathf.RoundToInt(size * GetUiScale());
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
            text.fontSize = ScaleFont(size);
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            return CreateButton(parent, label, anchor, onClick, new Vector2(720f, 112f));
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick, Vector2 size, bool interactable = true)
        {
            GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            Vector2 scaledSize = ScaleVector(size);
            rect.sizeDelta = scaledSize;

            Image image = buttonObject.GetComponent<Image>();
            image.color = interactable ? new Color(0.18f, 0.45f, 0.82f, 1f) : new Color(0.22f, 0.25f, 0.30f, 0.82f);

            Button button = buttonObject.GetComponent<Button>();
            button.interactable = interactable;
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(buttonObject.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = ScaleFont(scaledSize.x < ScaleValue(300f) ? 24 : 36);
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = interactable ? Color.white : new Color(0.72f, 0.75f, 0.78f, 1f);
            text.raycastTarget = false;
            return button;
        }
    }
}
