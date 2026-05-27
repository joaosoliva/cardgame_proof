using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CardgameProof.Bootstrap;

namespace CardgameProof.Core
{
    public sealed class GameController : MonoBehaviour
    {
        private static readonly IReadOnlyList<TutorialStep> DefaultTutorialSteps = new List<TutorialStep>
        {
            new TutorialStep { Id = "welcome_archive", Title = "Bem-vindo ao Arquivo", Body = "Neste jogo, vocês investigam personagens importantes da ciência e da academia. O objetivo é descobrir quem está escondido no arquivo do outro jogador.", Phase = GamePhase.TutorialIntro, TargetKey = "main_menu", OnlyShowOnce = true },
            new TutorialStep { Id = "build_archive", Title = "Monte seu arquivo", Body = "Arraste suas cartas para a grade. Personagens e cartas de arquivo ficarão escondidos do adversário.", Phase = GamePhase.Setup, TargetKey = "board_grid", OnlyShowOnce = true },
            new TutorialStep { Id = "rotate_reposition", Title = "Girar e reposicionar", Body = "Toque em uma carta posicionada para girar, remover ou ajustar sua posição antes de confirmar.", Phase = GamePhase.Setup, TargetKey = "placed_card_actions", OnlyShowOnce = true }
        };

        private const bool ContinueTurnAfterArchiveCard = false; // tune prototype behavior here

        private SceneRootBuilder sceneRoot;
        private TutorialOverlayView tutorialOverlay;
        private ReadyScreenView readyScreenView;
        private BoardController boardController;

        private RectTransform trayRoot;
        private RectTransform placedActionsRoot;
        private Button finalizeSetupButton;
        private readonly List<SetupCardView> trayCards = new List<SetupCardView>();
        private Vector2Int? selectedPlacedCoordinate;

        private readonly Dictionary<PlayerId, List<PlacedCardData>> playerBoardStates = new Dictionary<PlayerId, List<PlacedCardData>>();
        private readonly Dictionary<PlayerId, HashSet<string>> identifiedCharacters = new Dictionary<PlayerId, HashSet<string>>();
        private readonly Dictionary<PlayerId, int> scores = new Dictionary<PlayerId, int>();
        private readonly Dictionary<PlayerId, int> researchTokens = new Dictionary<PlayerId, int>();
        private readonly Dictionary<PlayerId, Dictionary<string, HashSet<ClueCategory>>> discoveredClues = new Dictionary<PlayerId, Dictionary<string, HashSet<ClueCategory>>>();
        private readonly Dictionary<PlayerId, HashSet<string>> blockedCharacterGuesses = new Dictionary<PlayerId, HashSet<string>>();

        private PlayerId currentSetupPlayer = PlayerId.PlayerOne;
        private PlayerId currentTurnPlayer = PlayerId.PlayerOne;

        private RectTransform hudRoot;
        private Text hudCurrentPlayer;
        private Text hudObjective;
        private Text hudScore;
        private Text hudResearch;

        public GameModeConfig ActiveModeConfig { get; private set; }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.MainMenu;

        public void InitializeMainMenu(SceneRootBuilder builtSceneRoot)
        {
            if (builtSceneRoot == null || builtSceneRoot.FullScreenRoot == null) return;
            sceneRoot = builtSceneRoot;

            EnsureTutorialOverlay();
            EnsureReadyScreenView();
            EnsureBoardController();

            RectTransform fullRoot = sceneRoot.FullScreenRoot;
            if (fullRoot.Find("MainMenuRoot") != null) return;

            GameObject menuRootObject = new GameObject("MainMenuRoot", typeof(RectTransform), typeof(Image));
            RectTransform menuRoot = menuRootObject.GetComponent<RectTransform>();
            menuRoot.SetParent(fullRoot, false);
            menuRoot.anchorMin = Vector2.zero; menuRoot.anchorMax = Vector2.one;
            menuRoot.offsetMin = Vector2.zero; menuRoot.offsetMax = Vector2.zero;
            menuRoot.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 1f);

