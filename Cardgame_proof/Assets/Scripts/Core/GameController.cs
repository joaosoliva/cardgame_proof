using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using CardgameProof.Bootstrap;

namespace CardgameProof.Core
{
    public sealed class GameController : MonoBehaviour
    {
        private enum GuessSource { FreshDiscovery, PersistentAction }
        private static readonly IReadOnlyList<TutorialStep> SetupTutorialSteps = new List<TutorialStep>
        {
            new TutorialStep { Id = "setup_intro", Title = "Bem-vindo ao Arquivo", Body = "Monte um arquivo secreto e investigue o arquivo do outro jogador para descobrir personagens acadêmicos usando pistas e pesquisa.", Phase = GamePhase.Setup, TargetKey = null, CompleteTrigger = TutorialTrigger.ContinueButton, ShowContinueButton = true, BlockOutsideTarget = false, PreferredPlacement = TutorialPanelPlacement.Center },
            new TutorialStep { Id = "setup_archive_card", Title = "Cartas de Arquivo", Body = "Toque em uma Carta de Arquivo para analisá-la.", CompactBody = "Analise a carta antes de posicionar.", Phase = GamePhase.Setup, TargetKey = "archive_card_hand", CompleteTrigger = TutorialTrigger.ArchiveCardFocused, ShowContinueButton = false, PreferredPlacement = TutorialPanelPlacement.Top, FadeDuringAction = true, CompactMode = true },
            new TutorialStep { Id = "setup_place_archive", Title = "Posicionar no arquivo", Body = "Toque em Posicionar no Arquivo e escolha uma célula da grade.", CompactBody = "Toque em Posicionar no Arquivo.", Phase = GamePhase.Setup, TargetKey = "board_grid", CompleteTrigger = TutorialTrigger.ArchiveCardPlaced, ShowContinueButton = false, PreferredPlacement = TutorialPanelPlacement.Top, FadeDuringAction = true, CompactMode = true },
            new TutorialStep { Id = "setup_character_card", Title = "Dossiês de Personagem", Body = "Toque em um Dossiê para ver suas informações antes de posicionar.", CompactBody = "Analise o Dossiê antes de posicionar.", Phase = GamePhase.Setup, TargetKey = "character_card_hand", CompleteTrigger = TutorialTrigger.CharacterCardFocused, ShowContinueButton = false, PreferredPlacement = TutorialPanelPlacement.Top, FadeDuringAction = true, CompactMode = true },
            new TutorialStep { Id = "setup_place_character", Title = "Escolha uma posição", Body = "Toque em uma célula destacada para esconder o Dossiê no arquivo.", Phase = GamePhase.Setup, TargetKey = "board_grid", CompleteTrigger = TutorialTrigger.CharacterCardPlaced, ShowContinueButton = false, PreferredPlacement = TutorialPanelPlacement.UpperBoard, CompactMode = true },
            new TutorialStep { Id = "setup_remaining_cards", Title = "Complete sua montagem", Body = "Agora posicione o restante das cartas importantes no arquivo.", Phase = GamePhase.Setup, TargetKey = "board_grid", CompleteTrigger = TutorialTrigger.AllRequiredCardsPlaced, ShowContinueButton = false, PreferredPlacement = TutorialPanelPlacement.UpperBoard, CompactMode = true },
            new TutorialStep { Id = "setup_finalize", Title = "Finalizar montagem", Body = "Toque em Finalizar montagem para completar o arquivo.", Phase = GamePhase.Setup, TargetKey = "confirm_setup_button", CompleteTrigger = TutorialTrigger.SetupConfirmed, ShowContinueButton = false, PreferredPlacement = TutorialPanelPlacement.Top, FadeDuringAction = true, CompactMode = true },
            new TutorialStep { Id = "setup_no_record", Title = "Cartas Sem Registro", Body = "As lacunas serão preenchidas automaticamente com cartas Sem Registro. Elas representam partes do arquivo onde nada útil foi encontrado.", Phase = GamePhase.Setup, TargetKey = "board_grid", CompleteTrigger = TutorialTrigger.AutoNoRecordFillCompleted, ShowContinueButton = false, PreferredPlacement = TutorialPanelPlacement.Top, CompactMode = true }
        };
        private static readonly IReadOnlyList<TutorialStep> InvestigationTutorialSteps = new List<TutorialStep>
        {
            new TutorialStep { Id = "investigation_intro", Title = "Investigue o arquivo adversário", Body = "Em seu turno, você escolhe uma carta virada para baixo no arquivo do outro jogador.", Phase = GamePhase.Investigation, TargetKey = "board_grid", CompleteTrigger = TutorialTrigger.ContinueButton, ShowContinueButton = true, BlockGameplayInput = true, PreferredPlacement = TutorialPanelPlacement.UpperBoard, CompactMode = true },
            new TutorialStep { Id = "investigation_results", Title = "Três resultados possíveis", Body = "Você pode encontrar Sem Registro, uma Carta de Arquivo ou um Dossiê de Personagem.", Phase = GamePhase.Investigation, TargetKey = "board_grid", CompleteTrigger = TutorialTrigger.ContinueButton, ShowContinueButton = true, BlockGameplayInput = true, PreferredPlacement = TutorialPanelPlacement.UpperBoard, CompactMode = true },
            new TutorialStep { Id = "no_record_explain", Title = "Sem Registro", Body = "Sem Registro não tem efeito. A carta fica revelada e o turno passa adiante.", Phase = GamePhase.Investigation, TargetKey = "board_grid", CompleteTrigger = TutorialTrigger.ContinueButton, ShowContinueButton = true, BlockGameplayInput = true, PreferredPlacement = TutorialPanelPlacement.UpperBoard, CompactMode = true },
            new TutorialStep { Id = "archive_explain", Title = "Cartas de Arquivo", Body = "Cartas de Arquivo revelam imediatamente e ativam seu efeito uma única vez.", Phase = GamePhase.Investigation, TargetKey = "rules_card", CompleteTrigger = TutorialTrigger.ContinueButton, ShowContinueButton = true, BlockGameplayInput = true, PreferredPlacement = TutorialPanelPlacement.Top, CompactMode = true },
            new TutorialStep { Id = "dossier_explain", Title = "Primeira pista", Body = "Ao encontrar um Dossiê, escolha uma categoria para receber a primeira pista gratuita. O nome continua oculto até a identificação correta.", Phase = GamePhase.Investigation, TargetKey = "board_grid", CompleteTrigger = TutorialTrigger.ContinueButton, ShowContinueButton = true, BlockGameplayInput = true, PreferredPlacement = TutorialPanelPlacement.UpperBoard, CompactMode = true },
            new TutorialStep { Id = "guide_explain", Title = "Guia de Apoio", Body = "Use o Guia de Apoio para consultar mini biografias e comparar com as pistas reveladas.", Phase = GamePhase.Investigation, TargetKey = "guide_button", CompleteTrigger = TutorialTrigger.ContinueButton, ShowContinueButton = true, BlockGameplayInput = true, PreferredPlacement = TutorialPanelPlacement.Top, CompactMode = true },
            new TutorialStep { Id = "identify_explain", Title = "Identificar Dossiê", Body = "Depois de revelar pelo menos uma pista, você pode tentar identificar um Dossiê encontrado.", Phase = GamePhase.Investigation, TargetKey = "identify_button", CompleteTrigger = TutorialTrigger.ContinueButton, ShowContinueButton = true, BlockGameplayInput = true, PreferredPlacement = TutorialPanelPlacement.Top, CompactMode = true }
        };
        private static readonly IReadOnlyList<TutorialStep> PlayerTwoSetupTutorialSteps = new List<TutorialStep>
        {
            new TutorialStep { Id = "player2_setup_intro", Title = "Vez do Jogador 2", Body = "Monte seu arquivo da mesma forma: posicione Dossiês e Cartas de Arquivo. As lacunas serão preenchidas com Sem Registro.", Phase = GamePhase.Setup, TargetKey = null, CompleteTrigger = TutorialTrigger.ContinueButton, ShowContinueButton = true, PreferredPlacement = TutorialPanelPlacement.Center }
        };



        private SceneRootBuilder sceneRoot;
        private TutorialOverlayView tutorialOverlay;
        private TutorialManager tutorialManager;
        private ReadyScreenView readyScreenView;
        private InvestigationOverlayView investigationOverlayView;
        private GuidebookOverlayView guidebookOverlayView;
        private WinScreenView winScreenView;
        private MatchReportView matchReportView;
        private CardRevealOverlayView cardRevealOverlayView;
        private readonly MatchReportService matchReportService = new MatchReportService();
        private string lastReportText;
        private BoardController boardController;
        private RectTransform mainMenuRoot;
        private HowToPlayView howToPlayView;
        private readonly List<HowToPlayPageData> howToPlayPages = BuildHowToPlayPages();

        private RectTransform trayRoot;
        private RectTransform trayCardsRow;
        private RectTransform setupPlacementInstructionRoot;
        private FocusCardView focusCardView;
        private PlacedCardData selectedCardForInspection;
        private PlacedCardData selectedCardForPlacement;
        private bool isInspectingCard;
        private bool isPlacementModeActive;
        private bool setupReadyToFinalize;
        private RectTransform placedActionsRoot;
        private Button finalizeSetupButton;
        private readonly List<SetupCardView> trayCards = new List<SetupCardView>();
        private Vector2Int? selectedPlacedCoordinate;

        private readonly Dictionary<PlayerId, List<PlacedCardData>> playerBoardStates = new Dictionary<PlayerId, List<PlacedCardData>>();
        private readonly Dictionary<PlayerId, HashSet<string>> identifiedCharacters = new Dictionary<PlayerId, HashSet<string>>();
        private readonly Dictionary<PlayerId, int> scores = new Dictionary<PlayerId, int>();
        private readonly Dictionary<PlayerId, int> researchTokens = new Dictionary<PlayerId, int>();
        private readonly Dictionary<PlayerId, Dictionary<string, HashSet<ClueCategory>>> discoveredClues = new Dictionary<PlayerId, Dictionary<string, HashSet<ClueCategory>>>();
        private readonly Dictionary<PlayerId, HashSet<string>> firstCluePendingByPlayer = new Dictionary<PlayerId, HashSet<string>>();
        private readonly Dictionary<PlayerId, HashSet<string>> firstClueChosenByPlayer = new Dictionary<PlayerId, HashSet<string>>();

        private int totalCluesRequested;
        private int totalResearchUses;
        private int totalGuesses;
        private bool isAutoFillingNoRecord;
        private bool isRevealAnimationPlaying;

        private PlayerId currentSetupPlayer = PlayerId.PlayerOne;
        private PlayerId currentTurnPlayer = PlayerId.PlayerOne;

        private RectTransform hudRoot;
        private RectTransform hudButtonCardsRoot;
        private Button guideCardButton;
        private Button rulesCardButton;
        private TextMeshProUGUI guideCardTokensText;
        private TextMeshProUGUI identifyCardDetailText;
        private Button identifyCardButton;
        private TextMeshProUGUI hudCurrentPlayer;
        private TextMeshProUGUI hudObjective;
        private TextMeshProUGUI hudScore;
        private TextMeshProUGUI hudResearch;
        private bool identificationHintShown;
        private bool playerTwoShortTutorialShown;
        private bool investigationTutorialSeen;

        public GameModeConfig ActiveModeConfig { get; private set; }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.MainMenu;

        public void InitializeMainMenu(SceneRootBuilder builtSceneRoot)
        {
            if (builtSceneRoot == null || builtSceneRoot.FullScreenRoot == null) return;
            sceneRoot = builtSceneRoot;
            Debug.Log("[GameController] InitializeMainMenu called");
            DisableLegacyManualSceneObjects();

            EnsureTutorialOverlay();
            EnsureReadyScreenView();
            EnsureInvestigationOverlayView();
            EnsureGuidebookOverlayView();
            EnsureWinScreenView();
            EnsureMatchReportView();
            EnsureCardRevealOverlayView();
            EnsureBoardController();
            HideAllScreensAndGameplayUi();

            RectTransform fullRoot = sceneRoot.FullScreenRoot;
            if (fullRoot.Find("MainMenuRoot") != null)
            {
                mainMenuRoot = fullRoot.Find("MainMenuRoot") as RectTransform;
                if (mainMenuRoot != null) mainMenuRoot.gameObject.SetActive(true);
                EnsureHowToPlayView();
                SetPhase(GamePhase.MainMenu);
                Debug.Log("[GameController] Entered MainMenu phase");
                return;
            }

            GameObject menuRootObject = new GameObject("MainMenuRoot", typeof(RectTransform), typeof(Image));
            mainMenuRoot = menuRootObject.GetComponent<RectTransform>();
            mainMenuRoot.SetParent(fullRoot, false);
            mainMenuRoot.anchorMin = Vector2.zero; mainMenuRoot.anchorMax = Vector2.one;
            mainMenuRoot.offsetMin = Vector2.zero; mainMenuRoot.offsetMax = Vector2.zero;
            mainMenuRoot.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 1f);

