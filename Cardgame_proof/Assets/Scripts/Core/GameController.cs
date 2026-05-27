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

        private SceneRootBuilder sceneRoot;
        private TutorialOverlayView tutorialOverlay;
        private ReadyScreenView readyScreenView;
        private InvestigationOverlayView investigationOverlayView;
        private GuidebookOverlayView guidebookOverlayView;
        private WinScreenView winScreenView;
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

        private int totalCluesRequested;
        private int totalResearchUses;
        private int totalGuesses;

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
            EnsureInvestigationOverlayView();
            EnsureGuidebookOverlayView();
            EnsureWinScreenView();
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

        public void LoadPrototypeMode(string modeId) => ActiveModeConfig = PrototypeDatabase.GetMode(modeId);
        public void ShowTutorialSequence(IReadOnlyList<TutorialStep> sequence) { EnsureTutorialOverlay(); tutorialOverlay?.ShowSequence(sequence); }
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
            winScreenView?.Hide();
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
        private void UpdateFinalizeButtonState() { int active = 0; foreach (var c in trayCards) if (c != null && c.gameObject.activeSelf) active++; finalizeSetupButton.interactable = active == 0; }

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
            totalCluesRequested = 0; totalResearchUses = 0; totalGuesses = 0;
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
            foreach (PlacedCardData card in playerBoardStates[opponent]) boardController.PlaceCard(new PlacedCardData { CardId = card.CardId, CardType = card.CardType, Owner = card.Owner, Coordinate = card.Coordinate, IsFaceUp = false });
            boardController.RefreshVisualsForPhase(GamePhase.Investigation);
            sceneRoot.CenterBoardArea.localRotation = Quaternion.Euler(0f, 0f, currentTurnPlayer == PlayerId.PlayerOne ? 0f : 180f);
        }

        private void OnInvestigationCellTapped(Vector2Int coordinate)
        {
            if (CurrentPhase != GamePhase.Investigation) return;
            PlacedCardData card = boardController.GetPlacedCard(coordinate);
            if (card == null || card.IsFaceUp) return;
            if (card.CardType == CardType.Character && blockedCharacterGuesses[currentTurnPlayer].Contains(card.CardId)) return;
            card.IsFaceUp = true; boardController.RemoveCard(coordinate); boardController.PlaceCard(card);
            if (card.CardType == CardType.Archive) { EnsureInvestigationOverlayView(); ResolveArchiveCard(card); return; }
            ResolveCharacterCard(card);
        }

        private void ResolveArchiveCard(PlacedCardData card)
        {
            LogTelemetry("archive_card_found", $"card={card.CardId}");
            int effectType = ParseCardIndex(card.CardId) % 3;
            if (effectType == 0) ResolveArchiveLacunaDeArquivo(card.CardId);
            else if (effectType == 1) ResolveArchiveReferenciaCruzada(card);
            else ResolveArchiveFragmentoDocumento(card.CardId);
        }
        private void ResolveArchiveLacunaDeArquivo(string cardId) { researchTokens[currentTurnPlayer] += 1; UpdateHud(); investigationOverlayView.Show("Lacuna de Arquivo", "Você ganhou +1 Ficha de Pesquisa."); investigationOverlayView.AddButton("Continuar", EndTurnAfterOverlay); LogTelemetry("archive_effect_resolved", $"card={cardId};effect=lacuna_de_arquivo;result=token_plus_one"); }
        private void ResolveArchiveReferenciaCruzada(PlacedCardData archiveCard)
        {
            List<Vector2Int> candidates = GetAdjacentHiddenCoordinates(archiveCard.Coordinate);
            if (candidates.Count == 0) { ShowNoValidTargetArchiveEffect("referencia_cruzada", archiveCard.CardId); return; }
            investigationOverlayView.Show("Referência Cruzada", "Escolha uma célula adjacente para investigar o tipo da carta.");
            foreach (Vector2Int coord in candidates)
            {
                Vector2Int captured = coord;
                investigationOverlayView.AddButton($"Célula {captured.x},{captured.y}", () => { PlacedCardData adjacent = boardController.GetPlacedCard(captured); string result = adjacent != null && adjacent.CardType == CardType.Character ? "Personagem" : "Arquivo"; investigationOverlayView.Show("Referência Cruzada", $"A célula {captured.x},{captured.y} contém: {result}."); investigationOverlayView.AddButton("Continuar", EndTurnAfterOverlay); LogTelemetry("archive_effect_resolved", $"card={archiveCard.CardId};effect=referencia_cruzada;result={result}"); });
            }
        }
        private void ResolveArchiveFragmentoDocumento(string cardId)
        {
            List<string> targets = GetDiscoveredButUnidentifiedCharacters();
            if (targets.Count == 0) { ShowNoValidTargetArchiveEffect("fragmento_documento", cardId); return; }
            investigationOverlayView.Show("Fragmento de Documento", "Escolha um personagem já descoberto para revelar uma pista extra.");
            foreach (string characterId in targets)
            {
                string captured = characterId;
                CharacterData character = FindCharacterByCardId(characterId);
                string label = character != null ? character.DisplayName : characterId;
                investigationOverlayView.AddButton(label, () => { if (!TryRevealExtraClue(captured, out ClueCategory category, out string clueText)) { ShowNoValidTargetArchiveEffect("fragmento_documento", cardId); return; } investigationOverlayView.Show("Fragmento de Documento", $"{GetCategoryLabel(category)}\n\n{clueText}"); investigationOverlayView.AddButton("Continuar", EndTurnAfterOverlay); LogTelemetry("archive_effect_resolved", $"card={cardId};effect=fragmento_documento;target={captured};category={category}"); });
            }
        }

        private void ResolveCharacterCard(PlacedCardData card) => ShowClueSelectionOverlay(card.CardId);
        private void ShowClueSelectionOverlay(string characterId)
        {
            EnsureInvestigationOverlayView();
            HashSet<ClueCategory> knownClues = GetKnownClues(currentTurnPlayer, characterId);
            investigationOverlayView.Show("Escolha uma pista", "Selecione uma categoria para revelar uma nova pista.");
            AddClueCategoryButton(characterId, ClueCategory.Area, "Área", knownClues.Contains(ClueCategory.Area));
            AddClueCategoryButton(characterId, ClueCategory.Era, "Época", knownClues.Contains(ClueCategory.Era));
            AddClueCategoryButton(characterId, ClueCategory.Region, "Região", knownClues.Contains(ClueCategory.Region));
            AddClueCategoryButton(characterId, ClueCategory.Contribution, "Contribuição", knownClues.Contains(ClueCategory.Contribution));
            AddClueCategoryButton(characterId, ClueCategory.ContextLegacy, "Contexto/Legado", knownClues.Contains(ClueCategory.ContextLegacy));
        }
        private void AddClueCategoryButton(string characterId, ClueCategory category, string label, bool alreadyKnown) => investigationOverlayView.AddButton(alreadyKnown ? $"{label} (já conhecida)" : label, () => OnClueSelected(characterId, category), !alreadyKnown);
        private void OnClueSelected(string characterId, ClueCategory category)
        {
            GetKnownClues(currentTurnPlayer, characterId).Add(category);
            blockedCharacterGuesses[currentTurnPlayer].Remove(characterId);
            totalCluesRequested += 1;
            LogTelemetry("clue_requested", $"character={characterId};category={category}");
            investigationOverlayView.Show("Pista revelada", $"{GetCategoryLabel(category)}\n\n{GetClueText(characterId, category)}");
            investigationOverlayView.AddButton("Tentar identificar", () => ShowGuessOverlay(characterId));
            investigationOverlayView.AddButton("Encerrar turno", EndTurnAfterOverlay);
        }
        private void ShowGuessOverlay(string characterId)
        {
            investigationOverlayView.Show("Escolha a personagem", "Selecione um nome e confirme sua identificação.");
            foreach (CharacterData character in PrototypeDatabase.Characters) { string selectedName = character.DisplayName; investigationOverlayView.AddButton(selectedName, () => ResolveGuess(characterId, selectedName)); }
            investigationOverlayView.AddButton("Voltar", () => OnClueSelected(characterId, GetLastKnownClue(characterId)));
        }
        private void ResolveGuess(string characterId, string guessedName)
        {
            totalGuesses += 1;
            CharacterData target = FindCharacterByCardId(characterId);
            bool correct = target != null && string.Equals(target.DisplayName, guessedName, StringComparison.Ordinal);
            LogTelemetry("guess_made", $"character={characterId};guess={guessedName}");
            if (correct)
            {
                LogTelemetry("guess_correct", $"character={characterId};guess={guessedName}");
                scores[currentTurnPlayer] += 1;
                identifiedCharacters[currentTurnPlayer].Add(characterId);
                UpdateHud();
                if (scores[currentTurnPlayer] >= ActiveModeConfig.ObjectiveIdentifications) { CompleteMatch(currentTurnPlayer); return; }
                investigationOverlayView.Show("Resultado", "Identificação correta!"); investigationOverlayView.AddButton("Encerrar turno", EndTurnAfterOverlay); return;
            }
            LogTelemetry("guess_wrong", $"character={characterId};guess={guessedName}"); blockedCharacterGuesses[currentTurnPlayer].Add(characterId); investigationOverlayView.Show("Resultado", "Ainda não. Você precisa de mais uma pista antes de tentar novamente."); investigationOverlayView.AddButton("Encerrar turno", EndTurnAfterOverlay);
        }

        private void EndTurnAfterOverlay() { investigationOverlayView.Hide(); EndTurnWithPassScreen(); }
        private void EndTurnWithPassScreen() { PlayerId next = currentTurnPlayer == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne; ShowReadyScreen("Passe o aparelho para o próximo jogador", "Estou pronto", () => { currentTurnPlayer = next; HideReadyScreen(); ShowOpponentBoardForCurrentTurn(); UpdateHud(); }); }

        private void BuildInvestigationHud()
        {
            if (hudRoot != null) Destroy(hudRoot.gameObject);
            GameObject hud = new GameObject("InvestigationHUD", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            hudRoot = hud.GetComponent<RectTransform>(); hudRoot.SetParent(sceneRoot.TopArea, false); hudRoot.anchorMin = Vector2.zero; hudRoot.anchorMax = Vector2.one; hudRoot.offsetMin = new Vector2(8, 8); hudRoot.offsetMax = new Vector2(-8, -8);
            hud.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            VerticalLayoutGroup v = hud.GetComponent<VerticalLayoutGroup>(); v.spacing = 6f;
            hudCurrentPlayer = CreateHudText(hudRoot, "Jogador atual"); hudObjective = CreateHudText(hudRoot, "Objetivo"); hudScore = CreateHudText(hudRoot, "Placar"); hudResearch = CreateHudText(hudRoot, "Fichas de pesquisa");
            RectTransform buttonsRow = new GameObject("HUDButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
            buttonsRow.SetParent(hudRoot, false); buttonsRow.GetComponent<HorizontalLayoutGroup>().spacing = 12f;
            CreateActionButton(buttonsRow, "Guia de Apoio", OnGuidebookButtonPressed); CreateActionButton(buttonsRow, "Regras", ShowRulesOverlay);
        }

        private void UpdateHud()
        {
            if (hudCurrentPlayer == null) return;
            hudCurrentPlayer.text = $"Jogador atual: {(currentTurnPlayer == PlayerId.PlayerOne ? "Jogador 1" : "Jogador 2")}";
            hudObjective.text = $"Objetivo: identificar {ActiveModeConfig.ObjectiveIdentifications}";
            hudScore.text = $"Placar J1 {scores[PlayerId.PlayerOne]} x {scores[PlayerId.PlayerTwo]} J2";
            hudResearch.text = $"Fichas de Pesquisa: {researchTokens[currentTurnPlayer]}";
        }

        private void CompleteMatch(PlayerId winner)
        {
            CurrentPhase = GamePhase.End;
            investigationOverlayView.Hide(); guidebookOverlayView.Hide(); HideReadyScreen();
            string modeLabel = ActiveModeConfig != null ? ActiveModeConfig.DisplayName : "Modo desconhecido";
            string winnerLabel = winner == PlayerId.PlayerOne ? "Jogador 1" : "Jogador 2";
            EnsureWinScreenView();
            winScreenView.Show($"Vitória do {winnerLabel}!", $"Personagens identificados: {scores[winner]}\nModo: {modeLabel}", RestartCurrentModeMatch, ReturnToMainMenu);
            LogTelemetry("match_finished", $"winner={winner};mode={ActiveModeConfig?.Id};duration={ActiveModeConfig?.DurationMinutes};final_scores={scores[PlayerId.PlayerOne]}-{scores[PlayerId.PlayerTwo]};total_clues_requested={totalCluesRequested};total_research_uses={totalResearchUses};total_guesses={totalGuesses}");
        }

        private void RestartCurrentModeMatch() { winScreenView.Hide(); StartPassAndPlaySetup(); }
        private void ReturnToMainMenu()
        {
            winScreenView.Hide();
            if (sceneRoot?.FullScreenRoot == null) return;
            for (int i = sceneRoot.FullScreenRoot.childCount - 1; i >= 0; i--) Destroy(sceneRoot.FullScreenRoot.GetChild(i).gameObject);
            CurrentPhase = GamePhase.MainMenu;
            InitializeMainMenu(sceneRoot);
        }

        private string GetClueText(string characterId, ClueCategory category)
        {
            foreach (var c in PrototypeDatabase.Characters)
            {
                if (!characterId.Contains(c.Id, StringComparison.OrdinalIgnoreCase)) continue;
                return category switch { ClueCategory.Area => c.Area, ClueCategory.Era => c.Era, ClueCategory.Region => c.Region, ClueCategory.Contribution => c.Contribution, _ => c.ContextOrLegacy };
            }
            return "Pista indisponível no protótipo.";
        }

        private static Text CreateHudText(RectTransform parent, string value) { GameObject go = new GameObject("HudText", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); Text t = go.GetComponent<Text>(); t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = 28; t.color = Color.white; t.text = value; return t; }
        private HashSet<ClueCategory> GetKnownClues(PlayerId player, string characterId) { if (!discoveredClues[player].TryGetValue(characterId, out HashSet<ClueCategory> clues)) { clues = new HashSet<ClueCategory>(); discoveredClues[player][characterId] = clues; } return clues; }
        private CharacterData FindCharacterByCardId(string characterId) { foreach (CharacterData character in PrototypeDatabase.Characters) if (characterId.Contains(character.Id, StringComparison.OrdinalIgnoreCase)) return character; return null; }
        private ClueCategory GetLastKnownClue(string characterId) { foreach (ClueCategory category in GetKnownClues(currentTurnPlayer, characterId)) return category; return ClueCategory.Area; }
        private static string GetCategoryLabel(ClueCategory category) => category switch { ClueCategory.Area => "Área", ClueCategory.Era => "Época", ClueCategory.Region => "Região", ClueCategory.Contribution => "Contribuição", _ => "Contexto/Legado" };
        private static void LogTelemetry(string eventName, string payload) => Debug.Log($"telemetry:{eventName}:{payload}");
        private static int ParseCardIndex(string cardId) { if (string.IsNullOrEmpty(cardId)) return 0; int underscore = cardId.LastIndexOf('_'); if (underscore < 0 || underscore >= cardId.Length - 1) return 0; return int.TryParse(cardId.Substring(underscore + 1), out int value) ? value : 0; }

        private List<Vector2Int> GetAdjacentHiddenCoordinates(Vector2Int from)
        {
            List<Vector2Int> list = new List<Vector2Int>(); Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = from + direction;
                if (next.x < 0 || next.y < 0 || next.x >= ActiveModeConfig.BoardSize.x || next.y >= ActiveModeConfig.BoardSize.y) continue;
                PlacedCardData candidate = boardController.GetPlacedCard(next);
                if (candidate != null && !candidate.IsFaceUp) list.Add(next);
            }
            return list;
        }

        private List<string> GetDiscoveredButUnidentifiedCharacters() { List<string> characters = new List<string>(); foreach (var pair in discoveredClues[currentTurnPlayer]) if (!identifiedCharacters[currentTurnPlayer].Contains(pair.Key)) characters.Add(pair.Key); return characters; }
        private bool TryRevealExtraClue(string characterId, out ClueCategory category, out string clueText)
        {
            HashSet<ClueCategory> known = GetKnownClues(currentTurnPlayer, characterId);
            ClueCategory[] order = { ClueCategory.Area, ClueCategory.Era, ClueCategory.Region, ClueCategory.Contribution, ClueCategory.ContextLegacy };
            foreach (ClueCategory c in order) { if (known.Contains(c)) continue; known.Add(c); blockedCharacterGuesses[currentTurnPlayer].Remove(characterId); category = c; clueText = GetClueText(characterId, c); return true; }
            category = ClueCategory.Area; clueText = string.Empty; return false;
        }
        private void ShowNoValidTargetArchiveEffect(string effectName, string cardId) { investigationOverlayView.Show("Efeito de Arquivo", "Sem alvo válido. A investigação continua."); investigationOverlayView.AddButton("Continuar", EndTurnAfterOverlay); LogTelemetry("archive_effect_resolved", $"card={cardId};effect={effectName};result=sem_alvo_valido"); }

        private void OnGuidebookButtonPressed()
        {
            EnsureInvestigationOverlayView();
            if (researchTokens[currentTurnPlayer] <= 0) { investigationOverlayView.Show("Guia de Apoio", "Você não tem Fichas de Pesquisa restantes."); investigationOverlayView.AddButton("Fechar", investigationOverlayView.Hide); return; }
            investigationOverlayView.Show("Guia de Apoio", "Gastar 1 Ficha de Pesquisa para abrir o guia?");
            investigationOverlayView.AddButton("Confirmar", () => { researchTokens[currentTurnPlayer] = Mathf.Max(0, researchTokens[currentTurnPlayer] - 1); totalResearchUses += 1; UpdateHud(); investigationOverlayView.Hide(); ShowGuidebookOverlay(); });
            investigationOverlayView.AddButton("Cancelar", investigationOverlayView.Hide);
        }
        private void ShowGuidebookOverlay() { EnsureGuidebookOverlayView(); guidebookOverlayView.Show(PrototypeDatabase.Characters); }
        private void ShowRulesOverlay() { EnsureInvestigationOverlayView(); investigationOverlayView.Show("Resumo das Regras", "1. Investigue posições no arquivo adversário.\n2. Ao encontrar um personagem, peça uma pista.\n3. Use as pistas para identificar quem é.\n4. Você pode gastar Fichas de Pesquisa para consultar o Guia de Apoio.\n5. Cartas de Arquivo dão efeitos úteis.\n6. Vence quem completar o objetivo do modo escolhido."); investigationOverlayView.AddButton("Fechar", investigationOverlayView.Hide); }

        private void HideSetupSensitiveUi()
        {
            if (trayRoot != null) trayRoot.gameObject.SetActive(false);
            if (placedActionsRoot != null) placedActionsRoot.gameObject.SetActive(false);
            CanvasGroup cg = sceneRoot.CenterBoardArea.GetComponent<CanvasGroup>(); if (cg == null) cg = sceneRoot.CenterBoardArea.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0.05f;
        }
        private void StorePlacedCardForCurrentPlayer(PlacedCardData placed) { playerBoardStates[currentSetupPlayer].RemoveAll(c => c.CardId == placed.CardId); playerBoardStates[currentSetupPlayer].Add(new PlacedCardData { CardId = placed.CardId, CardType = placed.CardType, Owner = placed.Owner, Coordinate = placed.Coordinate, IsFaceUp = placed.IsFaceUp }); }
        private void RemoveStoredPlacedCardForCurrentPlayer(string cardId) => playerBoardStates[currentSetupPlayer].RemoveAll(c => c.CardId == cardId);

        private void EnsureBoardController() { if (boardController != null) return; GameObject boardObject = new GameObject("BoardController", typeof(RectTransform)); boardObject.transform.SetParent(sceneRoot.CenterBoardArea, false); boardController = boardObject.AddComponent<BoardController>(); }
        private void EnsureTutorialOverlay() { if (tutorialOverlay != null) return; GameObject overlayObject = new GameObject("TutorialOverlayView"); overlayObject.transform.SetParent(sceneRoot.OverlayLayer, false); tutorialOverlay = overlayObject.AddComponent<TutorialOverlayView>(); tutorialOverlay.Initialize(sceneRoot.OverlayLayer); }
        private void EnsureInvestigationOverlayView() { if (investigationOverlayView != null) return; GameObject go = new GameObject("InvestigationOverlayView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); investigationOverlayView = go.AddComponent<InvestigationOverlayView>(); investigationOverlayView.Initialize(sceneRoot.OverlayLayer); }
        private void EnsureGuidebookOverlayView() { if (guidebookOverlayView != null) return; GameObject go = new GameObject("GuidebookOverlayView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); guidebookOverlayView = go.AddComponent<GuidebookOverlayView>(); guidebookOverlayView.Initialize(sceneRoot.OverlayLayer); }
        private void EnsureWinScreenView() { if (winScreenView != null) return; GameObject go = new GameObject("WinScreenView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); winScreenView = go.AddComponent<WinScreenView>(); winScreenView.Initialize(sceneRoot.OverlayLayer); }
        private void EnsureReadyScreenView() { if (readyScreenView != null) return; GameObject go = new GameObject("ReadyScreenView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); readyScreenView = go.AddComponent<ReadyScreenView>(); readyScreenView.Initialize(sceneRoot.OverlayLayer); }
        private void ShowReadyScreen(string message, string buttonText, Action onConfirm) { EnsureReadyScreenView(); readyScreenView.Show(message, buttonText, onConfirm); }
        private void HideReadyScreen() => readyScreenView?.Hide();

        private static Button CreateActionButton(RectTransform parent, string text, Action onClick)
        {
            GameObject go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false); go.GetComponent<LayoutElement>().preferredHeight = 88f; go.GetComponent<Image>().color = new Color(0.16f, 0.43f, 0.84f, 1f);
            Button b = go.GetComponent<Button>(); b.onClick.AddListener(() => onClick?.Invoke());
            GameObject label = new GameObject("Label", typeof(RectTransform), typeof(Text)); RectTransform lr = label.GetComponent<RectTransform>(); lr.SetParent(go.transform, false); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            Text t = label.GetComponent<Text>(); t.text = text; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.alignment = TextAnchor.MiddleCenter; t.fontSize = 28; t.color = Color.white;
            return b;
        }
        private static void CreateTitle(RectTransform parent, string textValue) { GameObject titleObj = new GameObject("MainMenuTitle", typeof(RectTransform), typeof(LayoutElement), typeof(Text)); titleObj.transform.SetParent(parent, false); titleObj.GetComponent<LayoutElement>().preferredHeight = 280f; Text text = titleObj.GetComponent<Text>(); text.text = textValue; text.alignment = TextAnchor.MiddleCenter; text.fontSize = 82; text.color = Color.white; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
        private static void CreateFooter(RectTransform parent, string textValue) { GameObject spacer = new GameObject("FooterSpacer", typeof(RectTransform), typeof(LayoutElement)); spacer.transform.SetParent(parent, false); spacer.GetComponent<LayoutElement>().flexibleHeight = 1f; GameObject footerObj = new GameObject("MainMenuFooter", typeof(RectTransform), typeof(LayoutElement), typeof(Text)); footerObj.transform.SetParent(parent, false); footerObj.GetComponent<LayoutElement>().preferredHeight = 120f; Text text = footerObj.GetComponent<Text>(); text.text = textValue; text.alignment = TextAnchor.MiddleCenter; text.fontSize = 38; text.color = new Color(0.84f, 0.88f, 0.92f, 1f); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
    }
}