            VerticalLayoutGroup layout = menuRootObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(56, 56, 120, 80);
            layout.spacing = 28f; layout.childControlWidth = true;

            CreateTitle(menuRoot, "Nosso jogo, diversão ilimitada");
            CreateModeButton(menuRoot, "Partida Rápida — 5 min", "5min");
            CreateModeButton(menuRoot, "Partida Completa — 10 min", "10min");
            CreateFooter(menuRoot, "Protótipo digital para teste de jogo físico");
            CurrentPhase = GamePhase.MainMenu;
        }

        private void CreateModeButton(RectTransform parent, string label, string modeId)
        {
            GameObject buttonObj = new GameObject($"{modeId}_Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObj.transform.SetParent(parent, false);
            buttonObj.GetComponent<LayoutElement>().preferredHeight = 180f;
            buttonObj.GetComponent<Image>().color = new Color(0.19f, 0.46f, 0.88f, 1f);
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnModeSelected(modeId));

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            RectTransform lr = labelObj.GetComponent<RectTransform>(); lr.SetParent(buttonObj.transform, false); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            Text t = labelObj.GetComponent<Text>(); t.text = label; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = 52; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
        }

        private void OnModeSelected(string modeId)
        {
            LoadPrototypeMode(modeId);
            if (ActiveModeConfig == null) return;
            TransitionToTutorialIntro();
            StartPassAndPlaySetup();
            ShowTutorialSequence(DefaultTutorialSteps);
        }

        public void LoadPrototypeMode(string modeId)
        {
            ActiveModeConfig = PrototypeDatabase.GetMode(modeId);
        }

        public void ShowTutorialSequence(IReadOnlyList<TutorialStep> sequence)
        {
            EnsureTutorialOverlay();
            tutorialOverlay?.ShowSequence(sequence);
        }

        private void TransitionToTutorialIntro() => CurrentPhase = GamePhase.TutorialIntro;

        private void StartPassAndPlaySetup()
        {
            playerBoardStates[PlayerId.PlayerOne] = new List<PlacedCardData>();
            playerBoardStates[PlayerId.PlayerTwo] = new List<PlacedCardData>();
            BeginSetupForPlayer(PlayerId.PlayerOne);
        }

        private void BeginSetupForPlayer(PlayerId player)
        {
            currentSetupPlayer = player;
            CurrentPhase = GamePhase.Setup;
            BuildBoardForActiveMode();
            BuildBottomTray();
            BuildPlacedCardActions();
            GenerateCurrentPlayerSetupCards(player);
            UpdateFinalizeButtonState();
            HideReadyScreen();
        }

        private void BuildBoardForActiveMode()
        {
            EnsureBoardController();
            boardController.ClearBoard();
            boardController.BuildBoard(sceneRoot.CenterBoardArea, ActiveModeConfig.BoardSize, null);
            boardController.OnPlacedCardTapped = OnPlacedCardTapped;
            boardController.RefreshVisualsForPhase(CurrentPhase);
        }

        private void BuildBottomTray()
        {
            if (trayRoot != null) Destroy(trayRoot.gameObject);
            GameObject tray = new GameObject("SetupBottomTray", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            trayRoot = tray.GetComponent<RectTransform>();
            trayRoot.SetParent(sceneRoot.BottomCardTray, false);
            trayRoot.anchorMin = Vector2.zero; trayRoot.anchorMax = Vector2.one;
            trayRoot.offsetMin = new Vector2(20f, 20f); trayRoot.offsetMax = new Vector2(-20f, -20f);
            tray.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.18f);
            HorizontalLayoutGroup h = tray.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 16f; h.childForceExpandWidth = true; h.padding = new RectOffset(16, 16, 16, 16);
        }

