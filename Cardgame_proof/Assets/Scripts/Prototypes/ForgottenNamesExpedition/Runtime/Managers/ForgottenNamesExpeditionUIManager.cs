using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardgameProof.App;
using CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime.Data;

namespace CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime.Managers
{
    public sealed class ForgottenNamesExpeditionUIManager
    {
        private const string PrototypeTitle = "A Expedição dos Nomes Esquecidos";
        private const string PrototypeSubtitle = "Um jogo narrativo sobre ciência, memória e nomes que quase esquecemos.";
        private const string PrototypeDescription = "Em 15 minutos, crie uma pequena expedição, encontre figuras científicas pouco conhecidas e decida como lembrar suas contribuições.";
        private const string QuestionHelperText = "Você pode responder com uma frase. Se quiser, elabore com mais detalhes.";
        private const string ScientistHelperText = "As duas escolhas são válidas: Party ajuda agora, Archive será lembrado depois.";
        private const string ChallengeHelperText = "Escolha alguém da Party para ajudar. Se não houver combinação perfeita, improvise uma solução.";
        private const string FinalHelperText = "Cada jogador responde. Depois, o grupo completa a frase final.";

        private PrototypeRuntimeContext context;
        private ForgottenNamesExpeditionState state;
        private GameObject root;
        private GameObject modalOverlay;
        private bool sessionStarted;

        public void Initialize(PrototypeRuntimeContext runtimeContext, ForgottenNamesExpeditionState initialState)
        {
            context = runtimeContext;
            state = initialState ?? new ForgottenNamesExpeditionState();
            if (context?.SceneRoot?.FullScreenRoot == null)
            {
                Debug.LogWarning("[ForgottenNamesExpedition] UI initialization failed: missing scene root.");
                return;
            }

            ShowRootScreen();
        }

        public void Cleanup()
        {
            CloseModal();
            if (root != null)
            {
                root.SetActive(false);
                Object.Destroy(root);
                root = null;
            }

            state = null;
            context = null;
        }

