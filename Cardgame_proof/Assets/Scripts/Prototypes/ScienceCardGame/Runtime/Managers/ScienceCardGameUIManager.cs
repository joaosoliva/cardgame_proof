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
        private PrototypeRuntimeContext context;
        private ScienceCardGameState state;
        private ScienceDeckManager deckManager;
        private ScienceBoardManager boardManager;
        private ScienceTurnManager turnManager;
        private ScienceTelemetryManager telemetry;
        private GameObject root;
        private GameObject cardDetailModal;
        private TextMeshProUGUI selectedPlayerCountText;
        private int selectedPlayerCount = 2;

        public GameObject Root => root;

        public void Initialize(
            PrototypeRuntimeContext runtimeContext,
            ScienceCardGameState gameState,
            ScienceDeckManager scienceDeckManager,
            ScienceBoardManager scienceBoardManager,
            ScienceTurnManager scienceTurnManager,
            ScienceTelemetryManager telemetryManager,
            Action<int> onStartGame)
        {
            context = runtimeContext;
            state = gameState;
            deckManager = scienceDeckManager;
            boardManager = scienceBoardManager;
            turnManager = scienceTurnManager;
            telemetry = telemetryManager;
            selectedPlayerCount = state?.SelectedPlayerCount ?? 2;
            recentActions.Clear();

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
            selectedPlayerCountText = null;
            context = null;
            state = null;
            deckManager = null;
            boardManager = null;
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
            RectTransform screen = root.GetComponent<RectTransform>();
            ClearChildren(root.transform);

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
            CreateText(parent, $"Jogador atual: {GetCurrentPlayerName()}  |  Turno {turnManager?.TurnNumber ?? 0}", 26, new Vector2(0.33f, 0.10f), new Vector2(0.58f, 0.90f), FontStyles.Bold);
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
            CreateText(parent, "Área Central do Tabuleiro", 28, new Vector2(0.04f, 0.91f), new Vector2(0.96f, 0.98f), FontStyles.Bold);
            CreateText(parent, "Posicionamento de cartas será implementado na próxima etapa.", 18, new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.90f), FontStyles.Italic);

            RectTransform grid = CreatePanel(parent, "BoardGrid", new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.82f), new Color(0.06f, 0.12f, 0.10f, 0.92f));
            int columns = Mathf.Max(1, state?.BoardSize.x ?? 5);
            int rows = Mathf.Max(1, state?.BoardSize.y ?? 3);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    float minX = x / (float)columns;
                    float maxX = (x + 1) / (float)columns;
                    float minY = 1f - ((y + 1) / (float)rows);
                    float maxY = 1f - (y / (float)rows);
                    RectTransform slot = CreatePanel(grid, $"BoardSlot_{x}_{y}", new Vector2(minX, minY), new Vector2(maxX, maxY), new Color(0.15f, 0.28f, 0.23f, 0.82f));
                    slot.offsetMin = new Vector2(slot.offsetMin.x + 6f, slot.offsetMin.y + 6f);
                    slot.offsetMax = new Vector2(slot.offsetMax.x - 6f, slot.offsetMax.y - 6f);
                    ScienceCardData boardCard = GetBoardCardAt(new Vector2Int(x, y));
                    if (boardCard != null)
                    {
                        ScienceCardView boardCardView = ScienceCardView.Create(slot, $"BoardCard_{x}_{y}", boardCard, ScienceCardViewDisplayMode.Board, OpenCardDetailsModal);
                        RectTransform boardCardRect = boardCardView.GetComponent<RectTransform>();
                        boardCardRect.anchorMin = new Vector2(0.5f, 0.5f);
                        boardCardRect.anchorMax = new Vector2(0.5f, 0.5f);
                        boardCardRect.anchoredPosition = Vector2.zero;
                    }
                    else
                    {
                        CreateText(slot, $"{x + 1},{y + 1}", 18, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f), FontStyles.Normal);
                    }
                }
            }
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
            CreateText(parent, "Ações", 27, new Vector2(0.08f, 0.91f), new Vector2(0.92f, 0.98f), FontStyles.Bold);
            CreateText(parent, $"Deck: {deckManager?.DrawPile?.Count ?? 0}\nDescarte: {deckManager?.DiscardPile?.Count ?? 0}", 21, new Vector2(0.10f, 0.78f), new Vector2(0.90f, 0.89f), FontStyles.Normal);

            CreateButton(parent, "Comprar carta", new Vector2(0.50f, 0.66f), DrawCardForCurrentPlayer, new Vector2(260f, 62f));
            CreateButton(parent, "Registrar conexão", new Vector2(0.50f, 0.55f), () => AddPlaceholderAction("Conexões entre cartas ainda serão implementadas."), new Vector2(260f, 62f));
            CreateButton(parent, "Encerrar ação", new Vector2(0.50f, 0.44f), () => AddPlaceholderAction("Ação encerrada sem alteração de estado."), new Vector2(260f, 62f));
            CreateButton(parent, "Próximo turno", new Vector2(0.50f, 0.30f), AdvanceTurn, new Vector2(260f, 72f));

            CreateText(parent, "Os botões já atualizam o layout e o log, mas as regras completas de mesa ainda não foram implementadas.", 18, new Vector2(0.10f, 0.06f), new Vector2(0.90f, 0.22f), FontStyles.Italic, TextAlignmentOptions.Top);
        }

        private RectTransform CreateCardView(RectTransform parent, ScienceCardData card, string name)
        {
            ScienceCardView view = ScienceCardView.Create(parent, name, card, ScienceCardViewDisplayMode.Hand, OpenCardDetailsModal);
            return view.GetComponent<RectTransform>();
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

        private void AdvanceTurn()
        {
            turnManager?.AdvanceTurn();
            AddLog($"Turno {turnManager?.TurnNumber ?? 0}: vez de {GetCurrentPlayerName()}.");
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