        private void BuildPlacedCardActions()
        {
            if (placedActionsRoot != null) Destroy(placedActionsRoot.gameObject);
            GameObject root = new GameObject("PlacedCardActions", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            placedActionsRoot = root.GetComponent<RectTransform>();
            placedActionsRoot.SetParent(sceneRoot.ActionArea, false);
            placedActionsRoot.anchorMin = Vector2.zero; placedActionsRoot.anchorMax = Vector2.one;
            placedActionsRoot.offsetMin = new Vector2(20f, 20f); placedActionsRoot.offsetMax = new Vector2(-20f, -20f);
            HorizontalLayoutGroup h = root.GetComponent<HorizontalLayoutGroup>(); h.spacing = 16f; h.childForceExpandWidth = true;

            CreateActionButton(placedActionsRoot, "Girar", () => { if (selectedPlacedCoordinate.HasValue) boardController.RotateCard(selectedPlacedCoordinate.Value); });
            CreateActionButton(placedActionsRoot, "Remover", OnRemoveSelectedPlacedCard);
            CreateActionButton(placedActionsRoot, "Confirmar", () => selectedPlacedCoordinate = null);
            finalizeSetupButton = CreateActionButton(placedActionsRoot, "Finalizar montagem", OnFinalizeSetupPressed);
        }

        private void GenerateCurrentPlayerSetupCards(PlayerId player)
        {
            foreach (var card in trayCards) if (card != null) Destroy(card.gameObject);
            trayCards.Clear();
            int total = ActiveModeConfig.CharactersPerPlayer + ActiveModeConfig.ArchiveCardsPerPlayer;
            for (int i = 0; i < total; i++)
            {
                CardType type = i < ActiveModeConfig.CharactersPerPlayer ? CardType.Character : CardType.Archive;
                var placed = new PlacedCardData { CardId = $"{player}_{type}_{i}", CardType = type, Owner = player, Coordinate = Vector2Int.zero, IsFaceUp = false };
                GameObject go = new GameObject($"SetupCard_{i}", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(CanvasGroup));
                go.transform.SetParent(trayRoot, false);
                SetupCardView view = go.AddComponent<SetupCardView>();
                view.Initialize(FindFirstObjectByType<Canvas>(), placed, OnSetupCardDrop);
                trayCards.Add(view);
            }
        }

        private void OnSetupCardDrop(SetupCardView cardView, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!boardController.TryGetCoordinateFromScreenPosition(eventData.position, out Vector2Int coord)) { cardView.ResetToTray(); return; }
            PlacedCardData data = cardView.CardData; data.Coordinate = coord;
            if (!boardController.PlaceCard(data)) { cardView.ResetToTray(); Debug.Log("Jogada inválida"); return; }
            cardView.gameObject.SetActive(false);
            StorePlacedCardForCurrentPlayer(data);
            UpdateFinalizeButtonState();
        }

        private void OnPlacedCardTapped(Vector2Int coordinate) => selectedPlacedCoordinate = coordinate;

        private void OnRemoveSelectedPlacedCard()
        {
            if (!selectedPlacedCoordinate.HasValue) return;
            PlacedCardData placed = boardController.GetPlacedCard(selectedPlacedCoordinate.Value);
            if (placed == null || !boardController.RemoveCard(selectedPlacedCoordinate.Value)) return;
            RemoveStoredPlacedCardForCurrentPlayer(placed.CardId);
            SetupCardView trayCard = trayCards.Find(c => c != null && c.CardData.CardId == placed.CardId);
            if (trayCard != null) trayCard.gameObject.SetActive(true);
            selectedPlacedCoordinate = null;
            UpdateFinalizeButtonState();
        }

        private void UpdateFinalizeButtonState()
        {
            int active = 0; foreach (var c in trayCards) if (c != null && c.gameObject.activeSelf) active++;
            finalizeSetupButton.interactable = active == 0;
        }

        private void OnFinalizeSetupPressed()
        {
            HideSetupSensitiveUi();
            if (currentSetupPlayer == PlayerId.PlayerOne)
            {
                ShowReadyScreen("Passe o aparelho para o Jogador 2", "Estou pronto", () => BeginSetupForPlayer(PlayerId.PlayerTwo));
                return;
            }

            ShowReadyScreen("Passe o aparelho para o Jogador 1", "Começar investigação", StartInvestigationPhase);
        }