            CreateTitle(mainMenuRoot, "Arquivo da Investigação");
            CreateMenuActionButton(mainMenuRoot, "Como Funciona", new Vector2(0.5f, 0.58f), () =>
            {
                Debug.Log("[MAIN_MENU] Como Funciona clicked");
                OpenHowToPlay();
            });
            CreateMenuActionButton(mainMenuRoot, "Jogar Protótipo", new Vector2(0.5f, 0.42f), () => StartGameMode("quick_5"));
            CreateFooter(mainMenuRoot, "Modo digital: seleção de modo → montagem → investigação");
            EnsureHowToPlayView();
            SetPhase(GamePhase.MainMenu);
            Debug.Log("[GameController] Entered MainMenu phase");
        }

        private void CreateMenuActionButton(RectTransform parent, string label, Vector2 anchor, UnityAction onClick)
        {
            GameObject buttonObj = new GameObject($"{label}_Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(parent, false);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchor; buttonRect.anchorMax = anchor; buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(760f, 190f);
            buttonObj.GetComponent<Image>().color = new Color(0.19f, 0.46f, 0.88f, 1f);
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform lr = labelObj.GetComponent<RectTransform>(); lr.SetParent(buttonObj.transform, false); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            TextMeshProUGUI t = labelObj.GetComponent<TextMeshProUGUI>();
            t.text = label;
            t.fontSize = 52;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
        }

        public void StartGameMode(string modeId)
        {
            string resolvedModeId = modeId;
            if (modeId == "quick_5")
            {
                resolvedModeId = "5min";
                Debug.Log("[GAME] Mode selected: quick_5");
            }
            else if (modeId == "full_10")
            {
                resolvedModeId = "10min";
                Debug.Log("[GAME] Mode selected: full_10");
            }

            AudioManager.Instance?.PlayButton();
            howToPlayView?.Hide();
            ResetRuntimeUIState("StartGameMode");
            LoadPrototypeMode(resolvedModeId);
            if (ActiveModeConfig == null) return;
            if (mainMenuRoot != null) mainMenuRoot.gameObject.SetActive(false);
            Debug.Log("[GAME] Entering Tutorial/Setup");
            TransitionToTutorialIntro();
            StartPassAndPlaySetup();
        }

        private void OpenHowToPlay()
        {
            EnsureHowToPlayView();
            if (howToPlayView == null)
            {
                Debug.LogWarning("[HOW_TO_PLAY] Could not open: HowToPlayView is unavailable.");
                return;
            }

            Debug.Log("[HOW_TO_PLAY] Open");
            howToPlayView.Show(
                howToPlayPages,
                () =>
                {
                    Debug.Log("[HOW_TO_PLAY] Close");
                    if (howToPlayView != null) howToPlayView.Hide();
                },
                () =>
                {
                    Debug.Log("[HOW_TO_PLAY] Play Prototype clicked");
                    StartGameMode("quick_5");
                });
        }

        private void EnsureHowToPlayView()
        {
            if (howToPlayView != null) return;
            if (sceneRoot == null || sceneRoot.FullScreenRoot == null)
            {
                Debug.LogWarning("[HOW_TO_PLAY] Initialization failed: sceneRoot/FullScreenRoot is null.");
                return;
            }
            GameObject go = new GameObject("HowToPlayView");
            go.transform.SetParent(sceneRoot.FullScreenRoot, false);
            go.transform.SetAsLastSibling();
            howToPlayView = go.AddComponent<HowToPlayView>();
            howToPlayView.Initialize(sceneRoot.FullScreenRoot);
        }

        public void LoadPrototypeMode(string modeId) => ActiveModeConfig = PrototypeDatabase.GetMode(modeId);
        public void ShowTutorialSequence(IReadOnlyList<TutorialStep> sequence, Action onCompleted = null) { EnsureTutorialOverlay(); tutorialManager?.StartSequence(sequence, onCompleted); UpdateTutorialLayoutContext(); }
        private void TransitionToTutorialIntro() => SetPhase(GamePhase.TutorialIntro);

        private void StartPassAndPlaySetup()
        {
            ResetRuntimeUIState("StartNewGame");
            playerBoardStates[PlayerId.PlayerOne] = new List<PlacedCardData>();
            playerBoardStates[PlayerId.PlayerTwo] = new List<PlacedCardData>();
            playerTwoShortTutorialShown = false;
            investigationTutorialSeen = false;
            BeginSetupForPlayer(PlayerId.PlayerOne);
            matchReportService.StartMatch(ActiveModeConfig.DisplayName);
        }

        private void BeginSetupForPlayer(PlayerId player)
        {
            currentSetupPlayer = player;
            matchReportService.StartSetup(player);
            SetInvestigationControlsVisible(false, "EnterSetup");
            SetPhase(GamePhase.Setup);
            selectedCardForInspection = null;
            selectedCardForPlacement = null;
            isInspectingCard = false;
            isPlacementModeActive = false;
            focusCardView?.Hide();
            if (setupPlacementInstructionRoot != null) setupPlacementInstructionRoot.gameObject.SetActive(false);
            Debug.Log("[STATE] Enter setup");
            RestoreBoardVisualState();
            if (sceneRoot?.CenterBoardArea != null) sceneRoot.CenterBoardArea.gameObject.SetActive(true);
            BuildBoardForActiveMode();
            BuildBottomTray();
            BuildPlacedCardActions();
            GenerateCurrentPlayerSetupCards(player);
            UpdateFinalizeButtonState();
            if (player == PlayerId.PlayerOne)
            {
                ShowTutorialSequence(SetupTutorialSteps);
            }
            else if (!playerTwoShortTutorialShown)
            {
                playerTwoShortTutorialShown = true;
                ShowTutorialSequence(PlayerTwoSetupTutorialSteps);
            }
            HideReadyScreen();
            winScreenView?.Hide();
        }

        private void BuildBoardForActiveMode()
        {
            EnsureBoardController();
            boardController.ClearBoard();
            boardController.BuildBoard(sceneRoot.CenterBoardArea, ActiveModeConfig.BoardSize, null);
            tutorialManager?.RegisterTarget("board_grid", sceneRoot.CenterBoardArea);
            boardController.OnPlacedCardTapped = OnPlacedCardTapped;
            boardController.OnCellTapped = OnSetupBoardCellTapped;
            boardController.RefreshVisualsForPhase(CurrentPhase);
        }

        private void BuildBottomTray()
        {
            if (trayRoot != null) Destroy(trayRoot.gameObject);
            GameObject tray = new GameObject("SetupBottomTray", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            trayRoot = tray.GetComponent<RectTransform>();
            trayRoot.SetParent(sceneRoot.BottomCardTray, false);
            trayRoot.anchorMin = Vector2.zero; trayRoot.anchorMax = Vector2.one;
            trayRoot.offsetMin = new Vector2(20f, 20f); trayRoot.offsetMax = new Vector2(-20f, -20f);
            tray.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.18f);

            VerticalLayoutGroup vertical = tray.GetComponent<VerticalLayoutGroup>();
            vertical.spacing = 8f;
            vertical.padding = new RectOffset(16, 16, 10, 12);
            vertical.childForceExpandHeight = false;
            vertical.childForceExpandWidth = true;

            GameObject instructionObj = new GameObject("TrayInstruction", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            instructionObj.transform.SetParent(trayRoot, false);
            LayoutElement instructionLayout = instructionObj.GetComponent<LayoutElement>();
            instructionLayout.preferredHeight = 34f;
            TextMeshProUGUI instruction = instructionObj.GetComponent<TextMeshProUGUI>();
            instruction.text = "Toque em uma carta para analisar";
            instruction.fontSize = 24;
            instruction.alignment = TextAlignmentOptions.Center;
            instruction.color = Color.white;

            GameObject row = new GameObject("TrayCardsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            trayCardsRow = row.GetComponent<RectTransform>();
            trayCardsRow.SetParent(trayRoot, false);
            LayoutElement rowLayout = row.GetComponent<LayoutElement>();
            rowLayout.preferredHeight = 270f;
            rowLayout.flexibleHeight = 1f;
            HorizontalLayoutGroup h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 14f; h.childForceExpandWidth = true; h.padding = new RectOffset(0, 0, 0, 0);
        }

        private void BuildPlacedCardActions()
        {
            if (placedActionsRoot != null) Destroy(placedActionsRoot.gameObject);
            GameObject root = new GameObject("PlacedCardActions", typeof(RectTransform));
            placedActionsRoot = root.GetComponent<RectTransform>();
            placedActionsRoot.SetParent(sceneRoot.ActionArea, false);
            placedActionsRoot.anchorMin = Vector2.zero; placedActionsRoot.anchorMax = Vector2.one;
            placedActionsRoot.offsetMin = new Vector2(16f, 12f); placedActionsRoot.offsetMax = new Vector2(-16f, -12f);

            float spacing = 14f;
            float width = (sceneRoot.ActionArea.rect.width - 32f - (spacing * 2f)) / 3f;
            if (width < 200f) width = 200f;
            float x = width * -1f - spacing;

            RectTransform r1 = CreateActionButton(placedActionsRoot, "Remover", OnRemoveSelectedPlacedCard).GetComponent<RectTransform>();
            RectTransform r2 = CreateActionButton(placedActionsRoot, "Confirmar", () => { selectedPlacedCoordinate = null; boardController.SetSelectedCoordinate(null); }).GetComponent<RectTransform>();
            finalizeSetupButton = CreateActionButton(placedActionsRoot, "Finalizar montagem", OnFinalizeSetupPressed);
            tutorialManager?.RegisterTarget("confirm_setup_button", finalizeSetupButton.GetComponent<RectTransform>());
            RectTransform r3 = finalizeSetupButton.GetComponent<RectTransform>();
            PositionActionButton(r1, x + (width + spacing) * 0f, width);
            PositionActionButton(r2, x + (width + spacing) * 1f, width);
            PositionActionButton(r3, x + (width + spacing) * 2f, width);
        }

        private void GenerateCurrentPlayerSetupCards(PlayerId player)
        {
            foreach (var card in trayCards) if (card != null) Destroy(card.gameObject);
            trayCards.Clear();
            int total = ActiveModeConfig.CharactersPerPlayer + ActiveModeConfig.ArchiveCardsPerPlayer;
            for (int i = 0; i < total; i++)
            {
                CardType type;
                if (i < ActiveModeConfig.CharactersPerPlayer) type = CardType.Character;
                else type = CardType.Archive;
                string cardId = BuildPrototypeCardIdForSetup(player, type, i);
                var placed = new PlacedCardData { CardId = cardId, CardType = type, Owner = player, Coordinate = Vector2Int.zero, IsFaceUp = false };
                GameObject go = new GameObject($"SetupCard_{i}", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(CanvasGroup));
                go.transform.SetParent(trayCardsRow != null ? trayCardsRow : trayRoot, false);
                SetupCardView view = go.AddComponent<SetupCardView>();
                view.Initialize(FindFirstObjectByType<Canvas>(), placed, OnSetupCardDrop, OnSetupCardTapped, false);
                trayCards.Add(view);
                if (type == CardType.Character) tutorialManager?.RegisterTarget("character_card_hand", view.GetComponent<RectTransform>());
                if (type == CardType.Archive) tutorialManager?.RegisterTarget("archive_card_hand", view.GetComponent<RectTransform>());
            }
        }

        private string BuildPrototypeCardIdForSetup(PlayerId player, CardType type, int setupIndex)
        {
            if (type == CardType.Character)
            {
                int characterIndex = setupIndex;
                CharacterData character = GetCharacterForSetup(player, characterIndex);
                if (character == null)
                {
                    Debug.LogWarning($"[Cards] Character data missing for setupIndex={setupIndex}. Using fallback id.");
                    return $"{player}_character_missing_{setupIndex}";
                }
                return $"{player}_{character.Id}_{setupIndex}";
            }

            if (type == CardType.SemRegistro)
            {
                int semIndex = setupIndex - ActiveModeConfig.CharactersPerPlayer - ActiveModeConfig.ArchiveCardsPerPlayer;
                return $"{player}_sem_registro_{semIndex}";
            }

            int archiveIndex = setupIndex - ActiveModeConfig.CharactersPerPlayer;
            ArchiveCardData archive = GetArchiveForSetup(player, archiveIndex);
            if (archive == null)
            {
                Debug.LogWarning($"[Cards] Archive data missing for setupIndex={setupIndex}. Using fallback id.");
                return $"{player}_archive_missing_{archiveIndex}";
            }
            return $"{player}_{archive.Id}_{archiveIndex}";
        }

        private static CharacterData GetCharacterForSetup(PlayerId player, int index)
        {
            if (PrototypeDatabase.Characters == null || PrototypeDatabase.Characters.Count == 0) return null;
            int offset = player == PlayerId.PlayerOne ? 0 : 1;
            int mapped = (index + offset) % PrototypeDatabase.Characters.Count;
            return PrototypeDatabase.Characters[mapped];
        }

        private static ArchiveCardData GetArchiveForSetup(PlayerId player, int index)
        {
            if (PrototypeDatabase.ArchiveCards == null || PrototypeDatabase.ArchiveCards.Count == 0) return null;
            int offset = player == PlayerId.PlayerOne ? 0 : 1;
            int mapped = (index + offset) % PrototypeDatabase.ArchiveCards.Count;
            return PrototypeDatabase.ArchiveCards[mapped];
        }

        private void OnSetupCardTapped(SetupCardView cardView)
        {
            if (cardView == null || isPlacementModeActive || isAutoFillingNoRecord) return;
            if (cardView.CardData == null || cardView.CardData.CardType == CardType.SemRegistro) return;

            AudioManager.Instance?.PlayCardPick();
            selectedCardForInspection = cardView.CardData;
            isInspectingCard = true;
            EnsureFocusCardView();
            focusCardView.Show(selectedCardForInspection, OnFocusPlacePressed, OnFocusBackPressed);
            UpdateTutorialLayoutContext();
            tutorialManager?.Notify(selectedCardForInspection.CardType == CardType.Character ? TutorialTrigger.CharacterCardFocused : TutorialTrigger.ArchiveCardFocused);
        }

        private void OnFocusPlacePressed()
        {
            if (selectedCardForInspection == null) return;
            AudioManager.Instance?.PlayButton();
            selectedCardForPlacement = selectedCardForInspection;
            selectedCardForInspection = null;
            isInspectingCard = false;
            focusCardView?.Hide();
            UpdateTutorialLayoutContext();
            EnterPlacementMode();
            tutorialManager?.Notify(TutorialTrigger.CardPlacementModeStarted);
        }

        private void OnFocusBackPressed()
        {
            AudioManager.Instance?.PlayButton();
            selectedCardForInspection = null;
            isInspectingCard = false;
            selectedCardForPlacement = null;
            focusCardView?.Hide();
            ExitPlacementMode(clearSelection: true);
            UpdateTutorialLayoutContext();
        }

        private void EnterPlacementMode()
        {
            if (selectedCardForPlacement == null) return;
            isPlacementModeActive = true;
            selectedPlacedCoordinate = null;
            boardController.SetSelectedCoordinate(null);
            boardController.SetPlacementHighlights(true);
            ShowPlacementInstruction(true);
            SetTraySelectionEnabled(false);
            UpdateTutorialLayoutContext();
        }

        private void ExitPlacementMode(bool clearSelection)
        {
            isPlacementModeActive = false;
            if (clearSelection) selectedCardForPlacement = null;
            boardController?.SetPlacementHighlights(false);
            ShowPlacementInstruction(false);
            SetTraySelectionEnabled(true);
            UpdateTutorialLayoutContext();
        }

        private void OnSetupBoardCellTapped(Vector2Int coordinate)
        {
            if (isInspectingCard) return;
            if (!isPlacementModeActive)
            {
                PlacedCardData existing = boardController.GetPlacedCard(coordinate);
                if (existing != null)
                {
                    OnPlacedCardTapped(coordinate);
                }
                return;
            }

            if (selectedCardForPlacement == null)
            {
                ExitPlacementMode(clearSelection: true);
                return;
            }

            if (!boardController.IsCellEmpty(coordinate))
            {
                AudioManager.Instance?.PlayInvalid();
                matchReportService.OnInvalidPlacement();
                ShowSetupFeedback("Escolha uma célula vazia.");
                return;
            }

            TryPlaceSelectedSetupCard(coordinate);
        }

        private void TryPlaceSelectedSetupCard(Vector2Int coordinate)
        {
            PlacedCardData data = selectedCardForPlacement;
            data.Coordinate = coordinate;
            TutorialTrigger placementTrigger = data.CardType == CardType.Character ? TutorialTrigger.CharacterCardPlaced : TutorialTrigger.ArchiveCardPlaced;
            tutorialManager?.NotifyActionStarted(placementTrigger);

            if (!boardController.PlaceCard(data))
            {
                tutorialManager?.NotifyActionEnded(placementTrigger);
                AudioManager.Instance?.PlayInvalid();
                matchReportService.OnInvalidPlacement();
                ShowSetupFeedback("Escolha uma célula vazia.");
                return;
            }

            AudioManager.Instance?.PlayCardPlace();
            SetupCardView trayCard = trayCards.Find(c => c != null && c.CardData.CardId == data.CardId);
            if (trayCard != null) trayCard.gameObject.SetActive(false);
            StorePlacedCardForCurrentPlayer(data);
            tutorialManager?.NotifyActionEnded(placementTrigger);
            tutorialManager?.Notify(placementTrigger);
            tutorialManager?.Notify(TutorialTrigger.AnyCardPlaced);
            ExitPlacementMode(clearSelection: true);
            ShowSetupFeedback("Carta posicionada");
            UpdateFinalizeButtonState();
        }

        private void SetTraySelectionEnabled(bool enabled)
        {
            foreach (SetupCardView card in trayCards)
            {
                if (card != null && card.gameObject.activeSelf) card.SetInteractionEnabled(enabled);
            }

            if (trayRoot != null)
            {
                CanvasGroup trayCg = trayRoot.GetComponent<CanvasGroup>();
                if (trayCg == null) trayCg = trayRoot.gameObject.AddComponent<CanvasGroup>();
                trayCg.alpha = enabled ? 1f : 0.55f;
                trayCg.interactable = enabled;
            }
        }

        private void ShowPlacementInstruction(bool show)
        {
            if (!show)
            {
                if (setupPlacementInstructionRoot != null) setupPlacementInstructionRoot.gameObject.SetActive(false);
                return;
            }

            if (setupPlacementInstructionRoot == null)
            {
                GameObject panel = new GameObject("SetupPlacementInstruction", typeof(RectTransform), typeof(Image));
                setupPlacementInstructionRoot = panel.GetComponent<RectTransform>();
                setupPlacementInstructionRoot.SetParent(sceneRoot.OverlayLayer, false);
                setupPlacementInstructionRoot.anchorMin = setupPlacementInstructionRoot.anchorMax = new Vector2(0.5f, 1f);
                setupPlacementInstructionRoot.pivot = new Vector2(0.5f, 1f);
                setupPlacementInstructionRoot.sizeDelta = new Vector2(760f, 120f);
                setupPlacementInstructionRoot.anchoredPosition = new Vector2(0f, -120f);
                Image image = panel.GetComponent<Image>();
                image.color = new Color(0.04f, 0.08f, 0.12f, 0.86f);
                image.raycastTarget = false;

                CreateInstructionText(setupPlacementInstructionRoot, "Title", "Escolha uma posição no arquivo", new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.94f), 34, FontStyles.Bold);
                CreateInstructionText(setupPlacementInstructionRoot, "Subtitle", "Toque em uma célula destacada", new Vector2(0.06f, 0.10f), new Vector2(0.94f, 0.48f), 25, FontStyles.Normal);
            }

            setupPlacementInstructionRoot.gameObject.SetActive(true);
            setupPlacementInstructionRoot.SetAsLastSibling();
        }

        private void CreateInstructionText(RectTransform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, int size, FontStyles style)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
        }

        private void ShowSetupFeedback(string text)
        {
            if (sceneRoot?.OverlayLayer == null) return;
            StartCoroutine(ShowSetupFeedbackCoroutine(text));
        }

        private IEnumerator ShowSetupFeedbackCoroutine(string text)
        {
            GameObject panel = new GameObject("SetupFeedback", typeof(RectTransform), typeof(Image));
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.SetParent(sceneRoot.OverlayLayer, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(700f, 95f);
            rect.anchoredPosition = new Vector2(0f, 390f);
            Image image = panel.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.76f);
            image.raycastTarget = false;

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 32;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;

            yield return new WaitForSecondsRealtime(0.85f);
            if (panel != null) Destroy(panel);
        }

        private void OnSetupCardDrop(SetupCardView cardView, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (cardView == null || eventData == null) return;
            if (!boardController.TryGetCoordinateFromScreenPosition(eventData.position, out Vector2Int coord)) { cardView.ResetToTray(); return; }
            selectedCardForPlacement = cardView.CardData;
            if (!boardController.IsCellEmpty(coord))
            {
                cardView.ResetToTray();
                AudioManager.Instance?.PlayInvalid();
                matchReportService.OnInvalidPlacement();
                ShowSetupFeedback("Escolha uma célula vazia.");
                return;
            }
            TryPlaceSelectedSetupCard(coord);
        }

        private void OnPlacedCardTapped(Vector2Int coordinate) { selectedPlacedCoordinate = coordinate; boardController.SetSelectedCoordinate(coordinate); }
        private void OnRemoveSelectedPlacedCard()
        {
            if (!selectedPlacedCoordinate.HasValue) return;
            PlacedCardData placed = boardController.GetPlacedCard(selectedPlacedCoordinate.Value);
            if (placed == null || !boardController.RemoveCard(selectedPlacedCoordinate.Value)) return;
            matchReportService.OnRepositionOrRemove();
            RemoveStoredPlacedCardForCurrentPlayer(placed.CardId);
            SetupCardView trayCard = trayCards.Find(c => c != null && c.CardData.CardId == placed.CardId);
            if (trayCard != null) trayCard.gameObject.SetActive(true);
            selectedPlacedCoordinate = null;
            boardController.SetSelectedCoordinate(null);
            UpdateFinalizeButtonState();
        }
        private void UpdateFinalizeButtonState()
        {
            int active = 0;
            foreach (var c in trayCards) if (c != null && c.gameObject.activeSelf) active++;
            bool ready = active == 0;
            setupReadyToFinalize = ready;
            finalizeSetupButton.interactable = true;
            Image buttonImage = finalizeSetupButton.GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = ready ? new Color(0.18f, 0.55f, 0.26f, 1f) : new Color(0.28f, 0.31f, 0.36f, 1f);
            TextMeshProUGUI label = finalizeSetupButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = ready ? "Finalizar montagem" : $"Finalizar montagem\nCartas restantes: {active}";
            if (ready) tutorialManager?.Notify(TutorialTrigger.AllRequiredCardsPlaced);
        }

        private void OnFinalizeSetupPressed()
        {
            if (isAutoFillingNoRecord)
            {
                return;
            }
            if (!setupReadyToFinalize)
            {
                AudioManager.Instance?.PlayInvalid();
                ShowSetupFeedback("Posicione todas as cartas antes de finalizar.");
                return;
            }
            Debug.Log("[STATE] Confirm setup");
            tutorialManager?.NotifyActionStarted(TutorialTrigger.SetupConfirmed);
            tutorialOverlay?.FadeTo(0f, 0.15f);
            tutorialManager?.Notify(TutorialTrigger.SetupConfirmed);
            matchReportService.EndSetup(currentSetupPlayer);
            HideSetupSensitiveUi();
            StartCoroutine(FillEmptyCellsWithNoRecordCoroutine(currentSetupPlayer, () =>
            {
                if (currentSetupPlayer == PlayerId.PlayerOne)
                {
                    ShowReadyScreen("Passe o aparelho para o Jogador 2", "Estou pronto", () => BeginSetupForPlayer(PlayerId.PlayerTwo));
                    return;
                }
                ShowReadyScreen("Passe o aparelho para o Jogador 1", "Começar investigação", StartInvestigationPhase);
            }));
        }

        private void StartInvestigationPhase()
        {
            HideReadyScreen();
            SetPhase(GamePhase.Investigation);
            SetInvestigationControlsVisible(true, "EnterInvestigation");
            Debug.Log("[STATE] Enter investigation");
            RestoreBoardVisualState();
            if (sceneRoot?.CenterBoardArea != null) sceneRoot.CenterBoardArea.gameObject.SetActive(true);
            BuildInvestigationHud();
            scores[PlayerId.PlayerOne] = 0; scores[PlayerId.PlayerTwo] = 0;
            researchTokens[PlayerId.PlayerOne] = ActiveModeConfig.ResearchTokensPerPlayer;
            researchTokens[PlayerId.PlayerTwo] = ActiveModeConfig.ResearchTokensPerPlayer;
            identifiedCharacters[PlayerId.PlayerOne] = new HashSet<string>(); identifiedCharacters[PlayerId.PlayerTwo] = new HashSet<string>();
            discoveredClues[PlayerId.PlayerOne] = new Dictionary<string, HashSet<ClueCategory>>(); discoveredClues[PlayerId.PlayerTwo] = new Dictionary<string, HashSet<ClueCategory>>();
            firstCluePendingByPlayer[PlayerId.PlayerOne] = new HashSet<string>(); firstCluePendingByPlayer[PlayerId.PlayerTwo] = new HashSet<string>();
            firstClueChosenByPlayer[PlayerId.PlayerOne] = new HashSet<string>(); firstClueChosenByPlayer[PlayerId.PlayerTwo] = new HashSet<string>();
            totalCluesRequested = 0; totalResearchUses = 0; totalGuesses = 0;
            identificationHintShown = false;
            currentTurnPlayer = PlayerId.PlayerOne;
            EnterInvestigationTurn(startTutorialIfNeeded: true);
        }

        private void EnterInvestigationTurn(bool startTutorialIfNeeded)
        {
            SetInvestigationControlsVisible(true, "EnterInvestigation");
            matchReportService.TurnStart(currentTurnPlayer);
            ShowOpponentBoardForCurrentTurn();
            UpdateHud();
            RunTurnSoftLockSafeguard();
            if (startTutorialIfNeeded)
            {
                StartInvestigationTutorial();
            }
        }

        private void StartInvestigationTutorial()
        {
            if (investigationTutorialSeen) return;
            Debug.Log("[TUTORIAL] Starting investigation tutorial");
            ShowTutorialSequence(InvestigationTutorialSteps, OnInvestigationTutorialCompleted);
        }

        private void OnInvestigationTutorialCompleted()
        {
            investigationTutorialSeen = true;
            Debug.Log("[TUTORIAL] Investigation tutorial completed; gameplay input unblocked.");
            if (hudObjective != null) hudObjective.text = "Escolha uma carta para investigar.";
        }

        private void ShowOpponentBoardForCurrentTurn()
        {
            RestoreBoardVisualState();
            PlayerId opponent = currentTurnPlayer == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne;
            boardController.ClearBoard();
            boardController.BuildBoard(sceneRoot.CenterBoardArea, ActiveModeConfig.BoardSize, null);
            tutorialManager?.RegisterTarget("board_grid", sceneRoot.CenterBoardArea);
            boardController.OnPlacedCardTapped = OnInvestigationCellTapped;
            Debug.Log($"[TURN] Switching to Player {currentTurnPlayer}. Rendering opponent board owned by {opponent}.");
            foreach (PlacedCardData card in playerBoardStates[opponent])
            {
                Debug.Log($"[BOARD] Rendering cell/card {card.CardId}: type={card.CardType}, investigated={card.IsInvestigated}, revealed={card.IsRevealed}, identified={card.IsIdentified}, effectResolved={card.EffectResolved}");
                boardController.PlaceCard(new PlacedCardData { CardId = card.CardId, CardType = card.CardType, Owner = card.Owner, Coordinate = card.Coordinate, IsFaceUp = card.IsRevealed, IsInvestigated = card.IsInvestigated, IsRevealed = card.IsRevealed, IsIdentified = card.IsIdentified, EffectResolved = card.EffectResolved });
            }
            boardController.RefreshVisualsForPhase(GamePhase.Investigation);
            sceneRoot.CenterBoardArea.localRotation = Quaternion.Euler(0f, 0f, currentTurnPlayer == PlayerId.PlayerOne ? 0f : 180f);
        }

        private void OnInvestigationCellTapped(Vector2Int coordinate)
        {
            if (tutorialManager != null && tutorialManager.IsBlockingGameplayInput)
            {
                Debug.Log("[TUTORIAL] Board input blocked during investigation tutorial.");
                return;
            }
            if (CurrentPhase != GamePhase.Investigation || isRevealAnimationPlaying) return;
            PlacedCardData card = boardController.GetPlacedCard(coordinate);
            if (card == null) return;
            if (card.IsInvestigated || card.IsRevealed || card.IsFaceUp)
            {
                if (card.CardType == CardType.Character && !card.IsIdentified)
                {
                    ShowDiscoveredCharacterActions(card.CardId);
                    return;
                }
                EnsureInvestigationOverlayView();
                investigationOverlayView.Show("Investigação", card.CardType == CardType.Archive ? "Esta carta de arquivo já foi revelada." : "Esta parte do arquivo já foi investigada.");
                investigationOverlayView.AddButton("Fechar", investigationOverlayView.Hide);
                return;
            }
            PlayerId opponent = currentTurnPlayer == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne;
            Debug.Log("[INVESTIGATION] Card clicked");
            Debug.Log($"[INVESTIGATION] Player {currentTurnPlayer} clicked card {card.CardId} type {card.CardType} on Player {opponent} board.");
            Debug.Log($"[INVESTIGATION] Before: investigated={card.IsInvestigated}, revealed={card.IsRevealed}, identified={card.IsIdentified}, effectResolved={card.EffectResolved}");
            AudioManager.Instance?.PlayReveal();
            tutorialOverlay?.FadeTo(0f, 0.1f);
            matchReportService.MarkFirstInvestigation();
            card.IsInvestigated = true;
            tutorialManager?.Notify(TutorialTrigger.CellInvestigated);
            if (card.CardType == CardType.Archive) { tutorialManager?.Notify(TutorialTrigger.ArchiveFound); EnsureInvestigationOverlayView(); ResolveArchiveCard(card); return; }
            if (card.CardType == CardType.SemRegistro) { tutorialManager?.Notify(TutorialTrigger.NoRecordFound); ResolveSemRegistroCard(card); return; }
            tutorialManager?.Notify(TutorialTrigger.CharacterFound);
            ResolveCharacterCard(card);
        }
        private void ResolveSemRegistroCard(PlacedCardData card)
        {
            card.IsRevealed = true;
            card.IsFaceUp = true;
            card.EffectResolved = true;
            PersistInvestigatedCard(card);
            boardController.RemoveCard(card.Coordinate); boardController.PlaceCard(card);
            Debug.Log($"[INVESTIGATION] After: investigated={card.IsInvestigated}, revealed={card.IsRevealed}, identified={card.IsIdentified}, effectResolved={card.EffectResolved}");
            matchReportService.OnNoRecordRevealed();
            PlayCardReveal(new CardRevealPayload
            {
                RevealType = CardRevealType.NoRecord,
                Title = "Sem Registro",
                Body = "Nenhum dossiê útil foi encontrado nesta parte do arquivo.",
                RequireTapToContinue = false,
                AutoContinueDelay = 1f
            }, EndTurnWithPassScreen);
        }

        private void ResolveArchiveCard(PlacedCardData card)
        {
            if (card.EffectResolved)
            {
                EnsureInvestigationOverlayView();
                investigationOverlayView.Show("Carta de Arquivo", "Esta carta de arquivo já foi revelada.");
                investigationOverlayView.AddButton("Fechar", investigationOverlayView.Hide);
                return;
            }
            card.IsRevealed = true;
            card.IsFaceUp = true;
            card.EffectResolved = true;
            PersistInvestigatedCard(card);
            boardController.RemoveCard(card.Coordinate); boardController.PlaceCard(card);
            Debug.Log($"[INVESTIGATION] After: investigated={card.IsInvestigated}, revealed={card.IsRevealed}, identified={card.IsIdentified}, effectResolved={card.EffectResolved}");
            LogTelemetry("archive_card_found", $"card={card.CardId}");
            int effectType = ParseCardIndex(card.CardId) % 3;
            string archiveTitle = effectType == 0 ? "Lacuna de Arquivo" : effectType == 1 ? "Referência Cruzada" : "Fragmento de Documento";
            string archiveBody = effectType == 0 ? "Você ganhou +1 Ficha de Pesquisa." : effectType == 1 ? "Permite investigar o tipo de uma célula adjacente." : "Revela uma pista extra para um personagem já descoberto.";
            PlayCardReveal(new CardRevealPayload
            {
                RevealType = CardRevealType.Archive,
                Title = archiveTitle,
                Body = archiveBody,
                RequireTapToContinue = true
            }, () =>
            {
                if (effectType == 0) { matchReportService.OnArchiveRevealed("lacuna"); ResolveArchiveLacunaDeArquivo(card.CardId); }
                else if (effectType == 1) { matchReportService.OnArchiveRevealed("referencia"); ResolveArchiveReferenciaCruzada(card); }
                else { matchReportService.OnArchiveRevealed("fragmento"); ResolveArchiveFragmentoDocumento(card.CardId); }
            });
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

        private void ResolveCharacterCard(PlacedCardData card)
        {
            PersistInvestigatedCard(card);
            Debug.Log($"[INVESTIGATION] After: investigated={card.IsInvestigated}, revealed={card.IsRevealed}, identified={card.IsIdentified}, effectResolved={card.EffectResolved}");
            matchReportService.MarkFirstCharacterFound(card.CardId);
            GetKnownClues(currentTurnPlayer, card.CardId);
            SetFirstCluePending(currentTurnPlayer, card.CardId, true);
            PlayCardReveal(new CardRevealPayload
            {
                RevealType = CardRevealType.CharacterFound,
                Title = "Dossiê encontrado",
                Body = "Você encontrou um personagem. Escolha uma primeira pista para investigar.",
                RequireTapToContinue = true
            }, () => ShowClueSelectionOverlay(card.CardId, true));
        }

        private void ShowClueSelectionOverlay(string characterId, bool firstClueContext = false)
        {
            EnsureInvestigationOverlayView();
            HashSet<ClueCategory> knownClues = GetKnownClues(currentTurnPlayer, characterId);
            bool firstPending = IsFirstCluePending(currentTurnPlayer, characterId) || (firstClueContext && knownClues.Count == 0);
            if (firstPending)
            {
                SetFirstCluePending(currentTurnPlayer, characterId, true);
                matchReportService.OnFirstCluePromptOpened(characterId);
                LogTelemetry("first_clue_prompt_opened", $"character={characterId}");
                investigationOverlayView.Show("Escolha a primeira pista", "A primeira pista é gratuita. Escolha uma categoria para receber a informação deste Dossiê.");
            }
            else
            {
                investigationOverlayView.Show("Escolha uma pista", "Escolha uma categoria ainda não revelada para este Dossiê.");
            }

            AddClueCategoryButton(characterId, ClueCategory.Area, "Área", knownClues.Contains(ClueCategory.Area));
            AddClueCategoryButton(characterId, ClueCategory.Era, "Época", knownClues.Contains(ClueCategory.Era));
            AddClueCategoryButton(characterId, ClueCategory.Region, "Região", knownClues.Contains(ClueCategory.Region));
            AddClueCategoryButton(characterId, ClueCategory.Contribution, "Contribuição", knownClues.Contains(ClueCategory.Contribution));
            AddClueCategoryButton(characterId, ClueCategory.ContextLegacy, "Contexto/Legado", knownClues.Contains(ClueCategory.ContextLegacy));
        }

        private void AddClueCategoryButton(string characterId, ClueCategory category, string label, bool alreadyKnown) => investigationOverlayView.AddButton(alreadyKnown ? $"{label} (já conhecida)" : label, () => OnClueSelected(characterId, category), !alreadyKnown);

        private void OnClueSelected(string characterId, ClueCategory category)
        {
            HashSet<ClueCategory> known = GetKnownClues(currentTurnPlayer, characterId);
            if (known.Contains(category)) return;

            bool wasFirstPending = IsFirstCluePending(currentTurnPlayer, characterId) && known.Count == 0;
            known.Add(category);
            if (wasFirstPending)
            {
                SetFirstCluePending(currentTurnPlayer, characterId, false);
                SetFirstClueChosen(currentTurnPlayer, characterId, true);
                matchReportService.OnFirstClueChosen(characterId, category);
                LogTelemetry("first_clue_chosen", $"character={characterId};category={category}");
            }

            totalCluesRequested += 1;
            matchReportService.OnClueRequested(characterId, category);
            LogTelemetry("clue_requested", $"character={characterId};category={category}");
            AudioManager.Instance?.PlayClue();
            tutorialManager?.Notify(TutorialTrigger.ClueSelected);
            RefreshPersistentIdentifyButton();
            investigationOverlayView.Show("Pista revelada", $"{GetCategoryLabel(category)}\n\n{GetClueText(characterId, category)}");
            investigationOverlayView.AddButton("Tentar identificar", () => ShowGuessOverlay(characterId, GuessSource.FreshDiscovery), IsCharacterEligibleForGuess(characterId));
            investigationOverlayView.AddButton("Ver pistas do Dossiê", () => ShowDiscoveredCharacterActions(characterId));
            investigationOverlayView.AddButton("Encerrar turno", EndTurnAfterOverlay);
        }
        private void ShowGuessOverlay(string characterId, GuessSource source)
        {
            investigationOverlayView.Show("Escolha a personagem", "Selecione um nome e confirme sua identificação.");
            foreach (CharacterData character in PrototypeDatabase.Characters) { string selectedName = character.DisplayName; investigationOverlayView.AddButton(selectedName, () => ResolveGuess(characterId, selectedName, source)); }
            investigationOverlayView.AddButton("Voltar", () => ShowDiscoveredCharacterActions(characterId));
        }
        private void ResolveGuess(string characterId, string guessedName, GuessSource source)
        {
            totalGuesses += 1;
            matchReportService.OnGuessAttemptSource(source == GuessSource.FreshDiscovery);
			
			CharacterData target = FindCharacterByCardId(characterId);
			bool correct = target != null && string.Equals(target.DisplayName, guessedName, StringComparison.Ordinal);
						
            matchReportService.OnGuess(correct, characterId);
    

            LogTelemetry("guess_made", $"character={characterId};guess={guessedName}");
            tutorialManager?.Notify(TutorialTrigger.GuessMade);
            if (correct)
            {
                LogTelemetry("guess_correct", $"character={characterId};guess={guessedName}");
                scores[currentTurnPlayer] += 1;
                identifiedCharacters[currentTurnPlayer].Add(characterId);
                MarkCharacterIdentified(characterId);
                UpdateHud();
                AudioManager.Instance?.PlayCorrect();
                CharacterData revealData = FindCharacterByCardId(characterId);
                PlayCardReveal(new CardRevealPayload
                {
                    RevealType = CardRevealType.CharacterIdentified,
                    Title = revealData != null ? revealData.DisplayName : "Identificação correta",
                    Body = revealData != null ? string.Join("\n", new[]
                    {
                        $"Área: {revealData.Area}",
                        $"Época: {revealData.Era}",
                        $"Região: {revealData.Region}",
                        $"Contribuição: {revealData.Contribution}"
                    }) : "Identificação correta!",
                    RequireTapToContinue = true,
                    Celebratory = true
                }, () =>
                {
                    if (scores[currentTurnPlayer] >= ActiveModeConfig.ObjectiveIdentifications) { CompleteMatch(currentTurnPlayer); return; }
                    EnsureInvestigationOverlayView();
                    investigationOverlayView.Show("Resultado", "Identificação correta!"); investigationOverlayView.AddButton("Encerrar turno", EndTurnAfterOverlay);
                });
                return;
            }
            matchReportService.OnRepeatedGuessAfterWrongAttempt(characterId);
            AudioManager.Instance?.PlayWrong();
            LogTelemetry("guess_wrong", $"character={characterId};guess={guessedName}"); investigationOverlayView.Show("Resultado", "Identificação incorreta."); investigationOverlayView.AddButton("Encerrar turno", EndTurnAfterOverlay);
        }

        private void EndTurnAfterOverlay() { Debug.Log("[TURN] End turn"); investigationOverlayView.Hide(); EndTurnWithPassScreen(); }
        private void EndTurnWithPassScreen() { tutorialManager?.Notify(TutorialTrigger.TurnPassed); PlayerId next = currentTurnPlayer == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne; ShowReadyScreen("Passe o aparelho para o próximo jogador", "Estou pronto", () => { currentTurnPlayer = next; Debug.Log($"[STATE] CurrentPlayer: {currentTurnPlayer}"); HideReadyScreen(); EnterInvestigationTurn(startTutorialIfNeeded: false); }); }

        private bool IsTutorialBlockingGameplayInput() => tutorialManager != null && tutorialManager.IsBlockingGameplayInput;

        private void BuildInvestigationHud()
        {
            if (hudRoot != null) Destroy(hudRoot.gameObject);
            GameObject hud = new GameObject("InvestigationHUD", typeof(RectTransform), typeof(Image));
            hudRoot = hud.GetComponent<RectTransform>();
            hudRoot.SetParent(sceneRoot.TopArea, false);
            hudRoot.anchorMin = Vector2.zero; hudRoot.anchorMax = Vector2.one;
            hudRoot.offsetMin = new Vector2(8, 6); hudRoot.offsetMax = new Vector2(-8, -6);
            hud.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.26f);

            hudCurrentPlayer = CreateHudText(hudRoot, "Jogador atual");
            hudObjective = CreateHudText(hudRoot, "Objetivo");
            hudScore = CreateHudText(hudRoot, "Placar");
            hudResearch = CreateHudText(hudRoot, "Fichas de pesquisa");
            PositionHudText(hudCurrentPlayer, new Vector2(0.02f, 0.53f), new Vector2(0.49f, 0.98f));
            PositionHudText(hudObjective, new Vector2(0.51f, 0.53f), new Vector2(0.98f, 0.98f));
            PositionHudText(hudScore, new Vector2(0.02f, 0.02f), new Vector2(0.49f, 0.47f));
            PositionHudText(hudResearch, new Vector2(0.51f, 0.02f), new Vector2(0.98f, 0.47f));

            if (hudButtonCardsRoot != null) Destroy(hudButtonCardsRoot.gameObject);
            GameObject cardRow = new GameObject("GuideRulesCards", typeof(RectTransform));
            hudButtonCardsRoot = cardRow.GetComponent<RectTransform>();
            hudButtonCardsRoot.SetParent(sceneRoot.ActionArea, false);
            hudButtonCardsRoot.anchorMin = Vector2.zero; hudButtonCardsRoot.anchorMax = Vector2.one;
            hudButtonCardsRoot.offsetMin = new Vector2(18f, 8f); hudButtonCardsRoot.offsetMax = new Vector2(-18f, -8f);
            guideCardButton = CreateInfoCardButton(hudButtonCardsRoot, "Guia", "Pesquisar personagens", "Fichas: 0", -460f, OnGuidebookButtonPressed, out guideCardTokensText);
            identifyCardButton = CreateInfoCardButton(hudButtonCardsRoot, "Tentar identificar Dossiê", "Use pistas já reveladas", "Sem dossiês elegíveis", 0f, ShowPersistentGuessTargetsOverlay, out identifyCardDetailText);
            rulesCardButton = CreateInfoCardButton(hudButtonCardsRoot, "Fluxo", "Ações do protótipo", string.Empty, 460f, ShowRulesOverlay, out _);
            tutorialManager?.RegisterTarget("guide_button", guideCardButton.GetComponent<RectTransform>());
            tutorialManager?.RegisterTarget("identify_button", identifyCardButton.GetComponent<RectTransform>());
            tutorialManager?.RegisterTarget("rules_card", rulesCardButton.GetComponent<RectTransform>());
            SetInvestigationControlsVisible(true, "BuildInvestigationHud");
        }

        private void UpdateHud()
        {
            if (hudCurrentPlayer == null) return;
            hudCurrentPlayer.text = $"Jogador atual: {(currentTurnPlayer == PlayerId.PlayerOne ? "Jogador 1" : "Jogador 2")}";
            hudObjective.text = $"Objetivo: identificar {ActiveModeConfig.ObjectiveIdentifications}";
            hudScore.text = $"Placar J1 {scores[PlayerId.PlayerOne]} x {scores[PlayerId.PlayerTwo]} J2";
            hudResearch.text = $"Fichas de Pesquisa: {researchTokens[currentTurnPlayer]}";
            if (guideCardTokensText != null)
            {
                guideCardTokensText.text = $"Fichas: {researchTokens[currentTurnPlayer]}";
            }
            RefreshPersistentIdentifyButton();
        }

        private void CompleteMatch(PlayerId winner)
        {
            SetPhase(GamePhase.End);
            SetInvestigationControlsVisible(false, "WinScreen");
            investigationOverlayView.Hide(); guidebookOverlayView.Hide(); HideReadyScreen();
            string modeLabel = ActiveModeConfig != null ? ActiveModeConfig.DisplayName : "Modo desconhecido";
            string winnerLabel = winner == PlayerId.PlayerOne ? "Jogador 1" : "Jogador 2";
            EnsureWinScreenView();
            EnsureMatchReportView();
            winScreenView.Show($"Vitória do {winnerLabel}!", $"Personagens identificados: {scores[winner]}\nModo: {modeLabel}", OpenReportFromWinScreen, RestartCurrentModeMatch, ReturnToMainMenu);
            AudioManager.Instance?.PlayWin();
            lastReportText = matchReportService.Finish(winnerLabel, scores[PlayerId.PlayerOne], scores[PlayerId.PlayerTwo]).ModeName != null ? matchReportService.BuildReadableReport() : string.Empty;
            LogTelemetry("match_finished", $"winner={winner};mode={ActiveModeConfig?.Id};duration={ActiveModeConfig?.DurationMinutes};final_scores={scores[PlayerId.PlayerOne]}-{scores[PlayerId.PlayerTwo]};total_clues_requested={totalCluesRequested};total_research_uses={totalResearchUses};total_guesses={totalGuesses}");
        }

        private void RestartCurrentModeMatch() { winScreenView.Hide(); ResetRuntimeUIState("PlayAgain"); Debug.Log("[GAME] PlayAgain reset complete"); StartPassAndPlaySetup(); }
        private void ReturnToMainMenu()
        {
            ResetRuntimeUIState("EnterMainMenu");
            winScreenView.Hide();
            if (sceneRoot?.FullScreenRoot == null) return;
            for (int i = sceneRoot.FullScreenRoot.childCount - 1; i >= 0; i--) Destroy(sceneRoot.FullScreenRoot.GetChild(i).gameObject);
            mainMenuRoot = null;
            SetPhase(GamePhase.MainMenu);
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

        private HashSet<ClueCategory> GetKnownClues(PlayerId player, string characterId) { EnsureClueStateForPlayer(player); if (!discoveredClues[player].TryGetValue(characterId, out HashSet<ClueCategory> clues)) { clues = new HashSet<ClueCategory>(); discoveredClues[player][characterId] = clues; } return clues; }
        private void EnsureClueStateForPlayer(PlayerId player) { if (!discoveredClues.ContainsKey(player)) discoveredClues[player] = new Dictionary<string, HashSet<ClueCategory>>(); if (!firstCluePendingByPlayer.ContainsKey(player)) firstCluePendingByPlayer[player] = new HashSet<string>(); if (!firstClueChosenByPlayer.ContainsKey(player)) firstClueChosenByPlayer[player] = new HashSet<string>(); }
        private bool IsFirstCluePending(PlayerId player, string characterId) { EnsureClueStateForPlayer(player); return firstCluePendingByPlayer[player].Contains(characterId); }
        private void SetFirstCluePending(PlayerId player, string characterId, bool pending) { EnsureClueStateForPlayer(player); if (pending) firstCluePendingByPlayer[player].Add(characterId); else firstCluePendingByPlayer[player].Remove(characterId); }
        private void SetFirstClueChosen(PlayerId player, string characterId, bool chosen) { EnsureClueStateForPlayer(player); if (chosen) firstClueChosenByPlayer[player].Add(characterId); else firstClueChosenByPlayer[player].Remove(characterId); }
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

        private List<string> GetDiscoveredButUnidentifiedCharacters() { EnsureClueStateForPlayer(currentTurnPlayer); List<string> characters = new List<string>(); foreach (var pair in discoveredClues[currentTurnPlayer]) if (!identifiedCharacters[currentTurnPlayer].Contains(pair.Key) && !IsFirstCluePending(currentTurnPlayer, pair.Key) && pair.Value.Count > 0) characters.Add(pair.Key); return characters; }
        private bool IsCharacterEligibleForGuess(string characterId, bool allowZeroCluesFallback = false) { if (string.IsNullOrEmpty(characterId) || identifiedCharacters[currentTurnPlayer].Contains(characterId)) return false; if (IsFirstCluePending(currentTurnPlayer, characterId)) return false; int clues = GetKnownClues(currentTurnPlayer, characterId).Count; return clues >= 1; }
        private List<string> GetEligibleGuessTargets(bool allowZeroCluesFallback = false) { List<string> list = new List<string>(); foreach (string characterId in GetDiscoveredButUnidentifiedCharacters()) if (IsCharacterEligibleForGuess(characterId, allowZeroCluesFallback)) list.Add(characterId); return list; }
        private bool HasHiddenCardsRemaining() { PlayerId owner = currentTurnPlayer == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne; foreach (PlacedCardData card in playerBoardStates[owner]) if (!card.IsInvestigated && !card.IsRevealed && !card.IsFaceUp) return true; return false; }
        private void ShowDiscoveredCharacterActions(string characterId)
        {
            HashSet<ClueCategory> known = GetKnownClues(currentTurnPlayer, characterId);
            if (IsFirstCluePending(currentTurnPlayer, characterId) || known.Count == 0)
            {
                investigationOverlayView.Show("Dossiê encontrado", "Primeira pista pendente. Escolha uma categoria para receber a primeira pista gratuita.");
                investigationOverlayView.AddButton("Escolher primeira pista", () => ShowClueSelectionOverlay(characterId, true));
                return;
            }

            investigationOverlayView.Show("Dossiê encontrado", $"Pistas reveladas: {known.Count}");
            foreach (ClueCategory c in known)
            {
                investigationOverlayView.AddButton(GetCategoryLabel(c), () =>
                {
                    investigationOverlayView.Show("Pista revelada", $"{GetCategoryLabel(c)}\n\n{GetClueText(characterId, c)}");
                    investigationOverlayView.AddButton("Voltar", () => ShowDiscoveredCharacterActions(characterId));
                });
            }
            investigationOverlayView.AddButton("Tentar identificar", () => ShowGuessOverlay(characterId, GuessSource.PersistentAction), IsCharacterEligibleForGuess(characterId));
            investigationOverlayView.AddButton("Encerrar turno", EndTurnAfterOverlay);
        }
        private void ShowPersistentGuessTargetsOverlay() { if (IsTutorialBlockingGameplayInput()) { Debug.Log("[TUTORIAL] Identify input blocked during investigation tutorial."); return; } List<string> targets = GetEligibleGuessTargets(); if (targets.Count == 0) { investigationOverlayView.Show("Identificação", "Nenhum Dossiê elegível no momento."); investigationOverlayView.AddButton("Fechar", investigationOverlayView.Hide); return; } investigationOverlayView.Show("Tentar identificar Dossiê", "Escolha um Dossiê encontrado para tentar identificar."); foreach (string characterId in targets) { int clues = GetKnownClues(currentTurnPlayer, characterId).Count; investigationOverlayView.AddButton($"Dossiê encontrado ({clues} pistas reveladas)", () => ShowGuessOverlay(characterId, GuessSource.PersistentAction)); } investigationOverlayView.AddButton("Fechar", investigationOverlayView.Hide); }
        private void RefreshPersistentIdentifyButton() { if (identifyCardButton == null) return; int eligible = GetEligibleGuessTargets().Count; bool canUse = eligible > 0; identifyCardButton.interactable = canUse; if (identifyCardDetailText != null) identifyCardDetailText.text = canUse ? $"{eligible} dossiê(s) elegível(is)" : "Sem dossiês elegíveis"; }
        private void RunTurnSoftLockSafeguard() { bool hiddenRemaining = HasHiddenCardsRemaining(); List<string> eligible = GetEligibleGuessTargets(); if (!hiddenRemaining) { matchReportService.OnNoHiddenCardsTurn(); if (eligible.Count > 0) { EnsureInvestigationOverlayView(); investigationOverlayView.Show("Aviso", "Não há mais cartas ocultas. Tente identificar um Dossiê."); investigationOverlayView.AddButton("Abrir identificação", ShowPersistentGuessTargetsOverlay); matchReportService.OnEndgameSafeguardTriggered(); } else { Debug.LogWarning("[SOFTLOCK WARNING] No hidden cards and no eligible guesses."); matchReportService.OnEndgameSafeguardTriggered(); List<string> fallback = GetEligibleGuessTargets(true); if (fallback.Count > 0) { EnsureInvestigationOverlayView(); investigationOverlayView.Show("Aviso", "Sem pistas reveladas suficientes. Identificação foi liberada para evitar soft lock."); foreach (string characterId in fallback) investigationOverlayView.AddButton("Tentar identificar Dossiê", () => ShowGuessOverlay(characterId, GuessSource.PersistentAction)); } } } if (!identificationHintShown && eligible.Count > 0) { identificationHintShown = true; EnsureInvestigationOverlayView(); investigationOverlayView.Show("Identificação disponível", "Você já tem pistas suficientes para tentar identificar um Dossiê. Pode fazer isso agora ou em um turno futuro."); investigationOverlayView.AddButton("Fechar", investigationOverlayView.Hide); } RefreshPersistentIdentifyButton(); }
        private bool TryRevealExtraClue(string characterId, out ClueCategory category, out string clueText)
        {
            HashSet<ClueCategory> known = GetKnownClues(currentTurnPlayer, characterId);
            ClueCategory[] order = { ClueCategory.Area, ClueCategory.Era, ClueCategory.Region, ClueCategory.Contribution, ClueCategory.ContextLegacy };
            foreach (ClueCategory c in order) { if (known.Contains(c)) continue; known.Add(c); category = c; clueText = GetClueText(characterId, c); return true; }
            category = ClueCategory.Area; clueText = string.Empty; return false;
        }
        private void ShowNoValidTargetArchiveEffect(string effectName, string cardId) { investigationOverlayView.Show("Efeito de Arquivo", "Sem alvo válido. A investigação continua."); investigationOverlayView.AddButton("Continuar", EndTurnAfterOverlay); matchReportService.OnArchiveResolution(false); LogTelemetry("archive_effect_resolved", $"card={cardId};effect={effectName};result=sem_alvo_valido"); }

        private void OnGuidebookButtonPressed()
        {
            if (IsTutorialBlockingGameplayInput())
            {
                Debug.Log("[TUTORIAL] Guide input blocked during investigation tutorial.");
                return;
            }
            EnsureInvestigationOverlayView();
            if (researchTokens[currentTurnPlayer] <= 0) { investigationOverlayView.Show("Guia de Apoio", "Você não tem Fichas de Pesquisa restantes."); investigationOverlayView.AddButton("Fechar", investigationOverlayView.Hide); return; }
            investigationOverlayView.Show("Guia de Apoio", "Gastar 1 Ficha de Pesquisa para abrir o guia?");
            investigationOverlayView.AddButton("Confirmar", () => { researchTokens[currentTurnPlayer] = Mathf.Max(0, researchTokens[currentTurnPlayer] - 1); totalResearchUses += 1; matchReportService.OnGuidebookUse(currentTurnPlayer); AudioManager.Instance?.PlayResearch(); UpdateHud(); investigationOverlayView.Hide(); ShowGuidebookOverlay(); });
            investigationOverlayView.AddButton("Cancelar", investigationOverlayView.Hide);
        }
        private void ShowGuidebookOverlay() { tutorialManager?.Notify(TutorialTrigger.GuideOpened); EnsureGuidebookOverlayView(); guidebookOverlayView.Show(PrototypeDatabase.Characters, researchTokens[currentTurnPlayer]); UpdateTutorialLayoutContext(); }
        private void ShowRulesOverlay() { if (IsTutorialBlockingGameplayInput()) { Debug.Log("[TUTORIAL] Rules input blocked during investigation tutorial."); return; } EnsureInvestigationOverlayView(); investigationOverlayView.Show("Fluxo do Protótipo", "Use este lembrete apenas para interações digitais:\n\n1. Toque em cartas para analisá-las e posicioná-las no arquivo.\n2. Toque em Finalizar montagem.\n3. Toque em cartas ocultas no tabuleiro para revelar.\n4. Use o botão Guia para consultar personagens.\n5. Ao encontrar um Dossiê, escolha uma primeira pista gratuita. Depois de revelar pelo menos uma pista, use Tentar identificar.\n6. Encerre o turno para passar o aparelho."); investigationOverlayView.AddButton("Fechar", investigationOverlayView.Hide); }

        private void HideSetupSensitiveUi()
        {
            if (trayRoot != null) trayRoot.gameObject.SetActive(false);
            if (placedActionsRoot != null) placedActionsRoot.gameObject.SetActive(false);
            focusCardView?.Hide();
            if (setupPlacementInstructionRoot != null) setupPlacementInstructionRoot.gameObject.SetActive(false);
            CanvasGroup cg = sceneRoot.CenterBoardArea.GetComponent<CanvasGroup>(); if (cg == null) cg = sceneRoot.CenterBoardArea.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        private void StorePlacedCardForCurrentPlayer(PlacedCardData placed) { playerBoardStates[currentSetupPlayer].RemoveAll(c => c.CardId == placed.CardId); playerBoardStates[currentSetupPlayer].Add(new PlacedCardData { CardId = placed.CardId, CardType = placed.CardType, Owner = placed.Owner, Coordinate = placed.Coordinate, IsFaceUp = placed.IsFaceUp, IsInvestigated = placed.IsInvestigated, IsRevealed = placed.IsRevealed, IsIdentified = placed.IsIdentified, EffectResolved = placed.EffectResolved }); }
        private void RemoveStoredPlacedCardForCurrentPlayer(string cardId) => playerBoardStates[currentSetupPlayer].RemoveAll(c => c.CardId == cardId);
        private void PersistInvestigatedCard(PlacedCardData updated)
        {
            if (!playerBoardStates.TryGetValue(updated.Owner, out List<PlacedCardData> cards)) return;
            int idx = cards.FindIndex(c => c.CardId == updated.CardId);
            if (idx < 0) return;
            cards[idx].IsInvestigated = updated.IsInvestigated;
            cards[idx].IsRevealed = updated.IsRevealed;
            cards[idx].IsFaceUp = updated.IsFaceUp;
            cards[idx].IsIdentified = updated.IsIdentified;
            cards[idx].EffectResolved = updated.EffectResolved;
        }
        private void MarkCharacterIdentified(string characterCardId)
        {
            PlayerId owner = currentTurnPlayer == PlayerId.PlayerOne ? PlayerId.PlayerTwo : PlayerId.PlayerOne;
            if (!playerBoardStates.TryGetValue(owner, out List<PlacedCardData> cards)) return;
            int idx = cards.FindIndex(c => c.CardId == characterCardId);
            if (idx < 0) return;
            cards[idx].IsIdentified = true;
            cards[idx].IsRevealed = true;
            cards[idx].IsFaceUp = true;
        }
        private IEnumerator FillEmptyCellsWithNoRecordCoroutine(PlayerId owner, Action onCompleted)
        {
            isAutoFillingNoRecord = true;
            tutorialManager?.Notify(TutorialTrigger.AutoNoRecordFillStarted);
            if (finalizeSetupButton != null) finalizeSetupButton.interactable = false;
            SetSetupInteractionEnabled(false);

            List<Vector2Int> emptyCells = CollectEmptyBoardCells();
            Debug.Log($"[NO_RECORD_FILL] Starting auto-fill for Player {owner}. Empty cells: {emptyCells.Count}");

            GameObject labelObj = CreateNoRecordFillLabel("Preenchendo o arquivo com cartas Sem Registro...");
            TextMeshProUGUI fillLabel = labelObj != null ? labelObj.GetComponentInChildren<TextMeshProUGUI>() : null;
            float started = Time.realtimeSinceStartup;
            int generated = 0;
            int semIndex = 0;

            foreach (Vector2Int coord in emptyCells)
            {
                yield return AnimateNoRecordSpawn(coord);

                var noRecord = new PlacedCardData
                {
                    CardId = $"{owner}_sem_registro_auto_{semIndex++}",
                    CardType = CardType.SemRegistro,
                    Owner = owner,
                    Coordinate = coord,
                    IsFaceUp = false,
                    IsInvestigated = false,
                    IsRevealed = false,
                    IsIdentified = false,
                    EffectResolved = false
                };
                boardController.PlaceCard(noRecord);
                StorePlacedCardForCurrentPlayer(noRecord);
                generated++;
                Debug.Log($"[NO_RECORD_FILL] Placed Sem Registro at cell {coord.x},{coord.y}");
                yield return new WaitForSeconds(0.05f);
            }

            float duration = Time.realtimeSinceStartup - started;
            int expectedNoRecord = (ActiveModeConfig.BoardSize.x * ActiveModeConfig.BoardSize.y) - ActiveModeConfig.CharactersPerPlayer - ActiveModeConfig.ArchiveCardsPerPlayer;
            if (expectedNoRecord != generated)
            {
                Debug.LogWarning($"[NO_RECORD_FILL] Expected {expectedNoRecord} Sem Registro, generated {generated}.");
            }
            matchReportService.OnAutoNoRecordGenerated(owner, generated);
            matchReportService.OnAutoFillDuration(duration);
            Debug.Log($"[NO_RECORD_FILL] Completed auto-fill for Player {owner} in {duration:F2} seconds");

            if (fillLabel != null) fillLabel.text = "Arquivo completo.";
            tutorialOverlay?.Hide();
            tutorialManager?.Notify(TutorialTrigger.AutoNoRecordFillCompleted);
            yield return new WaitForSeconds(0.65f);
            if (labelObj != null) Destroy(labelObj);

            isAutoFillingNoRecord = false;
            SetSetupInteractionEnabled(true);
            onCompleted?.Invoke();
        }

        private List<Vector2Int> CollectEmptyBoardCells()
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            for (int y = 0; y < ActiveModeConfig.BoardSize.y; y++)
            {
                for (int x = 0; x < ActiveModeConfig.BoardSize.x; x++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    if (boardController.GetPlacedCard(coord) == null) cells.Add(coord);
                }
            }
            return cells;
        }

        private IEnumerator AnimateNoRecordSpawn(Vector2Int coord)
        {
            RectTransform overlay = sceneRoot?.OverlayLayer;
            if (overlay == null) yield break;

            GameObject fly = new GameObject("NoRecordFlyCard", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            RectTransform rect = fly.GetComponent<RectTransform>();
            rect.SetParent(overlay, false);
            rect.sizeDelta = new Vector2(88f, 120f);
            fly.GetComponent<Image>().color = new Color(0.2f, 0.27f, 0.36f, 1f);
            CanvasGroup cg = fly.GetComponent<CanvasGroup>();
            cg.alpha = 0f;

            GameObject q = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            q.transform.SetParent(rect, false);
            var qt = q.GetComponent<TextMeshProUGUI>();
            qt.text = "?"; qt.alignment = TextAlignmentOptions.Center; qt.fontSize = 40; qt.color = Color.white;
            var qrt = q.GetComponent<RectTransform>();
            qrt.anchorMin = Vector2.zero; qrt.anchorMax = Vector2.one; qrt.offsetMin = Vector2.zero; qrt.offsetMax = Vector2.zero;

            Vector2 source = GetNoRecordSpawnSourceInOverlay();
            Vector2 target = GetBoardCellCenterInOverlay(coord);
            rect.anchoredPosition = source;
            rect.localScale = Vector3.one * 0.75f;

            float elapsed = 0f; const float duration = 0.22f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(source, target, t);
                rect.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, t);
                cg.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            Destroy(fly);
        }

        private Vector2 GetNoRecordSpawnSourceInOverlay()
        {
            RectTransform overlay = sceneRoot.OverlayLayer;
            RectTransform tray = trayRoot != null ? trayRoot : sceneRoot.BottomCardTray;
            if (tray == null) return new Vector2(0f, -720f);

            Vector3 world = tray.TransformPoint(new Vector3(0f, tray.rect.height * 0.5f, 0f));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, RectTransformUtility.WorldToScreenPoint(null, world), null, out Vector2 local);
            return local;
        }

        private Vector2 GetBoardCellCenterInOverlay(Vector2Int coord)
        {
            RectTransform overlay = sceneRoot.OverlayLayer;
            if (boardController.TryGetCellRectTransform(coord, out RectTransform cellRect))
            {
                Vector3 world = cellRect.TransformPoint(cellRect.rect.center);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, RectTransformUtility.WorldToScreenPoint(null, world), null, out Vector2 local);
                return local;
            }
            return Vector2.zero;
        }