        private void ShowRootScreen()
        {
            EnsureRoot();
            RectTransform screen = BeginScreen();

            CreatePanel(screen, "HeroPanel", new Vector2(0.07f, 0.56f), new Vector2(0.93f, 0.92f), new Color(0.10f, 0.13f, 0.20f, 0.96f));
            CreateText(screen, PrototypeTitle, 48, new Vector2(0.10f, 0.79f), new Vector2(0.90f, 0.90f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, PrototypeSubtitle, 28, new Vector2(0.12f, 0.69f), new Vector2(0.88f, 0.78f), FontStyles.Italic, TextAlignmentOptions.Center);
            CreateText(screen, PrototypeDescription, 27, new Vector2(0.11f, 0.57f), new Vector2(0.89f, 0.68f), FontStyles.Normal, TextAlignmentOptions.Center);

            CreateButton(screen, "Iniciar teste rápido", new Vector2(0.5f, 0.42f), ShowPremiseSelection);
            CreateButton(screen, "Como jogar", new Vector2(0.5f, 0.31f), ShowHowToPlayModal);
            CreateButton(screen, "Guia de Campo", new Vector2(0.5f, 0.20f), ShowFieldGuideScreen);
            CreateButton(screen, "Voltar", new Vector2(0.5f, 0.09f), ReturnToSelector, new Vector2(640f, 86f), SecondaryButtonColor);
        }

        private void ShowPremiseSelection()
        {
            RectTransform screen = BeginScreen();
            CreateText(screen, "Escolha a premissa", 50, new Vector2(0.08f, 0.85f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, "A premissa dá o tom da expedição de 15 minutos.", 26, new Vector2(0.10f, 0.78f), new Vector2(0.90f, 0.84f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);

            IReadOnlyList<ForgottenNamesPremise> premises = ForgottenNamesExpeditionContent.Premises;
            for (int i = 0; i < premises.Count; i++)
            {
                int premiseIndex = i;
                float centerY = 0.66f - (i * 0.19f);
                CreatePanel(screen, $"PremiseCard_{i}", new Vector2(0.07f, centerY - 0.075f), new Vector2(0.93f, centerY + 0.075f), CardColor);
                CreateText(screen, premises[i].Title, 30, new Vector2(0.11f, centerY + 0.015f), new Vector2(0.86f, centerY + 0.065f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
                CreateText(screen, premises[i].Description, 21, new Vector2(0.11f, centerY - 0.060f), new Vector2(0.86f, centerY + 0.010f), FontStyles.Normal, TextAlignmentOptions.TopLeft, MutedTextColor);
                CreateButton(screen, "Escolher", new Vector2(0.80f, centerY), () => SelectPremise(premiseIndex), new Vector2(210f, 74f), PrimaryButtonColor, 24);
            }

            CreateButton(screen, "Voltar", new Vector2(0.5f, 0.08f), ShowRootScreen, new Vector2(560f, 82f), SecondaryButtonColor);
        }

        private void SelectPremise(int premiseIndex)
        {
            state.SelectPremise(premiseIndex);
            ShowPlayerSetup();
        }

        private void ShowPlayerSetup()
        {
            RectTransform screen = BeginScreen();
            CreateText(screen, "Jogadores e papéis", 50, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.93f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, state.SelectedPremise.Title, 29, new Vector2(0.10f, 0.77f), new Vector2(0.90f, 0.83f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(screen, "Escolha a quantidade de jogadores. Os papéis são atribuídos em ordem para orientar as falas.", 24, new Vector2(0.11f, 0.68f), new Vector2(0.89f, 0.76f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);

            CreatePlayerCountButton(screen, 2, new Vector2(0.20f, 0.61f));
            CreatePlayerCountButton(screen, 3, new Vector2(0.40f, 0.61f));
            CreatePlayerCountButton(screen, 4, new Vector2(0.60f, 0.61f));
            CreatePlayerCountButton(screen, 5, new Vector2(0.80f, 0.61f));
            CreatePlayerCountButton(screen, 6, new Vector2(0.50f, 0.52f));

            CreateRoleList(screen, new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.49f));
            CreateButton(screen, "Começar tutorial", new Vector2(0.5f, 0.12f), ShowTutorialScreen);
            CreateButton(screen, "Trocar premissa", new Vector2(0.5f, 0.045f), ShowPremiseSelection, new Vector2(560f, 68f), SecondaryButtonColor, 24);
        }

        private void CreatePlayerCountButton(RectTransform screen, int playerCount, Vector2 anchor)
        {
            CreateButton(screen, playerCount + " jogadores", anchor, () => UpdatePlayerCount(playerCount), new Vector2(205f, 72f), state.PlayerCount == playerCount ? PrimaryButtonColor : SecondaryButtonColor, 21);
        }

        private void UpdatePlayerCount(int playerCount)
        {
            state.SetPlayerCount(playerCount);
            ShowPlayerSetup();
        }

        private void ShowTutorialScreen()
        {
            RectTransform screen = BeginScreen();
            CreateText(screen, "Tutorial rápido", 52, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.92f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, "Quatro regras para guiar a mesa", 28, new Vector2(0.10f, 0.75f), new Vector2(0.90f, 0.81f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);

            IReadOnlyList<string> rules = ForgottenNamesExpeditionContent.TutorialRules;
            for (int i = 0; i < rules.Count; i++)
            {
                float centerY = 0.65f - (i * 0.115f);
                CreatePanel(screen, $"TutorialRule_{i}", new Vector2(0.08f, centerY - 0.040f), new Vector2(0.92f, centerY + 0.040f), CardColor);
                CreateText(screen, $"{i + 1}. {rules[i]}", 25, new Vector2(0.12f, centerY - 0.030f), new Vector2(0.88f, centerY + 0.030f), FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            }

            CreateText(screen, "A sequência tem 9 cartas: Pergunta, Cientista, Pergunta, Cientista, Desafio, Cientista, Desafio, Pergunta e Final.", 23, new Vector2(0.10f, 0.22f), new Vector2(0.90f, 0.31f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateButton(screen, "Abrir mesa de cartas", new Vector2(0.5f, 0.13f), StartCardTable);
            CreateButton(screen, "Voltar", new Vector2(0.5f, 0.05f), ShowPlayerSetup, new Vector2(520f, 70f), SecondaryButtonColor, 24);
        }

        private void StartCardTable()
        {
            sessionStarted = true;
            ShowMainCardTable();
        }

        private void ShowMainCardTable()
        {
            if (state.IsComplete)
            {
                ShowSessionSummary();
                return;
            }

            RectTransform screen = BeginScreen();
            CreateTopBar(screen);
            CreatePartyArchiveAreas(screen);

            ForgottenNamesDeckCard card = state.CurrentCard;
            switch (card.Type)
            {
                case ForgottenNamesCardType.Question:
                    BuildQuestionCard(screen, ForgottenNamesExpeditionContent.Questions[card.ContentIndex]);
                    break;
                case ForgottenNamesCardType.Scientist:
                    BuildScientistCard(screen, card.ContentIndex, ForgottenNamesExpeditionContent.Scientists[card.ContentIndex]);
                    break;
                case ForgottenNamesCardType.Challenge:
                    BuildChallengeCard(screen, card.ContentIndex, ForgottenNamesExpeditionContent.Challenges[card.ContentIndex]);
                    break;
                case ForgottenNamesCardType.Final:
                    BuildFinalCard(screen);
                    break;
            }
        }

        private void BuildQuestionCard(RectTransform screen, ForgottenNamesQuestion question)
        {
            RectTransform cardPanel = CreateMainCardPanel(screen, "QuestionCard", new Color(0.12f, 0.16f, 0.25f, 0.98f));
            CreateText(cardPanel, "Pergunta", 26, new Vector2(0.08f, 0.88f), new Vector2(0.92f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, question.Title, 40, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.88f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, question.Body, 28, new Vector2(0.09f, 0.61f), new Vector2(0.91f, 0.73f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, question.Question, 34, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.58f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, QuestionHelperText + "\n" + question.Helper, 24, new Vector2(0.10f, 0.15f), new Vector2(0.90f, 0.32f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);

            CreateButton(screen, "Resposta falada: próxima carta", new Vector2(0.5f, 0.19f), () => CompleteCurrentPrompt($"Pergunta respondida: {question.Title}"), new Vector2(780f, 100f), PrimaryButtonColor, 28);
            CreateButton(screen, "Guia de Campo", new Vector2(0.30f, 0.09f), ShowFieldGuideScreen, new Vector2(400f, 82f), SecondaryButtonColor, 25);
            CreateButton(screen, "Encerrar", new Vector2(0.70f, 0.09f), ShowSessionSummary, new Vector2(320f, 82f), SecondaryButtonColor, 25);
        }

        private void BuildScientistCard(RectTransform screen, int scientistIndex, ForgottenNamesScientist scientist)
        {
            RectTransform cardPanel = CreateMainCardPanel(screen, "ScientistCard", new Color(0.11f, 0.18f, 0.18f, 0.98f));
            CreateText(cardPanel, "Cientista encontrada", 25, new Vector2(0.08f, 0.89f), new Vector2(0.92f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, scientist.Name, 42, new Vector2(0.08f, 0.79f), new Vector2(0.92f, 0.89f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, scientist.Field, 25, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.78f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, scientist.ShortContribution, 25, new Vector2(0.08f, 0.57f), new Vector2(0.92f, 0.70f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(cardPanel, scientist.HumanHook, 23, new Vector2(0.08f, 0.46f), new Vector2(0.92f, 0.56f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, "Tags: " + ForgottenNamesExpeditionContent.JoinTags(scientist.Tags), 22, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.44f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, scientist.EncounterPrompt, 22, new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.36f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(cardPanel, ScientistHelperText, 21, new Vector2(0.10f, 0.10f), new Vector2(0.90f, 0.18f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);

            CreateButton(screen, "Adicionar à Party", new Vector2(0.28f, 0.19f), () => AddScientistToParty(scientistIndex), new Vector2(430f, 92f), PrimaryButtonColor, 26);
            CreateButton(screen, "Registrar no Archive", new Vector2(0.72f, 0.19f), () => AddScientistToArchive(scientistIndex), new Vector2(430f, 92f), PrimaryButtonColor, 26);
            CreateButton(screen, "Ver bio no Guia", new Vector2(0.30f, 0.09f), () => ShowScientistBioModal(scientist), new Vector2(400f, 82f), SecondaryButtonColor, 24);
            CreateButton(screen, "Encerrar", new Vector2(0.70f, 0.09f), ShowSessionSummary, new Vector2(320f, 82f), SecondaryButtonColor, 24);
        }

        private void BuildChallengeCard(RectTransform screen, int challengeIndex, ForgottenNamesChallenge challenge)
        {
            RectTransform cardPanel = CreateMainCardPanel(screen, "ChallengeCard", new Color(0.18f, 0.13f, 0.18f, 0.98f));
            CreateText(cardPanel, "Desafio", 24, new Vector2(0.08f, 0.88f), new Vector2(0.92f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, challenge.Title, 40, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.88f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, challenge.Situation, 28, new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.75f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(cardPanel, "Tags recomendadas: " + ForgottenNamesExpeditionContent.JoinTags(challenge.RecommendedTags), 23, new Vector2(0.08f, 0.56f), new Vector2(0.92f, 0.62f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, ChallengeHelperText, 23, new Vector2(0.10f, 0.45f), new Vector2(0.90f, 0.54f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateChallengeHelperButtons(cardPanel, challengeIndex, challenge);

            CreateButton(screen, "Encerrar", new Vector2(0.5f, 0.09f), ShowSessionSummary, new Vector2(380f, 82f), SecondaryButtonColor, 24);
        }

        private void BuildChallengeResolutionCard(int challengeIndex, int scientistIndex, bool matched)
        {
            ForgottenNamesChallenge challenge = ForgottenNamesExpeditionContent.Challenges[challengeIndex];
            RectTransform screen = BeginScreen();
            CreateTopBar(screen);
            CreatePartyArchiveAreas(screen);

            RectTransform cardPanel = CreateMainCardPanel(screen, "ChallengeResolutionCard", new Color(0.14f, 0.12f, 0.20f, 0.98f));
            string helperName = scientistIndex >= 0 ? ForgottenNamesExpeditionContent.Scientists[scientistIndex].Name : "Improviso do grupo";
            CreateText(cardPanel, matched ? "Conexão forte" : "Solução improvisada", 24, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, helperName, 40, new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.84f), FontStyles.Bold, TextAlignmentOptions.Center);
            string resolutionPrompt = scientistIndex < 0 && state.PartyScientistIndexes.Count == 0
                ? "Sem uma figura ativa na Party, o grupo resolve junto. Que esforço extra foi necessário?"
                : matched ? challenge.MatchedPrompt : challenge.UnmatchedPrompt;
            CreateText(cardPanel, resolutionPrompt, 31, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.69f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(cardPanel, "Responda em voz alta. Quando terminar, toque em Concluir.", 25, new Vector2(0.10f, 0.25f), new Vector2(0.90f, 0.36f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);

            CreateButton(screen, "Concluir", new Vector2(0.5f, 0.19f), () => CompleteCurrentPrompt($"Desafio resolvido: {challenge.Title} com {helperName}"), new Vector2(720f, 100f), PrimaryButtonColor, 30);
            CreateButton(screen, "Encerrar", new Vector2(0.5f, 0.09f), ShowSessionSummary, new Vector2(380f, 82f), SecondaryButtonColor, 24);
        }

        private void BuildFinalCard(RectTransform screen)
        {
            ForgottenNamesFinalCard finalCard = ForgottenNamesExpeditionContent.FinalCard;
            RectTransform cardPanel = CreateMainCardPanel(screen, "FinalCard", new Color(0.17f, 0.14f, 0.10f, 0.98f));
            CreateText(cardPanel, "Final", 25, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, finalCard.Title, 40, new Vector2(0.08f, 0.75f), new Vector2(0.92f, 0.84f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, finalCard.Body, 28, new Vector2(0.08f, 0.61f), new Vector2(0.92f, 0.72f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, finalCard.FinalPrompt, 32, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.58f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, FinalHelperText, 24, new Vector2(0.10f, 0.31f), new Vector2(0.90f, 0.40f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, finalCard.GroupSentence, 30, new Vector2(0.10f, 0.17f), new Vector2(0.90f, 0.29f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);

            CreateButton(screen, "Encerrar expedição", new Vector2(0.5f, 0.19f), () => CompleteCurrentPrompt("Carta final concluída"), new Vector2(720f, 100f), PrimaryButtonColor, 30);
            CreateButton(screen, "Guia de Campo", new Vector2(0.30f, 0.09f), ShowFieldGuideScreen, new Vector2(400f, 82f), SecondaryButtonColor, 25);
            CreateButton(screen, "Sair", new Vector2(0.70f, 0.09f), ReturnToSelector, new Vector2(320f, 82f), SecondaryButtonColor, 25);
        }

        private void CreateChallengeHelperButtons(RectTransform cardPanel, int challengeIndex, ForgottenNamesChallenge challenge)
        {
            if (state.PartyScientistIndexes.Count == 0)
            {
                CreateText(cardPanel, "Sem uma figura ativa na Party, o grupo resolve junto. Que esforço extra foi necessário?", 27, new Vector2(0.10f, 0.21f), new Vector2(0.90f, 0.38f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
                CreateButton(cardPanel, "Resolver em grupo", new Vector2(0.50f, 0.14f), () => ResolveChallenge(challengeIndex, -1), new Vector2(460f, 82f), PrimaryButtonColor, 24);
                return;
            }

            int count = state.PartyScientistIndexes.Count;
            for (int i = 0; i < count; i++)
            {
                int scientistIndex = state.PartyScientistIndexes[i];
                ForgottenNamesScientist scientist = ForgottenNamesExpeditionContent.Scientists[scientistIndex];
                bool matched = ForgottenNamesExpeditionContent.HasMatchingTag(scientist, challenge);
                float x = count == 1 ? 0.50f : 0.22f + (i * 0.28f);
                RectTransform helperCard = CreatePanel(cardPanel, $"ChallengePartyScientist_{scientistIndex}", new Vector2(x - 0.13f, 0.12f), new Vector2(x + 0.13f, 0.34f), matched ? new Color(0.18f, 0.42f, 0.28f, 0.98f) : new Color(0.16f, 0.17f, 0.23f, 0.98f));
                CreateText(helperCard, scientist.Name, 20, new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center);
                CreateText(helperCard, matched ? "Tag compatível" : "Outra perspectiva", 18, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.58f), FontStyles.Italic, TextAlignmentOptions.Center, matched ? HighlightTextColor : MutedTextColor);
                CreateButton(helperCard, "Escolher", new Vector2(0.50f, 0.16f), () => ResolveChallenge(challengeIndex, scientistIndex), new Vector2(205f, 52f), matched ? PrimaryButtonColor : SecondaryButtonColor, 18);
            }
        }

        private void ResolveChallenge(int challengeIndex, int scientistIndex)
        {
            bool matched = false;
            if (scientistIndex >= 0)
            {
                matched = ForgottenNamesExpeditionContent.HasMatchingTag(ForgottenNamesExpeditionContent.Scientists[scientistIndex], ForgottenNamesExpeditionContent.Challenges[challengeIndex]);
            }

            BuildChallengeResolutionCard(challengeIndex, scientistIndex, matched);
        }

        private void AddScientistToParty(int scientistIndex)
        {
            if (state.TryAddScientistToParty(scientistIndex))
            {
                ShowMainCardTable();
                return;
            }

            ShowPartyFullModal(scientistIndex);
        }

        private void ShowPartyFullModal(int newScientistIndex)
        {
            CloseModal();
            if (root == null) return;

            ForgottenNamesScientist newScientist = ForgottenNamesExpeditionContent.Scientists[newScientistIndex];
            modalOverlay = new GameObject("ForgottenNamesPartyFullModal", typeof(RectTransform), typeof(Image));
            RectTransform overlayRect = modalOverlay.GetComponent<RectTransform>();
            overlayRect.SetParent(root.transform, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            modalOverlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.70f);

            RectTransform panel = CreatePanel(overlayRect, "PartyFullPanel", new Vector2(0.07f, 0.10f), new Vector2(0.93f, 0.88f), new Color(0.08f, 0.11f, 0.17f, 0.98f));
            CreateText(panel, "A Party está cheia", 43, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(panel, "A Party está cheia. Escolha uma figura para mover ao Archive.", 27, new Vector2(0.10f, 0.75f), new Vector2(0.90f, 0.84f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(panel, $"Chegando agora: {newScientist.Name} — {newScientist.Field}", 24, new Vector2(0.10f, 0.68f), new Vector2(0.90f, 0.74f), FontStyles.Bold, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(panel, "Tags: " + ForgottenNamesExpeditionContent.JoinTags(newScientist.Tags), 18, new Vector2(0.10f, 0.63f), new Vector2(0.90f, 0.68f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);

            for (int i = 0; i < state.PartyScientistIndexes.Count; i++)
            {
                int partyScientistIndex = state.PartyScientistIndexes[i];
                ForgottenNamesScientist partyScientist = ForgottenNamesExpeditionContent.Scientists[partyScientistIndex];
                float centerY = 0.57f - (i * 0.16f);
                CreatePanel(panel, $"PartyChoice_{partyScientistIndex}", new Vector2(0.08f, centerY - 0.055f), new Vector2(0.92f, centerY + 0.055f), CardColor);
                CreateText(panel, partyScientist.Name, 24, new Vector2(0.12f, centerY + 0.005f), new Vector2(0.58f, centerY + 0.045f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
                CreateText(panel, partyScientist.Field, 18, new Vector2(0.12f, centerY - 0.040f), new Vector2(0.58f, centerY + 0.000f), FontStyles.Italic, TextAlignmentOptions.MidlineLeft, MutedTextColor);
                CreateButton(panel, "Mover para o Archive", new Vector2(0.73f, centerY), () => MovePartyScientistToArchiveAndAddNew(partyScientistIndex, newScientistIndex), new Vector2(310f, 58f), PrimaryButtonColor, 19);
            }

            CreateText(panel, "Todos continuam reconhecidos: a Party ajuda agora, e o Archive guarda nomes para lembrar depois.", 21, new Vector2(0.10f, 0.14f), new Vector2(0.90f, 0.23f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateButton(panel, "Cancelar", new Vector2(0.50f, 0.08f), CloseModal, new Vector2(300f, 58f), SecondaryButtonColor, 18);
        }

        private void MovePartyScientistToArchiveAndAddNew(int partyScientistIndex, int newScientistIndex)
        {
            state.ReplacePartyScientist(partyScientistIndex, newScientistIndex);
            CloseModal();
            ShowMainCardTable();
        }

        private void AddScientistToArchive(int scientistIndex)
        {
            state.AddScientistToArchive(scientistIndex);
            ShowArchivePromptThenContinue(scientistIndex);
        }

        private void ShowArchivePromptThenContinue(int scientistIndex)
        {
            ForgottenNamesScientist scientist = ForgottenNamesExpeditionContent.Scientists[scientistIndex];
            RectTransform screen = BeginScreen();
            CreateTopBar(screen);
            CreatePartyArchiveAreas(screen);
            RectTransform cardPanel = CreateMainCardPanel(screen, "ArchivePromptCard", new Color(0.13f, 0.14f, 0.20f, 0.98f));
            CreateText(cardPanel, "Registro no Archive", 26, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, scientist.Name, 42, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.84f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, scientist.ArchivePrompt, 32, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.66f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(cardPanel, "Respondam em voz alta. O app apenas guia a conversa.", 25, new Vector2(0.10f, 0.24f), new Vector2(0.90f, 0.34f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateButton(screen, "Registro falado: continuar", new Vector2(0.5f, 0.19f), () => CompleteCurrentPrompt($"Registro no Archive concluído: {scientist.Name}"), new Vector2(720f, 100f), PrimaryButtonColor, 30);
            CreateButton(screen, "Encerrar", new Vector2(0.5f, 0.09f), ShowSessionSummary, new Vector2(380f, 82f), SecondaryButtonColor, 24);
        }

        private void CompleteCurrentPrompt(string summary)
        {
            state.CompletePromptCard(summary);
            ShowMainCardTable();
        }

        private void ShowSessionSummary()
        {
            RectTransform screen = BeginScreen();
            CreateText(screen, "Resumo da sessão", 52, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, state.SelectedPremise.Title, 29, new Vector2(0.10f, 0.77f), new Vector2(0.90f, 0.83f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);

            CreatePanel(screen, "SummaryPanel", new Vector2(0.07f, 0.22f), new Vector2(0.93f, 0.73f), CardColor);
            CreateText(screen, BuildSummaryText(), 25, new Vector2(0.11f, 0.25f), new Vector2(0.89f, 0.70f), FontStyles.Normal, TextAlignmentOptions.TopLeft);

            CreateButton(screen, "Nova expedição", new Vector2(0.5f, 0.14f), RestartSession);
            CreateButton(screen, "Voltar aos protótipos", new Vector2(0.5f, 0.055f), ReturnToSelector, new Vector2(620f, 72f), SecondaryButtonColor, 24);
        }

        private string BuildSummaryText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Premissa: {state.SelectedPremise.Title}");
            builder.AppendLine($"Cartas concluídas: {Mathf.Min(state.CurrentDeckIndex, state.TotalCards)} de {state.TotalCards}");
            builder.AppendLine($"Jogadores: {state.PlayerCount}");
            builder.AppendLine("Papéis: " + BuildRoleSummary());
            builder.AppendLine();
            builder.AppendLine("Party: " + FormatScientistList(state.PartyScientistIndexes));
            builder.AppendLine("Archive: " + FormatScientistList(state.ArchiveScientistIndexes));
            builder.AppendLine();
            builder.AppendLine("Frase final sugerida:");
            builder.AppendLine(ForgottenNamesExpeditionContent.FinalCard.GroupSentence);
            return builder.ToString();
        }

        private string BuildRoleSummary()
        {
            List<string> roles = new List<string>();
            for (int i = 0; i < state.PlayerCount; i++)
            {
                roles.Add($"J{i + 1} {state.GetRoleForPlayer(i).Title}");
            }

            return string.Join("; ", roles);
        }

        private static string FormatScientistList(List<int> indexes)
        {
            if (indexes == null || indexes.Count == 0) return "ninguém ainda";
            List<string> names = new List<string>();
            foreach (int index in indexes)
            {
                names.Add(ForgottenNamesExpeditionContent.Scientists[index].Name);
            }

            return string.Join(", ", names);
        }

        private void RestartSession()
        {
            state = new ForgottenNamesExpeditionState();
            sessionStarted = false;
            ShowPremiseSelection();
        }

        private void ShowFieldGuideScreen()
        {
            RectTransform screen = BeginScreen();
            CreateText(screen, "Guia de Campo", 52, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, "Role para consultar as biografias. Use este guia quando uma carta pedir contexto.", 25, new Vector2(0.10f, 0.77f), new Vector2(0.90f, 0.83f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);

            RectTransform content = CreateScrollContent(screen, "FieldGuideScroll", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.75f), new Color(0.07f, 0.09f, 0.14f, 0.92f));
            for (int i = 0; i < ForgottenNamesExpeditionContent.Scientists.Count; i++)
            {
                CreateFieldGuideRow(content, ForgottenNamesExpeditionContent.Scientists[i]);
            }

            CreateButton(screen, sessionStarted ? "Voltar à mesa" : "Voltar", new Vector2(0.30f, 0.07f), () => { if (sessionStarted) ShowMainCardTable(); else ShowRootScreen(); }, new Vector2(410f, 82f), PrimaryButtonColor, 25);
            CreateButton(screen, "Início", new Vector2(0.70f, 0.07f), ShowRootScreen, new Vector2(320f, 82f), SecondaryButtonColor, 25);
        }

        private static RectTransform CreateScrollContent(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject viewportObject = new GameObject(name + "Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            viewport.SetParent(parent, false);
            viewport.anchorMin = anchorMin;
            viewport.anchorMax = anchorMax;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewportObject.GetComponent<Image>().color = color;

            GameObject contentObject = new GameObject(name + "Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            RectTransform content = contentObject.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 16f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = viewportObject.GetComponent<ScrollRect>();
            scrollRect.viewport = viewport;
            scrollRect.content = content;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            return content;
        }

        private static void CreateFieldGuideRow(RectTransform content, ForgottenNamesScientist scientist)
        {
            GameObject rowObject = new GameObject("FieldGuideScientist_" + scientist.Name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            RectTransform row = rowObject.GetComponent<RectTransform>();
            row.SetParent(content, false);
            rowObject.GetComponent<Image>().color = CardColor;
            rowObject.GetComponent<LayoutElement>().preferredHeight = 210f;

            CreateText(row, scientist.Name, 27, new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.94f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            CreateText(row, scientist.Field, 21, new Vector2(0.05f, 0.54f), new Vector2(0.95f, 0.70f), FontStyles.Italic, TextAlignmentOptions.MidlineLeft, HighlightTextColor);
            CreateText(row, scientist.FieldGuideBio, 21, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.52f), FontStyles.Normal, TextAlignmentOptions.TopLeft, MutedTextColor);
        }

        private void ShowHowToPlayModal()
        {
            CloseModal();
            if (root == null) return;

            modalOverlay = new GameObject("ForgottenNamesHowToPlayModal", typeof(RectTransform), typeof(Image));
            RectTransform overlayRect = modalOverlay.GetComponent<RectTransform>();
            overlayRect.SetParent(root.transform, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            modalOverlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.68f);

            RectTransform panel = CreatePanel(overlayRect, "HowToPlayPanel", new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.84f), new Color(0.08f, 0.11f, 0.17f, 0.98f));
            CreateText(panel, "Como jogar", 46, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(panel, BuildHowToPlayText(), 27, new Vector2(0.10f, 0.24f), new Vector2(0.90f, 0.78f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
            CreateButton(panel, "Fechar", new Vector2(0.5f, 0.13f), CloseModal, new Vector2(420f, 82f), PrimaryButtonColor);
        }

        private static string BuildHowToPlayText()
        {
            StringBuilder builder = new StringBuilder();
            IReadOnlyList<string> rules = ForgottenNamesExpeditionContent.TutorialRules;
            for (int i = 0; i < rules.Count; i++)
            {
                builder.AppendLine($"{i + 1}. {rules[i]}");
            }

            builder.AppendLine();
            builder.AppendLine("O app não registra respostas faladas: ele apenas conduz a ordem das cartas e mantém Party/Archive visíveis.");
            return builder.ToString();
        }

        private void ShowScientistBioModal(ForgottenNamesScientist scientist)
        {
            CloseModal();
            if (root == null) return;

            modalOverlay = new GameObject("ForgottenNamesScientistBioModal", typeof(RectTransform), typeof(Image));
            RectTransform overlayRect = modalOverlay.GetComponent<RectTransform>();
            overlayRect.SetParent(root.transform, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            modalOverlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.68f);

            RectTransform panel = CreatePanel(overlayRect, "ScientistBioPanel", new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.82f), new Color(0.08f, 0.11f, 0.17f, 0.98f));
            CreateText(panel, scientist.Name, 44, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.92f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(panel, scientist.FieldGuideBio, 28, new Vector2(0.10f, 0.28f), new Vector2(0.90f, 0.74f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateButton(panel, "Fechar", new Vector2(0.5f, 0.16f), CloseModal, new Vector2(420f, 82f), PrimaryButtonColor);
        }

        private void ReturnToSelector()
        {
            context?.ReturnToSelector?.Invoke();
        }

        private RectTransform BeginScreen()
        {
            EnsureRoot();
            CloseModal();
            RectTransform screen = root.GetComponent<RectTransform>();
            ClearChildren(screen);
            return screen;
        }

        private void EnsureRoot()
        {
            if (root != null) return;

            root = new GameObject("ForgottenNamesExpeditionRoot", typeof(RectTransform), typeof(Image));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.SetParent(context.SceneRoot.FullScreenRoot, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = BackgroundColor;
        }

        private void CreateTopBar(RectTransform screen)
        {
            CreatePanel(screen, "TopBar", new Vector2(0f, 0.90f), new Vector2(1f, 1f), new Color(0.04f, 0.06f, 0.10f, 0.98f));
            CreateText(screen, state.SelectedPremise.Title, 25, new Vector2(0.05f, 0.948f), new Vector2(0.95f, 0.986f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(screen, $"Jogador {state.CurrentPlayerIndex + 1}: {state.CurrentRole.Title}", 23, new Vector2(0.06f, 0.910f), new Vector2(0.62f, 0.946f), FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            CreateText(screen, $"Carta {Mathf.Min(state.CurrentDeckIndex + 1, state.TotalCards)}/{state.TotalCards}", 23, new Vector2(0.62f, 0.910f), new Vector2(0.94f, 0.946f), FontStyles.Normal, TextAlignmentOptions.MidlineRight);
        }

        private void CreatePartyArchiveAreas(RectTransform screen)
        {
            RectTransform party = CreatePanel(screen, "PartyPanel", new Vector2(0.05f, 0.79f), new Vector2(0.48f, 0.89f), new Color(0.08f, 0.13f, 0.18f, 0.96f));
            RectTransform archive = CreatePanel(screen, "ArchivePanel", new Vector2(0.52f, 0.79f), new Vector2(0.95f, 0.89f), new Color(0.10f, 0.10f, 0.16f, 0.96f));
            CreateText(party, "Party", 23, new Vector2(0.05f, 0.60f), new Vector2(0.95f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(party, FormatCompactScientistList(state.PartyScientistIndexes), 19, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.60f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(archive, "Archive", 23, new Vector2(0.05f, 0.60f), new Vector2(0.95f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(archive, FormatCompactScientistList(state.ArchiveScientistIndexes), 19, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.60f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
        }

        private void CreateRoleList(RectTransform screen, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform panel = CreatePanel(screen, "RoleListPanel", anchorMin, anchorMax, CardColor);
            CreateText(panel, "Papéis desta expedição", 27, new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            float step = state.PlayerCount > 4 ? 0.12f : 0.18f;
            int titleSize = state.PlayerCount > 4 ? 21 : 24;
            int bodySize = state.PlayerCount > 4 ? 18 : 20;
            for (int i = 0; i < state.PlayerCount; i++)
            {
                ForgottenNamesRole role = state.GetRoleForPlayer(i);
                float top = 0.78f - (i * step);
                CreateText(panel, $"Jogador {i + 1}: {role.Title}", titleSize, new Vector2(0.08f, top - 0.038f), new Vector2(0.92f, top), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
                CreateText(panel, role.ShortDescription, bodySize, new Vector2(0.08f, top - 0.090f), new Vector2(0.92f, top - 0.038f), FontStyles.Normal, TextAlignmentOptions.TopLeft, MutedTextColor);
            }
        }

        private static RectTransform CreateMainCardPanel(RectTransform screen, string name, Color color)
        {
            return CreatePanel(screen, name, new Vector2(0.06f, 0.30f), new Vector2(0.94f, 0.77f), color);
        }

        private static string FormatCompactScientistList(List<int> indexes)
        {
            if (indexes == null || indexes.Count == 0) return "—";
            List<string> names = new List<string>();
            foreach (int index in indexes)
            {
                names.Add(ForgottenNamesExpeditionContent.Scientists[index].Name);
            }

            return string.Join("\n", names);
        }

        private void CloseModal()
        {
            if (modalOverlay == null) return;
            modalOverlay.SetActive(false);
            Object.Destroy(modalOverlay);
            modalOverlay = null;
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
            panel.GetComponent<Image>().color = color;
            return rect;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style, TextAlignmentOptions alignment)
        {
            return CreateText(parent, value, size, anchorMin, anchorMax, style, alignment, Color.white);
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style, TextAlignmentOptions alignment, Color color)
        {
            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.enableWordWrapping = true;
            text.margin = new Vector4(8f, 4f, 8f, 4f);
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            return CreateButton(parent, label, anchor, onClick, new Vector2(740f, 100f), PrimaryButtonColor);
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick, Vector2 size, Color color, int fontSize = 30)
        {
            GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            buttonObject.GetComponent<Image>().color = color;

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            CreateText(rect, label, fontSize, Vector2.zero, Vector2.one, FontStyles.Bold, TextAlignmentOptions.Center);
            return button;
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                Object.Destroy(child);
            }
        }

        private static Color BackgroundColor => new Color(0.055f, 0.075f, 0.12f, 1f);
        private static Color CardColor => new Color(0.11f, 0.15f, 0.22f, 0.96f);
        private static Color PrimaryButtonColor => new Color(0.18f, 0.48f, 0.86f, 1f);
        private static Color SecondaryButtonColor => new Color(0.24f, 0.27f, 0.33f, 1f);
        private static Color MutedTextColor => new Color(0.80f, 0.86f, 0.94f, 1f);
        private static Color HighlightTextColor => new Color(0.96f, 0.78f, 0.40f, 1f);
    }
}