        private void StartInvestigationPhase()
        {
            HideReadyScreen();
            CurrentPhase = GamePhase.Investigation;
            BuildInvestigationHud();

            scores[PlayerId.PlayerOne] = 0; scores[PlayerId.PlayerTwo] = 0;
            researchTokens[PlayerId.PlayerOne] = ActiveModeConfig.ResearchTokensPerPlayer;
            researchTokens[PlayerId.PlayerTwo] = ActiveModeConfig.ResearchTokensPerPlayer;
            identifiedCharacters[PlayerId.PlayerOne] = new HashSet<string>(); identifiedCharacters[PlayerId.PlayerTwo] = new HashSet<string>();
            discoveredClues[PlayerId.PlayerOne] = new Dictionary<string, HashSet<ClueCategory>>(); discoveredClues[PlayerId.PlayerTwo] = new Dictionary<string, HashSet<ClueCategory>>();
            blockedCharacterGuesses[PlayerId.PlayerOne] = new HashSet<string>(); blockedCharacterGuesses[PlayerId.PlayerTwo] = new HashSet<string>();

            currentTurnPlayer = PlayerId.PlayerOne;
            ShowOpponentBoardForCurrentTurn();
            UpdateHud();
        }

        private void ShowOpponentBoardForCurrentTurn()
        {
            PlayerId opponent = currentTurnPlayer == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne;
            boardController.ClearBoard();
            boardController.BuildBoard(sceneRoot.CenterBoardArea, ActiveModeConfig.BoardSize, null);
            boardController.OnPlacedCardTapped = OnInvestigationCellTapped;

            foreach (PlacedCardData card in playerBoardStates[opponent])
            {
                boardController.PlaceCard(new PlacedCardData { CardId = card.CardId, CardType = card.CardType, Owner = card.Owner, Coordinate = card.Coordinate, IsFaceUp = false });
            }
            boardController.RefreshVisualsForPhase(GamePhase.Investigation);
            sceneRoot.CenterBoardArea.localRotation = Quaternion.Euler(0f, 0f, currentTurnPlayer == PlayerId.PlayerOne ? 0f : 180f);
        }

        private void OnInvestigationCellTapped(Vector2Int coordinate)
        {
            PlacedCardData card = boardController.GetPlacedCard(coordinate);
            if (card == null || card.IsFaceUp) return;

            card.IsFaceUp = true;
            boardController.RemoveCard(coordinate);
            boardController.PlaceCard(card);

            if (card.CardType == CardType.Archive)
            {
                ResolveArchiveCard(card);
                if (!ContinueTurnAfterArchiveCard) EndTurnWithPassScreen();
                return;
            }

            ResolveCharacterCard(card);
        }

        private void ResolveArchiveCard(PlacedCardData card)
        {
            Debug.Log($"Arquivo revelado: {card.CardId}. Efeito simples aplicado.");
            researchTokens[currentTurnPlayer] = Mathf.Max(0, researchTokens[currentTurnPlayer] - 1);
            UpdateHud();
        }

        private void ResolveCharacterCard(PlacedCardData card)
        {
            string characterId = card.CardId;
            ClueCategory category = ClueCategory.Area;
            string clueText = GetClueText(characterId, category);
            if (!discoveredClues[currentTurnPlayer].ContainsKey(characterId)) discoveredClues[currentTurnPlayer][characterId] = new HashSet<ClueCategory>();
            discoveredClues[currentTurnPlayer][characterId].Add(category);
            blockedCharacterGuesses[currentTurnPlayer].Remove(characterId);
            Debug.Log($"Dossiê encontrado. Pista ({category}): {clueText}");

            bool guessed = TryPrototypeGuess(characterId);
            if (!guessed) EndTurnWithPassScreen();
        }

