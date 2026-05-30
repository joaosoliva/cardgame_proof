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
        private int currentHowToPlayPage;
        private int activeQuestionDeckIndex = -1;
        private int questionRevealStep;

        private static readonly string[] PremiseIcons = { "▣", "✦", "?" };

        private static readonly HelpPage[] HowToPlayPages =
        {
            new HelpPage("O que é este jogo?", "A Expedição dos Nomes Esquecidos é um jogo narrativo de cartas.\nVocês criam juntos uma pequena jornada sobre ciência, memória e descobertas pouco lembradas.\nNão há pontos, vilão ou resposta certa."),
            new HelpPage("Como responder?", "Leia a carta em voz alta.\nResponda como seu personagem.\nUma frase já é suficiente.\nSe quiser, você pode elaborar com mais detalhes."),
            new HelpPage("Premissa da expedição", "No começo, o grupo escolhe uma carta de premissa.\nEla define o contexto da jornada.\nA premissa fica visível durante a partida, como se estivesse sobre a mesa."),
            new HelpPage("Party e Archive", "Quando uma figura científica aparece, ela pode entrar na Party ou ser registrada no Archive.\nParty = ajuda agora nos desafios.\nArchive = será lembrado depois.\nAs duas escolhas são válidas."),
            new HelpPage("Desafios", "Em desafios, escolha alguém da Party para ajudar.\nSe as palavras-chave combinarem, a ajuda é mais direta.\nSe não combinarem, o grupo ainda resolve a cena improvisando outro caminho."),
            new HelpPage("Fim da jornada", "A carta final mostra o ponto de chegada da expedição.\nNo fim, cada jogador responde o que deseja preservar.\nDepois, o grupo completa a frase: 'Nós lembramos ______ porque ______.'")
        };

        private readonly struct HelpPage
        {
            public HelpPage(string title, string body)
            {
                Title = title;
                Body = body;
            }

            public string Title { get; }
            public string Body { get; }
        }

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
            CreateText(screen, "Escolha a premissa", 50, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, "Escolham uma carta para guiar a jornada. Ela ficará visível sobre a mesa durante toda a partida.", 25, new Vector2(0.10f, 0.78f), new Vector2(0.90f, 0.85f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);

            IReadOnlyList<ForgottenNamesPremise> premises = ForgottenNamesExpeditionContent.Premises;
            for (int i = 0; i < premises.Count; i++)
            {
                int premiseIndex = i;
                ForgottenNamesPremise premise = premises[i];
                float centerY = 0.64f - (i * 0.18f);
                RectTransform card = CreatePanel(screen, $"PremiseCard_{i}", new Vector2(0.07f, centerY - 0.075f), new Vector2(0.93f, centerY + 0.075f), new Color(0.12f, 0.16f, 0.24f, 0.98f));
                CreateText(card, PremiseIcons[i % PremiseIcons.Length], 40, new Vector2(0.04f, 0.18f), new Vector2(0.16f, 0.84f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
                CreateText(card, premise.Title, 29, new Vector2(0.18f, 0.56f), new Vector2(0.78f, 0.90f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
                CreateText(card, premise.Description, 19, new Vector2(0.18f, 0.24f), new Vector2(0.94f, 0.56f), FontStyles.Normal, TextAlignmentOptions.TopLeft, MutedTextColor);
                CreateText(card, premise.TableReminder, 18, new Vector2(0.18f, 0.08f), new Vector2(0.94f, 0.24f), FontStyles.Italic, TextAlignmentOptions.TopLeft, HighlightTextColor);
                CreateButton(card, "Escolher", new Vector2(0.84f, 0.73f), () => SelectPremise(premiseIndex), new Vector2(190f, 54f), PrimaryButtonColor, 19);
            }

            CreateButton(screen, "Voltar", new Vector2(0.5f, 0.08f), ShowRootScreen, new Vector2(560f, 82f), SecondaryButtonColor);
        }

        private void SelectPremise(int premiseIndex)
        {
            state.SelectPremise(premiseIndex);
            ShowPremiseConfirmation();
        }

        private void ShowPremiseConfirmation()
        {
            RectTransform screen = BeginScreen();
            CreateText(screen, "Premissa escolhida", 50, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center);
            RectTransform card = CreatePanel(screen, "SelectedPremiseCard", new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.75f), new Color(0.13f, 0.18f, 0.28f, 0.98f));
            CreateText(card, PremiseIcons[state.SelectedPremiseIndex % PremiseIcons.Length], 58, new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.88f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(card, state.SelectedPremise.Title, 40, new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.68f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(card, state.SelectedPremise.Description, 25, new Vector2(0.10f, 0.34f), new Vector2(0.90f, 0.50f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(card, state.SelectedPremise.TableReminder, 24, new Vector2(0.10f, 0.22f), new Vector2(0.90f, 0.34f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(card, "Pense em: " + FormatAnswerHints(state.SelectedPremise), 21, new Vector2(0.10f, 0.10f), new Vector2(0.90f, 0.22f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(screen, "Esta será a premissa da expedição.", 30, new Vector2(0.10f, 0.25f), new Vector2(0.90f, 0.31f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateButton(screen, "Continuar", new Vector2(0.5f, 0.15f), ShowPlayerSetup);
            CreateButton(screen, "Trocar premissa", new Vector2(0.5f, 0.06f), ShowPremiseSelection, new Vector2(560f, 76f), SecondaryButtonColor, 24);
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
            CreateButton(screen, "Ver carta final", new Vector2(0.5f, 0.12f), ShowFinalCardPreview);
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

        private void ShowFinalCardPreview()
        {
            RectTransform screen = BeginScreen();
            ForgottenNamesFinalCard finalCard = ForgottenNamesExpeditionContent.FinalCard;
            CreateText(screen, "Carta Final da Expedição", 48, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, "Esta carta mostra o ponto de chegada da jornada. Ela não é um spoiler: ela ajuda o grupo a entender para onde a história está caminhando.", 24, new Vector2(0.09f, 0.76f), new Vector2(0.91f, 0.85f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            RectTransform card = CreatePanel(screen, "FinalPreviewCard", new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.73f), new Color(0.18f, 0.14f, 0.09f, 0.98f));
            CreateText(card, state.SelectedPremise.Title, 25, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(card, state.SelectedPremise.FinalBridge, 24, new Vector2(0.09f, 0.72f), new Vector2(0.91f, 0.84f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(card, finalCard.Title, 37, new Vector2(0.08f, 0.56f), new Vector2(0.92f, 0.70f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(card, finalCard.FinalPrompt, 30, new Vector2(0.10f, 0.28f), new Vector2(0.90f, 0.52f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(card, finalCard.GroupSentence, 27, new Vector2(0.10f, 0.12f), new Vector2(0.90f, 0.24f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);
            CreateButton(screen, "Entendi", new Vector2(0.5f, 0.17f), ShowTutorialScreen);
            CreateButton(screen, "Voltar", new Vector2(0.5f, 0.07f), ShowPlayerSetup, new Vector2(520f, 76f), SecondaryButtonColor, 24);
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

            CreateText(screen, "A sequência é gerada no começo da sessão: blocos curtos de perguntas, três cientistas, dois desafios e a carta final sempre por último.", 23, new Vector2(0.10f, 0.22f), new Vector2(0.90f, 0.31f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateButton(screen, "Abrir mesa de cartas", new Vector2(0.5f, 0.13f), StartCardTable);
            CreateButton(screen, "Voltar", new Vector2(0.5f, 0.05f), ShowPlayerSetup, new Vector2(520f, 70f), SecondaryButtonColor, 24);
        }

        private void StartCardTable()
        {
            state.GenerateSessionDeck();
            activeQuestionDeckIndex = -1;
            questionRevealStep = 0;
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
            if (activeQuestionDeckIndex != state.CurrentDeckIndex)
            {
                activeQuestionDeckIndex = state.CurrentDeckIndex;
                questionRevealStep = 0;
            }

            if (questionRevealStep <= 0)
            {
                RectTransform coveredCard = CreateMainCardPanel(screen, "QuestionCardBack", new Color(0.08f, 0.12f, 0.20f, 0.98f));
                CreateText(coveredCard, state.SelectedPremise.QuestionBridge, 24, new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.86f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
                CreateText(coveredCard, "Carta de Pergunta", 34, new Vector2(0.08f, 0.54f), new Vector2(0.92f, 0.68f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
                CreateText(coveredCard, "Respirem, virem a carta e leiam juntos.", 26, new Vector2(0.12f, 0.40f), new Vector2(0.88f, 0.52f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
                CreateButton(screen, "Revelar", new Vector2(0.5f, 0.075f), () => SetQuestionRevealStep(1), new Vector2(720f, 100f), PrimaryButtonColor, 30);
            }
            else
            {
                RectTransform cardPanel = CreateMainCardPanel(screen, "QuestionCard", new Color(0.12f, 0.16f, 0.25f, 0.98f));
                CreateText(cardPanel, "Pergunta", 24, new Vector2(0.08f, 0.90f), new Vector2(0.92f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
                CreateText(cardPanel, state.SelectedPremise.QuestionBridge, 22, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.89f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
                CreateText(cardPanel, question.Title, 37, new Vector2(0.08f, 0.67f), new Vector2(0.92f, 0.79f), FontStyles.Bold, TextAlignmentOptions.Center);
                CreateText(cardPanel, question.Body, 26, new Vector2(0.09f, 0.51f), new Vector2(0.91f, 0.65f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);

                if (questionRevealStep >= 2)
                {
                    RectTransform questionFocus = CreatePanel(cardPanel, "QuestionFocusBox", new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.50f), new Color(0.18f, 0.22f, 0.32f, 0.96f));
                    CreateText(questionFocus, "Pergunta", 18, new Vector2(0.06f, 0.70f), new Vector2(0.94f, 0.94f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft, HighlightTextColor);
                    CreateText(questionFocus, question.Question, 30, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.72f), FontStyles.Bold, TextAlignmentOptions.Center);
                    CreateText(cardPanel, QuestionHelperText + "\n" + question.Helper, 23, new Vector2(0.10f, 0.10f), new Vector2(0.90f, 0.26f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
                    CreateButton(screen, "Concluir", new Vector2(0.5f, 0.075f), () => CompleteCurrentPrompt($"Pergunta respondida: {question.Title}"), new Vector2(720f, 100f), PrimaryButtonColor, 30);
                }
                else
                {
                    CreateText(cardPanel, "Quando todos tiverem entendido a cena, revelem a pergunta principal.", 26, new Vector2(0.12f, 0.28f), new Vector2(0.88f, 0.44f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
                    CreateButton(screen, "Ver pergunta", new Vector2(0.5f, 0.075f), () => SetQuestionRevealStep(2), new Vector2(720f, 100f), PrimaryButtonColor, 30);
                }
            }

            CreateButton(screen, "Guia de Campo", new Vector2(0.30f, 0.030f), ShowFieldGuideScreen, new Vector2(400f, 82f), SecondaryButtonColor, 25);
            CreateButton(screen, "Encerrar", new Vector2(0.70f, 0.030f), ShowSessionSummary, new Vector2(320f, 82f), SecondaryButtonColor, 25);
        }

        private void SetQuestionRevealStep(int step)
        {
            questionRevealStep = step;
            ShowMainCardTable();
        }

        private void BuildScientistCard(RectTransform screen, int scientistIndex, ForgottenNamesScientist scientist)
        {
            RectTransform cardPanel = CreateMainCardPanel(screen, "ScientistCard", new Color(0.11f, 0.18f, 0.18f, 0.98f));
            CreateText(cardPanel, "Cientista encontrada", 23, new Vector2(0.08f, 0.91f), new Vector2(0.92f, 0.97f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, state.SelectedPremise.ScientistBridge, 21, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.90f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, scientist.Name, 38, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.82f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, scientist.Field, 23, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.72f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, scientist.ShortContribution, 23, new Vector2(0.08f, 0.52f), new Vector2(0.92f, 0.64f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(cardPanel, scientist.HumanHook, 21, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.51f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, "Tags: " + ForgottenNamesExpeditionContent.JoinTags(scientist.Tags), 20, new Vector2(0.08f, 0.35f), new Vector2(0.92f, 0.41f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, scientist.EncounterPrompt, 19, new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.34f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(cardPanel, "Entra na Party ou seu nome é registrado no Archive?", 20, new Vector2(0.08f, 0.15f), new Vector2(0.92f, 0.22f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, ScientistHelperText, 18, new Vector2(0.10f, 0.07f), new Vector2(0.90f, 0.14f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);

            CreateButton(screen, "Adicionar à Party", new Vector2(0.28f, 0.075f), () => AddScientistToParty(scientistIndex), new Vector2(430f, 92f), PrimaryButtonColor, 26);
            CreateButton(screen, "Registrar no Archive", new Vector2(0.72f, 0.075f), () => AddScientistToArchive(scientistIndex), new Vector2(430f, 92f), PrimaryButtonColor, 26);
            CreateButton(screen, "Ler mini bio", new Vector2(0.30f, 0.030f), () => ShowFieldGuideDetailScreen(scientistIndex, false), new Vector2(400f, 82f), SecondaryButtonColor, 24);
            CreateButton(screen, "Encerrar", new Vector2(0.70f, 0.030f), ShowSessionSummary, new Vector2(320f, 82f), SecondaryButtonColor, 24);
        }

        private void BuildChallengeCard(RectTransform screen, int challengeIndex, ForgottenNamesChallenge challenge)
        {
            RectTransform cardPanel = CreateMainCardPanel(screen, "ChallengeCard", new Color(0.18f, 0.13f, 0.18f, 0.98f));
            CreateText(cardPanel, "Desafio", 23, new Vector2(0.08f, 0.91f), new Vector2(0.92f, 0.97f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, state.SelectedPremise.ChallengeBridge, 21, new Vector2(0.08f, 0.81f), new Vector2(0.92f, 0.90f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, challenge.Title, 38, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.80f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, challenge.Situation, 25, new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.68f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(cardPanel, "Tags recomendadas: " + ForgottenNamesExpeditionContent.JoinTags(challenge.RecommendedTags), 23, new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.56f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, ChallengeHelperText, 23, new Vector2(0.10f, 0.40f), new Vector2(0.90f, 0.49f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateChallengeHelperButtons(cardPanel, challengeIndex, challenge);

            CreateButton(screen, "Encerrar", new Vector2(0.5f, 0.035f), ShowSessionSummary, new Vector2(380f, 82f), SecondaryButtonColor, 24);
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

            CreateButton(screen, "Concluir", new Vector2(0.5f, 0.075f), () => CompleteCurrentPrompt($"Desafio resolvido: {challenge.Title} com {helperName}"), new Vector2(720f, 100f), PrimaryButtonColor, 30);
            CreateButton(screen, "Encerrar", new Vector2(0.5f, 0.035f), ShowSessionSummary, new Vector2(380f, 82f), SecondaryButtonColor, 24);
        }

        private void BuildFinalCard(RectTransform screen)
        {
            ForgottenNamesFinalCard finalCard = ForgottenNamesExpeditionContent.FinalCard;
            RectTransform cardPanel = CreateMainCardPanel(screen, "FinalCard", new Color(0.17f, 0.14f, 0.10f, 0.98f));
            CreateText(cardPanel, "Carta Final da Expedição", 23, new Vector2(0.08f, 0.91f), new Vector2(0.92f, 0.97f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, state.SelectedPremise.FinalBridge, 22, new Vector2(0.08f, 0.79f), new Vector2(0.92f, 0.90f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, finalCard.Title, 38, new Vector2(0.08f, 0.67f), new Vector2(0.92f, 0.78f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, finalCard.Body, 25, new Vector2(0.08f, 0.56f), new Vector2(0.92f, 0.65f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, finalCard.FinalPrompt, 30, new Vector2(0.08f, 0.39f), new Vector2(0.92f, 0.55f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, FinalHelperText, 24, new Vector2(0.10f, 0.31f), new Vector2(0.90f, 0.40f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(cardPanel, finalCard.GroupSentence, 30, new Vector2(0.10f, 0.17f), new Vector2(0.90f, 0.29f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);

            CreateButton(screen, "Encerrar expedição", new Vector2(0.5f, 0.075f), () => CompleteCurrentPrompt("Carta final concluída"), new Vector2(720f, 100f), PrimaryButtonColor, 30);
            CreateButton(screen, "Guia de Campo", new Vector2(0.30f, 0.030f), ShowFieldGuideScreen, new Vector2(400f, 82f), SecondaryButtonColor, 25);
            CreateButton(screen, "Sair", new Vector2(0.70f, 0.030f), ReturnToSelector, new Vector2(320f, 82f), SecondaryButtonColor, 25);
        }

        private void CreateChallengeHelperButtons(RectTransform cardPanel, int challengeIndex, ForgottenNamesChallenge challenge)
        {
            CreateText(cardPanel, "Escolha alguém da Party para ajudar.", 22, new Vector2(0.10f, 0.32f), new Vector2(0.90f, 0.39f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            if (state.PartyScientistIndexes.Count == 0)
            {
                CreateText(cardPanel, "Sem uma figura ativa na Party, o grupo resolve junto. Que esforço extra foi necessário?", 24, new Vector2(0.10f, 0.19f), new Vector2(0.90f, 0.31f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);
                CreateButton(cardPanel, "Resolver em grupo", new Vector2(0.50f, 0.12f), () => ResolveChallenge(challengeIndex, -1), new Vector2(460f, 72f), PrimaryButtonColor, 22);
                return;
            }

            for (int i = 0; i < ForgottenNamesExpeditionState.PartyLimit; i++)
            {
                int scientistIndex = i < state.PartyScientistIndexes.Count ? state.PartyScientistIndexes[i] : -1;
                bool matched = scientistIndex >= 0 && ForgottenNamesExpeditionContent.HasMatchingTag(ForgottenNamesExpeditionContent.Scientists[scientistIndex], challenge);
                float left = 0.08f + (i * 0.29f);
                if (scientistIndex >= 0)
                {
                    int capturedScientistIndex = scientistIndex;
                    CreatePartySlotCard(cardPanel, i, scientistIndex, new Vector2(left, 0.08f), new Vector2(left + 0.26f, 0.30f), true, matched, () => ResolveChallenge(challengeIndex, capturedScientistIndex));
                }
                else
                {
                    CreatePartySlotCard(cardPanel, i, -1, new Vector2(left, 0.08f), new Vector2(left + 0.26f, 0.30f), false, false, null);
                }
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

            RectTransform panel = CreateCardPanel(overlayRect, "PartyFullPanel", new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.90f), new Color(0.08f, 0.11f, 0.17f, 0.98f), true);
            CreateText(panel, "A Party está cheia", 41, new Vector2(0.08f, 0.87f), new Vector2(0.92f, 0.97f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(panel, "Escolha uma figura para mover ao Archive.", 27, new Vector2(0.10f, 0.78f), new Vector2(0.90f, 0.86f), FontStyles.Normal, TextAlignmentOptions.Center);
            RectTransform newCard = CreateCardPanel(panel, "IncomingScientistCard", new Vector2(0.08f, 0.61f), new Vector2(0.92f, 0.76f), new Color(0.11f, 0.18f, 0.18f, 0.98f), true);
            CreateText(newCard, "Chegando à mesa", 18, new Vector2(0.06f, 0.66f), new Vector2(0.94f, 0.94f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft, HighlightTextColor);
            CreateText(newCard, newScientist.Name, 24, new Vector2(0.06f, 0.36f), new Vector2(0.94f, 0.66f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            CreateText(newCard, newScientist.Field + " • " + FormatShortTags(newScientist.Tags), 16, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.35f), FontStyles.Italic, TextAlignmentOptions.MidlineLeft, MutedTextColor);

            CreateText(panel, "Party atual", 22, new Vector2(0.08f, 0.54f), new Vector2(0.92f, 0.60f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft, HighlightTextColor);
            for (int i = 0; i < state.PartyScientistIndexes.Count; i++)
            {
                int partyScientistIndex = state.PartyScientistIndexes[i];
                float left = 0.08f + (i * 0.29f);
                CreatePartySlotCard(panel, i, partyScientistIndex, new Vector2(left, 0.31f), new Vector2(left + 0.26f, 0.52f), false, false, null);
                CreateButton(panel, "Mover", new Vector2(left + 0.13f, 0.27f), () => MovePartyScientistToArchiveAndAddNew(partyScientistIndex, newScientistIndex), new Vector2(170f, 48f), PrimaryButtonColor, 17);
            }

            CreateText(panel, "Todos continuam reconhecidos: a Party ajuda agora, e o Archive guarda nomes para lembrar depois.", 20, new Vector2(0.10f, 0.14f), new Vector2(0.90f, 0.22f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
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
            CreateText(cardPanel, "Nome registrado no Archive.", 26, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(cardPanel, scientist.Name, 42, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.84f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(cardPanel, scientist.ArchivePrompt, 32, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.66f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateText(cardPanel, "Respondam em voz alta. O app apenas guia a conversa.", 25, new Vector2(0.10f, 0.24f), new Vector2(0.90f, 0.34f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateButton(screen, "Registro falado: continuar", new Vector2(0.5f, 0.075f), () => CompleteCurrentPrompt($"Registro no Archive concluído: {scientist.Name}"), new Vector2(720f, 100f), PrimaryButtonColor, 30);
            CreateButton(screen, "Encerrar", new Vector2(0.5f, 0.035f), ShowSessionSummary, new Vector2(380f, 82f), SecondaryButtonColor, 24);
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
            CreateText(screen, "Guia de Campo", 52, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, "Escolha uma figura para ler uma mini bio confortável em tela cheia.", 25, new Vector2(0.10f, 0.78f), new Vector2(0.90f, 0.85f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);

            RectTransform content = CreateScrollContent(screen, "FieldGuideList", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.76f), new Color(0.07f, 0.09f, 0.14f, 0.92f));
            for (int i = 0; i < ForgottenNamesExpeditionContent.Scientists.Count; i++)
            {
                CreateFieldGuideRow(content, i, ForgottenNamesExpeditionContent.Scientists[i]);
            }

            CreateButton(screen, sessionStarted ? "Voltar à mesa" : "Voltar", new Vector2(0.30f, 0.07f), () => { if (sessionStarted) ShowMainCardTable(); else ShowRootScreen(); }, new Vector2(410f, 82f), PrimaryButtonColor, 25);
            CreateButton(screen, "Início", new Vector2(0.70f, 0.07f), ShowRootScreen, new Vector2(320f, 82f), SecondaryButtonColor, 25);
        }

        private void ShowFieldGuideDetailScreen(int scientistIndex, bool returnToList)
        {
            ForgottenNamesScientist scientist = ForgottenNamesExpeditionContent.Scientists[scientistIndex];
            RectTransform screen = BeginScreen();
            CreateText(screen, scientist.Name, 48, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, scientist.Field, 27, new Vector2(0.08f, 0.79f), new Vector2(0.92f, 0.85f), FontStyles.Italic, TextAlignmentOptions.Center, HighlightTextColor);

            RectTransform content = CreateScrollContent(screen, "FieldGuideBio", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.77f), new Color(0.08f, 0.10f, 0.16f, 0.94f));
            CreateBioSection(content, "Por que talvez você não conheça", scientist.HumanHook);
            CreateBioSection(content, "O que mudou", scientist.ShortContribution);
            CreateBioSection(content, "Como ajuda a expedição", scientist.EncounterPrompt);
            CreateBioSection(content, "Use quando", ForgottenNamesExpeditionContent.JoinTags(scientist.Tags));

            if (returnToList)
            {
                CreateButton(screen, "Voltar ao guia", new Vector2(0.30f, 0.07f), ShowFieldGuideScreen, new Vector2(430f, 82f), PrimaryButtonColor, 25);
            }
            else
            {
                CreateButton(screen, "Voltar à carta", new Vector2(0.30f, 0.07f), ShowMainCardTable, new Vector2(430f, 82f), PrimaryButtonColor, 25);
            }

            CreateButton(screen, sessionStarted ? "Mesa" : "Menu", new Vector2(0.70f, 0.07f), () => { if (sessionStarted) ShowMainCardTable(); else ShowRootScreen(); }, new Vector2(320f, 82f), SecondaryButtonColor, 25);
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
            layout.padding = new RectOffset(28, 28, 28, 28);
            layout.spacing = 20f;
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

        private void CreateFieldGuideRow(RectTransform content, int scientistIndex, ForgottenNamesScientist scientist)
        {
            GameObject rowObject = new GameObject("FieldGuideScientist_" + scientist.Name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            RectTransform row = rowObject.GetComponent<RectTransform>();
            row.SetParent(content, false);
            rowObject.GetComponent<Image>().color = CardColor;
            rowObject.GetComponent<Button>().onClick.AddListener(() => ShowFieldGuideDetailScreen(scientistIndex, true));
            rowObject.GetComponent<LayoutElement>().preferredHeight = 178f;

            CreateText(row, scientist.Name, 31, new Vector2(0.06f, 0.62f), new Vector2(0.94f, 0.92f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            CreateText(row, scientist.Field, 24, new Vector2(0.06f, 0.40f), new Vector2(0.94f, 0.62f), FontStyles.Italic, TextAlignmentOptions.MidlineLeft, HighlightTextColor);
            CreateText(row, ForgottenNamesExpeditionContent.JoinTags(scientist.Tags), 21, new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.38f), FontStyles.Normal, TextAlignmentOptions.TopLeft, MutedTextColor);
        }

        private static void CreateBioSection(RectTransform content, string title, string body)
        {
            GameObject sectionObject = new GameObject("FieldGuideSection_" + title, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            RectTransform section = sectionObject.GetComponent<RectTransform>();
            section.SetParent(content, false);
            sectionObject.GetComponent<Image>().color = CardColor;
            sectionObject.GetComponent<LayoutElement>().preferredHeight = 210f;

            CreateText(section, title, 27, new Vector2(0.06f, 0.68f), new Vector2(0.94f, 0.92f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft, HighlightTextColor);
            CreateText(section, body, 25, new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.66f), FontStyles.Normal, TextAlignmentOptions.TopLeft, MutedTextColor);
        }

        private void ShowHowToPlayModal()
        {
            currentHowToPlayPage = Mathf.Clamp(currentHowToPlayPage, 0, HowToPlayPages.Length - 1);
            RenderHowToPlayModal();
        }

        private void RenderHowToPlayModal()
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

            HelpPage page = HowToPlayPages[currentHowToPlayPage];
            RectTransform panel = CreatePanel(overlayRect, "HowToPlayPanel", new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.88f), new Color(0.08f, 0.11f, 0.17f, 0.98f));
            CreateText(panel, "Como jogar", 42, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(panel, $"{currentHowToPlayPage + 1}/{HowToPlayPages.Length}", 24, new Vector2(0.76f, 0.79f), new Vector2(0.92f, 0.85f), FontStyles.Bold, TextAlignmentOptions.MidlineRight, HighlightTextColor);
            CreateText(panel, page.Title, 34, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.82f), FontStyles.Bold, TextAlignmentOptions.Center, HighlightTextColor);
            CreateText(panel, page.Body, 29, new Vector2(0.10f, 0.34f), new Vector2(0.90f, 0.68f), FontStyles.Normal, TextAlignmentOptions.Center, MutedTextColor);

            CreateButton(panel, "Retornar", new Vector2(0.28f, 0.22f), ShowPreviousHowToPlayPage, new Vector2(320f, 78f), currentHowToPlayPage == 0 ? SecondaryButtonColor : PrimaryButtonColor, 24);
            CreateButton(panel, "Avançar", new Vector2(0.72f, 0.22f), ShowNextHowToPlayPage, new Vector2(320f, 78f), currentHowToPlayPage == HowToPlayPages.Length - 1 ? SecondaryButtonColor : PrimaryButtonColor, 24);
            CreateButton(panel, "Fechar", new Vector2(0.5f, 0.10f), CloseModal, new Vector2(420f, 78f), SecondaryButtonColor, 24);
        }

        private void ShowPreviousHowToPlayPage()
        {
            if (currentHowToPlayPage > 0)
            {
                currentHowToPlayPage--;
            }

            RenderHowToPlayModal();
        }

        private void ShowNextHowToPlayPage()
        {
            if (currentHowToPlayPage < HowToPlayPages.Length - 1)
            {
                currentHowToPlayPage++;
            }

            RenderHowToPlayModal();
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
            CreatePanel(screen, "TopStatusStrip", new Vector2(0f, 0.945f), new Vector2(1f, 1f), new Color(0.035f, 0.050f, 0.080f, 0.98f));
            CreateText(screen, $"Jogador {state.CurrentPlayerIndex + 1}: {state.CurrentRole.Title}", 22, new Vector2(0.06f, 0.958f), new Vector2(0.62f, 0.992f), FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            CreateText(screen, $"Carta {Mathf.Min(state.CurrentDeckIndex + 1, state.TotalCards)}/{state.TotalCards}", 22, new Vector2(0.62f, 0.958f), new Vector2(0.94f, 0.992f), FontStyles.Normal, TextAlignmentOptions.MidlineRight);

            RectTransform premiseCard = CreateCardPanel(screen, "PremiseTableCard", new Vector2(0.06f, 0.785f), new Vector2(0.94f, 0.935f), new Color(0.13f, 0.18f, 0.28f, 0.98f), true);
            CreateText(premiseCard, "Premissa da Expedição", 18, new Vector2(0.06f, 0.70f), new Vector2(0.94f, 0.94f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft, HighlightTextColor);
            CreateText(premiseCard, state.SelectedPremise.Title, 24, new Vector2(0.06f, 0.47f), new Vector2(0.94f, 0.71f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            CreateText(premiseCard, state.SelectedPremise.TableReminder, 18, new Vector2(0.06f, 0.25f), new Vector2(0.94f, 0.47f), FontStyles.Normal, TextAlignmentOptions.TopLeft, MutedTextColor);
            CreateText(premiseCard, "Pense em: " + FormatAnswerHints(state.SelectedPremise), 16, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.24f), FontStyles.Italic, TextAlignmentOptions.TopLeft, HighlightTextColor);
        }

        private void CreatePartyArchiveAreas(RectTransform screen)
        {
            CreateText(screen, "Party — ajuda agora", 20, new Vector2(0.06f, 0.326f), new Vector2(0.55f, 0.354f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft, HighlightTextColor);
            for (int i = 0; i < ForgottenNamesExpeditionState.PartyLimit; i++)
            {
                int scientistIndex = i < state.PartyScientistIndexes.Count ? state.PartyScientistIndexes[i] : -1;
                float left = 0.06f + (i * 0.30f);
                CreatePartySlotCard(screen, i, scientistIndex, new Vector2(left, 0.215f), new Vector2(left + 0.28f, 0.322f), false, false, null);
            }

            RectTransform archive = CreateCardPanel(screen, "ArchiveRememberedStack", new Vector2(0.06f, 0.125f), new Vector2(0.94f, 0.195f), new Color(0.10f, 0.10f, 0.16f, 0.96f), false);
            CreateText(archive, "Archive — será lembrado depois", 19, new Vector2(0.05f, 0.52f), new Vector2(0.76f, 0.93f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft, HighlightTextColor);
            CreateText(archive, $"▰▰▰  {state.ArchiveScientistIndexes.Count} nome(s) registrado(s)", 18, new Vector2(0.05f, 0.10f), new Vector2(0.95f, 0.52f), FontStyles.Normal, TextAlignmentOptions.MidlineLeft, MutedTextColor);
            if (state.ArchiveScientistIndexes.Count > 0)
            {
                CreateText(archive, FormatInlineScientistList(state.ArchiveScientistIndexes), 15, new Vector2(0.42f, 0.08f), new Vector2(0.96f, 0.90f), FontStyles.Italic, TextAlignmentOptions.MidlineRight, MutedTextColor);
            }
        }

        private void CreatePartySlotCard(RectTransform parent, int slotIndex, int scientistIndex, Vector2 anchorMin, Vector2 anchorMax, bool selectable, bool highlighted, UnityEngine.Events.UnityAction onClick)
        {
            Color color = scientistIndex >= 0
                ? highlighted ? new Color(0.17f, 0.36f, 0.25f, 0.98f) : new Color(0.09f, 0.15f, 0.20f, 0.98f)
                : new Color(0.07f, 0.09f, 0.13f, 0.78f);
            RectTransform slot = CreateCardPanel(parent, "PartySlot_" + slotIndex, anchorMin, anchorMax, color, highlighted);
            if (selectable && onClick != null)
            {
                Button slotButton = slot.gameObject.AddComponent<Button>();
                slotButton.onClick.AddListener(onClick);
            }

            if (scientistIndex < 0)
            {
                CreateText(slot, "Espaço livre", 18, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.68f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
                return;
            }

            ForgottenNamesScientist scientist = ForgottenNamesExpeditionContent.Scientists[scientistIndex];
            CreateText(slot, scientist.Name, 17, new Vector2(0.06f, 0.58f), new Vector2(0.94f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(slot, scientist.Field, 13, new Vector2(0.06f, 0.36f), new Vector2(0.94f, 0.58f), FontStyles.Italic, TextAlignmentOptions.Center, MutedTextColor);
            CreateText(slot, FormatShortTags(scientist.Tags), 12, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.34f), FontStyles.Normal, TextAlignmentOptions.Center, highlighted ? HighlightTextColor : MutedTextColor);
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
            return CreateCardPanel(screen, name, new Vector2(0.06f, 0.36f), new Vector2(0.94f, 0.76f), color, true);
        }

        private static string FormatAnswerHints(ForgottenNamesPremise premise)
        {
            return ForgottenNamesExpeditionContent.JoinTags(premise.AnswerHints);
        }

        private static string FormatInlineScientistList(List<int> indexes)
        {
            if (indexes == null || indexes.Count == 0) return string.Empty;
            List<string> names = new List<string>();
            foreach (int index in indexes)
            {
                names.Add(ForgottenNamesExpeditionContent.Scientists[index].Name);
            }

            return string.Join(" • ", names);
        }

        private static string FormatShortTags(string[] tags)
        {
            if (tags == null || tags.Length == 0) return string.Empty;
            int count = Mathf.Min(3, tags.Length);
            List<string> shortTags = new List<string>();
            for (int i = 0; i < count; i++)
            {
                shortTags.Add(tags[i]);
            }

            return string.Join(" • ", shortTags);
        }

        private void CloseModal()
        {
            if (modalOverlay == null) return;
            modalOverlay.SetActive(false);
            Object.Destroy(modalOverlay);
            modalOverlay = null;
        }

        private static RectTransform CreateCardPanel(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color, bool emphasized)
        {
            Vector2 shadowOffset = emphasized ? new Vector2(0.012f, -0.010f) : new Vector2(0.008f, -0.007f);
            CreatePanel(parent, name + "Shadow", anchorMin + shadowOffset, anchorMax + shadowOffset, new Color(0f, 0f, 0f, emphasized ? 0.34f : 0.22f));
            RectTransform border = CreatePanel(parent, name + "Border", anchorMin - new Vector2(0.004f, 0.004f), anchorMax + new Vector2(0.004f, 0.004f), emphasized ? HighlightTextColor : new Color(0.30f, 0.34f, 0.42f, 1f));
            RectTransform card = CreatePanel(parent, name, anchorMin, anchorMax, color);
            border.SetAsFirstSibling();
            card.SetAsLastSibling();
            return card;
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