        private GameObject CreateNoRecordFillLabel(string text)
        {
            RectTransform overlay = sceneRoot?.OverlayLayer;
            if (overlay == null) return null;

            GameObject panel = new GameObject("NoRecordFillLabel", typeof(RectTransform), typeof(Image));
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.SetParent(overlay, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(860f, 130f);
            rect.anchoredPosition = new Vector2(0f, 420f);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

            GameObject labelObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform lrt = labelObj.GetComponent<RectTransform>();
            lrt.SetParent(rect, false);
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = new Vector2(22f, 12f); lrt.offsetMax = new Vector2(-22f, -12f);
            TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 34;
            label.color = Color.white;
            label.enableWordWrapping = true;

            return panel;
        }

        private void SetSetupInteractionEnabled(bool enabled)
        {
            if (trayRoot != null)
            {
                CanvasGroup trayCg = trayRoot.GetComponent<CanvasGroup>();
                if (trayCg == null) trayCg = trayRoot.gameObject.AddComponent<CanvasGroup>();
                trayCg.interactable = enabled;
                trayCg.blocksRaycasts = enabled;
            }

            if (placedActionsRoot != null)
            {
                CanvasGroup actionsCg = placedActionsRoot.GetComponent<CanvasGroup>();
                if (actionsCg == null) actionsCg = placedActionsRoot.gameObject.AddComponent<CanvasGroup>();
                actionsCg.interactable = enabled;
                actionsCg.blocksRaycasts = enabled;
            }

            if (sceneRoot?.CenterBoardArea != null)
            {
                CanvasGroup boardCg = sceneRoot.CenterBoardArea.GetComponent<CanvasGroup>();
                if (boardCg == null) boardCg = sceneRoot.CenterBoardArea.gameObject.AddComponent<CanvasGroup>();
                boardCg.interactable = enabled;
                boardCg.blocksRaycasts = enabled;
            }
        }



        private void ResetRuntimeUIState(string reason)
        {
            Debug.Log($"[UI_STATE] ResetRuntimeUIState reason={reason}");
            SetInvestigationControlsVisible(false, reason);

            selectedCardForInspection = null;
            selectedCardForPlacement = null;
            selectedPlacedCoordinate = null;
            isInspectingCard = false;
            isPlacementModeActive = false;
            setupReadyToFinalize = false;
            isAutoFillingNoRecord = false;
            isRevealAnimationPlaying = false;
            identificationHintShown = false;
            currentSetupPlayer = PlayerId.PlayerOne;
            currentTurnPlayer = PlayerId.PlayerOne;

            focusCardView?.Hide();
            cardRevealOverlayView?.Hide();
            guidebookOverlayView?.Hide();
            investigationOverlayView?.Hide();
            tutorialOverlay?.Hide();
            matchReportView?.Hide();
            winScreenView?.Hide();
            readyScreenView?.Hide();

            boardController?.SetPlacementHighlights(false);
            boardController?.SetSelectedCoordinate(null);

            if (setupPlacementInstructionRoot != null) setupPlacementInstructionRoot.gameObject.SetActive(false);
            if (trayRoot != null) trayRoot.gameObject.SetActive(false);
            if (placedActionsRoot != null) placedActionsRoot.gameObject.SetActive(false);

            if (hudRoot != null)
            {
                Destroy(hudRoot.gameObject);
                hudRoot = null;
            }
            if (hudButtonCardsRoot != null)
            {
                Destroy(hudButtonCardsRoot.gameObject);
                hudButtonCardsRoot = null;
            }

            guideCardButton = null;
            identifyCardButton = null;
            rulesCardButton = null;
            guideCardTokensText = null;
            identifyCardDetailText = null;
            hudCurrentPlayer = null;
            hudObjective = null;
            hudScore = null;
            hudResearch = null;

            playerBoardStates.Clear();
            identifiedCharacters.Clear();
            scores.Clear();
            researchTokens.Clear();
            discoveredClues.Clear();
            firstCluePendingByPlayer.Clear();
            firstClueChosenByPlayer.Clear();
            totalCluesRequested = 0;
            totalResearchUses = 0;
            totalGuesses = 0;
            lastReportText = string.Empty;

            UpdateTutorialLayoutContext();
        }

        private void SetInvestigationControlsVisible(bool visible, string reason)
        {
            bool shouldShow = visible && CurrentPhase == GamePhase.Investigation;
            Debug.Log($"[UI_STATE] InvestigationControls visible={shouldShow.ToString().ToLowerInvariant()} reason={reason}");

            SetUiRootVisible(hudRoot, shouldShow);
            SetUiRootVisible(hudButtonCardsRoot, shouldShow);

            SetButtonVisible(guideCardButton, shouldShow);
            SetButtonVisible(identifyCardButton, shouldShow);
            SetButtonVisible(rulesCardButton, shouldShow);
        }

        private static void SetUiRootVisible(RectTransform root, bool visible)
        {
            if (root == null) return;
            root.gameObject.SetActive(visible);
            CanvasGroup cg = root.GetComponent<CanvasGroup>();
            if (cg == null) cg = root.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button == null) return;
            button.gameObject.SetActive(visible);
            button.interactable = visible;
            CanvasGroup cg = button.GetComponent<CanvasGroup>();
            if (cg == null) cg = button.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }

        private void PlayCardReveal(CardRevealPayload payload, Action onComplete)
        {
            EnsureCardRevealOverlayView();
            isRevealAnimationPlaying = true;
            UpdateTutorialLayoutContext();
            cardRevealOverlayView.PlayReveal(payload, () =>
            {
                isRevealAnimationPlaying = false;
                UpdateTutorialLayoutContext();
                tutorialOverlay?.RestoreAfterActionIfStillActive();
                onComplete?.Invoke();
            });
        }

        private void UpdateTutorialLayoutContext()
        {
            bool focusActive = (focusCardView != null && focusCardView.IsVisible) || isInspectingCard;
            bool guideActive = guidebookOverlayView != null && guidebookOverlayView.IsVisible;
            bool revealActive = (cardRevealOverlayView != null && cardRevealOverlayView.IsVisible) || isRevealAnimationPlaying;
            tutorialOverlay?.SetLayoutContext(focusActive, isPlacementModeActive, guideActive, revealActive);
        }

        private void EnsureBoardController() { if (boardController != null) return; GameObject boardObject = new GameObject("BoardController", typeof(RectTransform)); boardObject.transform.SetParent(sceneRoot.CenterBoardArea, false); boardController = boardObject.AddComponent<BoardController>(); }
        private void EnsureCardRevealOverlayView() { if (cardRevealOverlayView != null) return; GameObject go = new GameObject("CardRevealOverlayView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); cardRevealOverlayView = go.AddComponent<CardRevealOverlayView>(); cardRevealOverlayView.Initialize(sceneRoot.OverlayLayer); }
        private void EnsureFocusCardView() { if (focusCardView != null) return; GameObject go = new GameObject("FocusCardView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); focusCardView = go.AddComponent<FocusCardView>(); focusCardView.Initialize(sceneRoot.OverlayLayer); }
        private void EnsureTutorialOverlay() { if (tutorialOverlay != null) return; Debug.Log("[UI] Creating TutorialOverlayView"); GameObject overlayObject = new GameObject("TutorialOverlayView"); overlayObject.transform.SetParent(sceneRoot.OverlayLayer, false); tutorialOverlay = overlayObject.AddComponent<TutorialOverlayView>(); tutorialOverlay.Initialize(sceneRoot.OverlayLayer);
            tutorialManager = new TutorialManager(tutorialOverlay); }
        private void EnsureInvestigationOverlayView() { if (investigationOverlayView != null) return; Debug.Log("[UI] Creating InvestigationOverlayView"); GameObject go = new GameObject("InvestigationOverlayView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); investigationOverlayView = go.AddComponent<InvestigationOverlayView>(); investigationOverlayView.Initialize(sceneRoot.OverlayLayer); }
        private void EnsureGuidebookOverlayView() { if (guidebookOverlayView != null) return; Debug.Log("[UI] Creating GuidebookOverlayView"); GameObject go = new GameObject("GuidebookOverlayView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); guidebookOverlayView = go.AddComponent<GuidebookOverlayView>(); guidebookOverlayView.Initialize(sceneRoot.OverlayLayer); guidebookOverlayView.OnVisibilityChanged += UpdateTutorialLayoutContext; }
        private void EnsureWinScreenView() { if (winScreenView != null) return; Debug.Log("[UI] Creating WinScreenView"); GameObject go = new GameObject("WinScreenView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); winScreenView = go.AddComponent<WinScreenView>(); winScreenView.Initialize(sceneRoot.OverlayLayer); }
        private void EnsureMatchReportView() { if (matchReportView != null) return; Debug.Log("[UI] Creating MatchReportView"); GameObject go = new GameObject("MatchReportView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); matchReportView = go.AddComponent<MatchReportView>(); matchReportView.Initialize(sceneRoot.OverlayLayer); }
        private void OpenReportFromWinScreen() { EnsureMatchReportView(); winScreenView.Hide(); matchReportView.Show(lastReportText, () => { matchReportView.Hide(); winScreenView.Show("Resultado", "Veja o resumo final", OpenReportFromWinScreen, RestartCurrentModeMatch, ReturnToMainMenu); }); }
        private void EnsureReadyScreenView() { if (readyScreenView != null) return; GameObject go = new GameObject("ReadyScreenView"); go.transform.SetParent(sceneRoot.OverlayLayer, false); readyScreenView = go.AddComponent<ReadyScreenView>(); readyScreenView.Initialize(sceneRoot.OverlayLayer); }
        private void ShowReadyScreen(string message, string buttonText, Action onConfirm) { SetInvestigationControlsVisible(false, "ReadyScreen"); EnsureReadyScreenView(); readyScreenView.Show(message, buttonText, onConfirm); }
        private void HideReadyScreen() => readyScreenView?.Hide();
        private void HideAllScreensAndGameplayUi()
        {
            ResetRuntimeUIState("HideAllScreens");
            if (sceneRoot?.CenterBoardArea != null) sceneRoot.CenterBoardArea.gameObject.SetActive(false);
        }

        private static void DisableLegacyManualSceneObjects()
        {
            string[] legacyNames = { "GuidebookView", "GuidebookOverlayView", "Guia de Apoio", "TutorialOverlayView", "GameController", "Canvas", "EventSystem" };
            foreach (string name in legacyNames)
            {
                GameObject[] matches = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                int activeCount = 0;
                foreach (GameObject go in matches)
                {
                    if (!string.Equals(go.name, name, StringComparison.Ordinal)) continue;
                    activeCount++;
                }
                if (activeCount > 1)
                {
                    Debug.LogWarning($"[Bootstrap] Potential duplicate object detected: {name} ({activeCount})");
                }
            }
        }

        private static Button CreateActionButton(RectTransform parent, string text, Action onClick)
        {
            GameObject go = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false); go.GetComponent<LayoutElement>().preferredHeight = 88f; go.GetComponent<Image>().color = new Color(0.16f, 0.43f, 0.84f, 1f);
            Button b = go.GetComponent<Button>(); b.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); onClick?.Invoke(); });
            GameObject label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)); RectTransform lr = label.GetComponent<RectTransform>(); lr.SetParent(go.transform, false); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            TextMeshProUGUI t = label.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = 28;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            return b;
        }

        private static void CreateTitle(RectTransform parent, string textValue)
        {
            GameObject titleObj = new GameObject("MainMenuTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(parent, false);
            RectTransform rect = titleObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.08f, 0.74f);
            rect.anchorMax = new Vector2(0.92f, 0.92f);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = titleObj.GetComponent<TextMeshProUGUI>();
            text.text = textValue;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 54;
            text.color = Color.white;
            text.enableWordWrapping = true;
        }

        private static void CreateFooter(RectTransform parent, string textValue)
        {
            GameObject footerObj = new GameObject("MainMenuFooter", typeof(RectTransform), typeof(TextMeshProUGUI));
            footerObj.transform.SetParent(parent, false);
            RectTransform rect = footerObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.08f, 0.06f);
            rect.anchorMax = new Vector2(0.92f, 0.18f);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = footerObj.GetComponent<TextMeshProUGUI>();
            text.text = textValue;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = new Color(0.84f, 0.88f, 0.92f, 1f);
            text.enableWordWrapping = true;
        }

        private static TextMeshProUGUI CreateHudText(RectTransform parent, string value)
        {
            GameObject go = new GameObject("HudText", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            t.fontSize = 28;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.enableWordWrapping = true;
            t.text = value;
            return t;
        }
        private static void PositionHudText(TextMeshProUGUI text, Vector2 min, Vector2 max)
        {
            RectTransform rect = text.rectTransform;
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }
        private static void PositionActionButton(RectTransform rect, float x, float width)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, 84f);
            rect.anchoredPosition = new Vector2(x, 0f);
        }
        private Button CreateInfoCardButton(RectTransform parent, string title, string subtitle, string detailText, float xOffset, Action onClick, out TextMeshProUGUI detailLabel)
        {
            GameObject go = new GameObject($"{title}Card", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(430f, 150f);
            rect.anchoredPosition = new Vector2(xOffset, 0f);
            Image bg = go.GetComponent<Image>();
            bg.color = new Color(0.9f, 0.92f, 0.96f, 0.98f);
            Button b = go.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); onClick?.Invoke(); });
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.2f);
            outline.effectDistance = new Vector2(2f, -2f);
            TextMeshProUGUI t1 = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            t1.rectTransform.SetParent(rect, false); t1.rectTransform.anchorMin = new Vector2(0.06f, 0.56f); t1.rectTransform.anchorMax = new Vector2(0.94f, 0.94f); t1.rectTransform.offsetMin = Vector2.zero; t1.rectTransform.offsetMax = Vector2.zero;
            t1.text = title; t1.fontSize = 40; t1.color = new Color(0.12f, 0.12f, 0.14f, 1f); t1.alignment = TextAlignmentOptions.MidlineLeft;
            TextMeshProUGUI t2 = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            t2.rectTransform.SetParent(rect, false); t2.rectTransform.anchorMin = new Vector2(0.06f, 0.24f); t2.rectTransform.anchorMax = new Vector2(0.94f, 0.54f); t2.rectTransform.offsetMin = Vector2.zero; t2.rectTransform.offsetMax = Vector2.zero;
            t2.text = subtitle; t2.fontSize = 24; t2.color = new Color(0.2f, 0.24f, 0.29f, 1f); t2.alignment = TextAlignmentOptions.MidlineLeft;
            detailLabel = new GameObject("Detail", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            detailLabel.rectTransform.SetParent(rect, false); detailLabel.rectTransform.anchorMin = new Vector2(0.06f, 0.04f); detailLabel.rectTransform.anchorMax = new Vector2(0.94f, 0.22f); detailLabel.rectTransform.offsetMin = Vector2.zero; detailLabel.rectTransform.offsetMax = Vector2.zero;
            detailLabel.text = detailText;
            detailLabel.fontSize = 20;
            detailLabel.color = new Color(0.35f, 0.38f, 0.45f, 1f);
            detailLabel.alignment = TextAlignmentOptions.MidlineLeft;
            return b;
        }

        private static List<HowToPlayPageData> BuildHowToPlayPages()
        {
            return new List<HowToPlayPageData>
            {
                new HowToPlayPageData { PageId = "p1", Title = "O que é o jogo?", Body = "Este é um jogo de investigação e dedução sobre personagens acadêmicos e científicos.\n\nSeu objetivo é descobrir quais personagens estão escondidos no arquivo do outro jogador usando pistas, pesquisa e lógica." },
                new HowToPlayPageData { PageId = "p2", Title = "As cartas do jogo", Body = "O jogo possui 3 tipos de carta:\n\n• Dossiês de Personagem\n• Cartas de Arquivo\n• Cartas Sem Registro\n\nCada tipo tem uma função diferente durante a investigação." },
                new HowToPlayPageData { PageId = "p3", Title = "Preparando o tabuleiro", Body = "Cada jogador monta um arquivo secreto usando 9 cartas viradas para baixo em uma grade 3x3." },
                new HowToPlayPageData { PageId = "p4", Title = "O que existe no arquivo?", Body = "Na versão rápida 3x3, cada arquivo possui:\n\n• 3 Dossiês de Personagem\n• 3 Cartas de Arquivo\n• 3 cartas Sem Registro" },
                new HowToPlayPageData { PageId = "p5", Title = "Cartas escondidas", Body = "Enquanto estão viradas para baixo, todas as cartas parecem iguais.\n\nOs jogadores não sabem onde estão os personagens, arquivos ou Sem Registro." },
                new HowToPlayPageData { PageId = "p6", Title = "Começando a partida", Body = "Depois que os dois jogadores terminam seus arquivos, a investigação começa.\n\nEm cada turno, um jogador escolhe uma carta do arquivo adversário." },
                new HowToPlayPageData { PageId = "p7", Title = "Sem Registro", Body = "Se a carta escolhida for Sem Registro, nada acontece.\n\nA carta permanece revelada e o turno passa para o próximo jogador." },
                new HowToPlayPageData { PageId = "p8", Title = "Cartas de Arquivo", Body = "Cartas de Arquivo ativam efeitos especiais que ajudam na investigação.\n\nAlgumas revelam pistas extras.\nOutras permitem novas ações ou ajudam na pesquisa." },
                new HowToPlayPageData { PageId = "p9", Title = "Encontrando um personagem", Body = "Se um Dossiê for encontrado, você escolhe uma primeira pista gratuitamente. Depois de revelar pelo menos uma pista, já pode tentar identificar.\n\nO nome nunca pode ser revelado diretamente." },
                new HowToPlayPageData { PageId = "p10", Title = "Tipos de pista", Body = "As pistas podem revelar informações como:\n\n• Área de atuação\n• Época\n• Região\n• Contribuições\n• Contexto histórico" },
                new HowToPlayPageData { PageId = "p11", Title = "Fichas de Pesquisa", Body = "Os jogadores possuem Fichas de Pesquisa que permitem consultar o Guia de Apoio." },
                new HowToPlayPageData { PageId = "p12", Title = "Guia de Apoio", Body = "O Guia de Apoio funciona como um pequeno códice com mini biografias dos personagens do jogo.\n\nEle ajuda os jogadores a relacionar pistas com personagens reais." },
                new HowToPlayPageData { PageId = "p13", Title = "Consulta limitada", Body = "O Guia de Apoio não pode ser consultado livremente por muito tempo.\n\nA ideia é fazer pesquisas rápidas para manter o ritmo da partida." },
                new HowToPlayPageData { PageId = "p14", Title = "Como vencer", Body = "Vence o jogador que identificar a quantidade necessária de personagens antes do adversário." }
            };
        }

        private void SetPhase(GamePhase newPhase)
        {
            GamePhase previous = CurrentPhase;
            CurrentPhase = newPhase;
            Debug.Log($"[STATE] {previous} -> {newPhase}");
            Debug.Log($"[STATE] CurrentPlayer: {currentTurnPlayer}");
            Debug.Log($"[STATE] SelectedMode: {ActiveModeConfig?.Id ?? "none"}");
        }

        private void RestoreBoardVisualState()
        {
            if (sceneRoot?.CenterBoardArea == null) return;
            CanvasGroup cg = sceneRoot.CenterBoardArea.GetComponent<CanvasGroup>();
            if (cg == null) return;
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }
}