        private bool TryPrototypeGuess(string characterId)
        {
            bool correct = UnityEngine.Random.value > 0.5f;
            if (!correct)
            {
                blockedCharacterGuesses[currentTurnPlayer].Add(characterId);
                Debug.Log("Palpite incorreto. Descubra outra pista antes de tentar este personagem novamente.");
                return false;
            }

            scores[currentTurnPlayer] += 1;
            identifiedCharacters[currentTurnPlayer].Add(characterId);
            Debug.Log("Palpite correto! +1 ponto.");
            UpdateHud();

            if (scores[currentTurnPlayer] >= ActiveModeConfig.ObjectiveIdentifications)
            {
                Debug.Log($"Vitória de {currentTurnPlayer}!");
                return true;
            }

            return false;
        }

        private void EndTurnWithPassScreen()
        {
            PlayerId next = currentTurnPlayer == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne;
            ShowReadyScreen("Passe o aparelho para o próximo jogador", "Estou pronto", () =>
            {
                currentTurnPlayer = next;
                HideReadyScreen();
                ShowOpponentBoardForCurrentTurn();
                UpdateHud();
            });
        }

        private void BuildInvestigationHud()
        {
            if (hudRoot != null) Destroy(hudRoot.gameObject);
            GameObject hud = new GameObject("InvestigationHUD", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            hudRoot = hud.GetComponent<RectTransform>();
            hudRoot.SetParent(sceneRoot.TopArea, false);
            hudRoot.anchorMin = Vector2.zero; hudRoot.anchorMax = Vector2.one;
            hudRoot.offsetMin = new Vector2(8, 8); hudRoot.offsetMax = new Vector2(-8, -8);
            hud.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            VerticalLayoutGroup v = hud.GetComponent<VerticalLayoutGroup>(); v.spacing = 6f;

            hudCurrentPlayer = CreateHudText(hudRoot, "Jogador atual");
            hudObjective = CreateHudText(hudRoot, "Objetivo");
            hudScore = CreateHudText(hudRoot, "Placar");
            hudResearch = CreateHudText(hudRoot, "Fichas de pesquisa");

            RectTransform buttonsRow = new GameObject("HUDButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
            buttonsRow.SetParent(hudRoot, false);
            buttonsRow.GetComponent<HorizontalLayoutGroup>().spacing = 12f;
            CreateActionButton(buttonsRow, "Guia", () => Debug.Log("Abrir guia"));
            CreateActionButton(buttonsRow, "Regras", () => Debug.Log("Abrir regras"));
        }

        private void UpdateHud()
        {
            if (hudCurrentPlayer == null) return;
            hudCurrentPlayer.text = $"Jogador atual: {(currentTurnPlayer == PlayerId.PlayerOne ? "Jogador 1" : "Jogador 2")}";
            hudObjective.text = $"Objetivo: identificar {ActiveModeConfig.ObjectiveIdentifications}";
            hudScore.text = $"Placar J1 {scores[PlayerId.PlayerOne]} x {scores[PlayerId.PlayerTwo]} J2";
            hudResearch.text = $"Pesquisa restante: J1 {researchTokens[PlayerId.PlayerOne]} | J2 {researchTokens[PlayerId.PlayerTwo]}";
        }

        private string GetClueText(string characterId, ClueCategory category)
        {
            foreach (var c in PrototypeDatabase.Characters)
            {
                if (!characterId.Contains(c.Id, StringComparison.OrdinalIgnoreCase)) continue;
                return category switch
                {
                    ClueCategory.Area => c.Area,
                    ClueCategory.Era => c.Era,
                    ClueCategory.Region => c.Region,
                    ClueCategory.Contribution => c.Contribution,
                    _ => c.ContextOrLegacy
                };
            }
            return "Pista indisponível no protótipo.";
        }

        private static Text CreateHudText(RectTransform parent, string value)
        {
            GameObject go = new GameObject("HudText", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text t = go.GetComponent<Text>(); t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = 28; t.color = Color.white; t.text = value;
            return t;
        }

        private void HideSetupSensitiveUi()
        {
            if (trayRoot != null) trayRoot.gameObject.SetActive(false);
            if (placedActionsRoot != null) placedActionsRoot.gameObject.SetActive(false);
            CanvasGroup cg = sceneRoot.CenterBoardArea.GetComponent<CanvasGroup>(); if (cg == null) cg = sceneRoot.CenterBoardArea.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0.05f;
        }

        private void StorePlacedCardForCurrentPlayer(PlacedCardData placed)
        {
            playerBoardStates[currentSetupPlayer].RemoveAll(c => c.CardId == placed.CardId);
            playerBoardStates[currentSetupPlayer].Add(new PlacedCardData { CardId = placed.CardId, CardType = placed.CardType, Owner = placed.Owner, Coordinate = placed.Coordinate, IsFaceUp = placed.IsFaceUp });
        }

        private void RemoveStoredPlacedCardForCurrentPlayer(string cardId) => playerBoardStates[currentSetupPlayer].RemoveAll(c => c.CardId == cardId);

        private void EnsureBoardController()
        {
            if (boardController != null) return;
            GameObject boardObject = new GameObject("BoardController", typeof(RectTransform));
            boardObject.transform.SetParent(sceneRoot.CenterBoardArea, false);
            boardController = boardObject.AddComponent<BoardController>();
        }

        private void EnsureTutorialOverlay()
        {
            if (tutorialOverlay != null) return;
            GameObject overlayObject = new GameObject("TutorialOverlayView");
            overlayObject.transform.SetParent(sceneRoot.OverlayLayer, false);
            tutorialOverlay = overlayObject.AddComponent<TutorialOverlayView>();
            tutorialOverlay.Initialize(sceneRoot.OverlayLayer);
        }

        private void EnsureReadyScreenView()
        {
            if (readyScreenView != null) return;
            GameObject go = new GameObject("ReadyScreenView");
            go.transform.SetParent(sceneRoot.OverlayLayer, false);
            readyScreenView = go.AddComponent<ReadyScreenView>();
            readyScreenView.Initialize(sceneRoot.OverlayLayer);
        }

        private void ShowReadyScreen(string message, string buttonText, Action onConfirm)
        {
            EnsureReadyScreenView();
            readyScreenView.Show(message, buttonText, onConfirm);
        }

        private void HideReadyScreen() => readyScreenView?.Hide();

        private static Button CreateActionButton(RectTransform parent, string text, Action onClick)
        {
            GameObject go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 88f;
            go.GetComponent<Image>().color = new Color(0.16f, 0.43f, 0.84f, 1f);
            Button b = go.GetComponent<Button>();
            b.onClick.AddListener(() => onClick?.Invoke());

            GameObject label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            RectTransform lr = label.GetComponent<RectTransform>(); lr.SetParent(go.transform, false); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            Text t = label.GetComponent<Text>(); t.text = text; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.alignment = TextAnchor.MiddleCenter; t.fontSize = 28; t.color = Color.white;
            return b;
        }

        private static void CreateTitle(RectTransform parent, string textValue)
        {
            GameObject titleObj = new GameObject("MainMenuTitle", typeof(RectTransform), typeof(LayoutElement), typeof(Text));
            titleObj.transform.SetParent(parent, false);
            titleObj.GetComponent<LayoutElement>().preferredHeight = 280f;
            Text text = titleObj.GetComponent<Text>(); text.text = textValue; text.alignment = TextAnchor.MiddleCenter; text.fontSize = 82; text.color = Color.white; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void CreateFooter(RectTransform parent, string textValue)
        {
            GameObject spacer = new GameObject("FooterSpacer", typeof(RectTransform), typeof(LayoutElement)); spacer.transform.SetParent(parent, false); spacer.GetComponent<LayoutElement>().flexibleHeight = 1f;
            GameObject footerObj = new GameObject("MainMenuFooter", typeof(RectTransform), typeof(LayoutElement), typeof(Text)); footerObj.transform.SetParent(parent, false); footerObj.GetComponent<LayoutElement>().preferredHeight = 120f;
            Text text = footerObj.GetComponent<Text>(); text.text = textValue; text.alignment = TextAnchor.MiddleCenter; text.fontSize = 38; text.color = new Color(0.84f, 0.88f, 0.92f, 1f); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
