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
            new TutorialStep
            {
                Id = "welcome_archive",
                Title = "Bem-vindo ao Arquivo",
                Body = "Neste jogo, vocês investigam personagens importantes da ciência e da academia. O objetivo é descobrir quem está escondido no arquivo do outro jogador.",
                Phase = GamePhase.TutorialIntro,
                TargetKey = "main_menu",
                OnlyShowOnce = true
            },
            new TutorialStep
            {
                Id = "build_archive",
                Title = "Monte seu arquivo",
                Body = "Arraste suas cartas para a grade. Personagens e cartas de arquivo ficarão escondidos do adversário.",
                Phase = GamePhase.Setup,
                TargetKey = "board_grid",
                OnlyShowOnce = true
            },
            new TutorialStep
            {
                Id = "character_cards",
                Title = "Cartas de Personagem",
                Body = "Cada personagem representa uma figura acadêmica. Durante a partida, o adversário poderá pedir pistas para tentar identificá-la.",
                Phase = GamePhase.Investigation,
                TargetKey = "character_cards",
                OnlyShowOnce = true
            },
            new TutorialStep
            {
                Id = "archive_cards",
                Title = "Cartas de Arquivo",
                Body = "Cartas de arquivo não são vazias. Elas representam lacunas, fragmentos ou referências e sempre geram alguma informação ou recurso.",
                Phase = GamePhase.Investigation,
                TargetKey = "archive_cards",
                OnlyShowOnce = true
            }
        };

        private SceneRootBuilder sceneRoot;
        private TutorialOverlayView tutorialOverlay;

        public GameModeConfig ActiveModeConfig { get; private set; }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.MainMenu;

        public void LoadPrototypeMode(string modeId)
        {
            ActiveModeConfig = PrototypeDatabase.GetMode(modeId);
            if (ActiveModeConfig == null)
            {
                Debug.LogWarning($"Prototype mode '{modeId}' not found.");
                return;
            }

            Debug.Log($"Loaded prototype mode: {ActiveModeConfig.DisplayName} ({ActiveModeConfig.DurationMinutes} min)");
            Debug.Log($"Prototype database has {PrototypeDatabase.Characters.Count} characters and {PrototypeDatabase.ArchiveCards.Count} archive card types.");
        }

        public void InitializeMainMenu(SceneRootBuilder builtSceneRoot)
        {
            if (builtSceneRoot == null || builtSceneRoot.FullScreenRoot == null)
            {
                Debug.LogWarning("Main menu initialization skipped: SceneRootBuilder is not ready.");
                return;
            }

            sceneRoot = builtSceneRoot;
            EnsureTutorialOverlay();

            RectTransform fullRoot = sceneRoot.FullScreenRoot;
            Transform existing = fullRoot.Find("MainMenuRoot");
            if (existing != null)
            {
                return;
            }

            GameObject menuRootObject = new GameObject("MainMenuRoot", typeof(RectTransform), typeof(Image));
            RectTransform menuRoot = menuRootObject.GetComponent<RectTransform>();
            menuRoot.SetParent(fullRoot, false);
            menuRoot.anchorMin = Vector2.zero;
            menuRoot.anchorMax = Vector2.one;
            menuRoot.offsetMin = Vector2.zero;
            menuRoot.offsetMax = Vector2.zero;

            Image bg = menuRoot.GetComponent<Image>();
            bg.color = new Color(0.08f, 0.11f, 0.16f, 1f);

            VerticalLayoutGroup layout = menuRootObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(56, 56, 120, 80);
            layout.spacing = 28f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            CreateTitle(menuRoot, "Nosso jogo, diversão ilimitada");
            CreateModeButton(menuRoot, "Partida Rápida — 5 min", "5min");
            CreateModeButton(menuRoot, "Partida Completa — 10 min", "10min");
            CreateFooter(menuRoot, "Protótipo digital para teste de jogo físico");

            CurrentPhase = GamePhase.MainMenu;
        }

        public void ShowTutorialSequence(IReadOnlyList<TutorialStep> sequence)
        {
            EnsureTutorialOverlay();
            tutorialOverlay?.ShowSequence(sequence);
        }

        private void OnModeSelected(string modeId)
        {
            PlayButtonClickIfAudioManagerExists();
            LoadPrototypeMode(modeId);
            if (ActiveModeConfig == null)
            {
                return;
            }

            TransitionToTutorialIntro();
            ShowTutorialSequence(DefaultTutorialSteps);
        }

        private void TransitionToTutorialIntro()
        {
            CurrentPhase = GamePhase.TutorialIntro;
            Debug.Log($"Transitioning to TutorialIntro with mode '{ActiveModeConfig.Id}' ({ActiveModeConfig.DisplayName}).");
        }

        private void EnsureTutorialOverlay()
        {
            if (sceneRoot == null || sceneRoot.OverlayLayer == null)
            {
                return;
            }

            if (tutorialOverlay != null)
            {
                return;
            }

            GameObject overlayObject = new GameObject("TutorialOverlayView");
            overlayObject.transform.SetParent(sceneRoot.OverlayLayer, false);
            tutorialOverlay = overlayObject.AddComponent<TutorialOverlayView>();
            tutorialOverlay.Initialize(sceneRoot.OverlayLayer);
        }

        private static void PlayButtonClickIfAudioManagerExists()
        {
            Type audioManagerType = Type.GetType("AudioManager");
            if (audioManagerType == null)
            {
                return;
            }

            UnityEngine.Object manager = FindFirstObjectByType(audioManagerType);
            if (manager == null)
            {
                return;
            }

            audioManagerType.GetMethod("PlayButtonClick")?.Invoke(manager, null);
        }

        private void CreateModeButton(RectTransform parent, string label, string modeId)
        {
            GameObject buttonObj = new GameObject($"{modeId}_Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.SetParent(parent, false);

            LayoutElement layout = buttonObj.GetComponent<LayoutElement>();
            layout.preferredHeight = 180f;

            Image image = buttonObj.GetComponent<Image>();
            image.color = new Color(0.19f, 0.46f, 0.88f, 1f);

            Button button = buttonObj.GetComponent<Button>();
            string capturedModeId = modeId;
            button.onClick.AddListener(() => OnModeSelected(capturedModeId));

            GameObject labelObj = new GameObject("Label", typeof(RectTransform));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.SetParent(buttonRect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(20f, 20f);
            labelRect.offsetMax = new Vector2(-20f, -20f);

            if (!TryCreateTextMeshPro(labelObj, label, 52f, 514))
            {
                Text text = labelObj.AddComponent<Text>();
                text.text = label;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 52;
                text.color = Color.white;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
        }

        private static void CreateTitle(RectTransform parent, string textValue)
        {
            GameObject titleObj = new GameObject("MainMenuTitle", typeof(RectTransform), typeof(LayoutElement));
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.SetParent(parent, false);
            LayoutElement layout = titleObj.GetComponent<LayoutElement>();
            layout.preferredHeight = 280f;

            if (!TryCreateTextMeshPro(titleObj, textValue, 82f, 514))
            {
                Text text = titleObj.AddComponent<Text>();
                text.text = textValue;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 82;
                text.color = Color.white;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        private static void CreateFooter(RectTransform parent, string textValue)
        {
            GameObject spacer = new GameObject("FooterSpacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(parent, false);
            spacer.GetComponent<LayoutElement>().flexibleHeight = 1f;

            GameObject footerObj = new GameObject("MainMenuFooter", typeof(RectTransform), typeof(LayoutElement));
            RectTransform footerRect = footerObj.GetComponent<RectTransform>();
            footerRect.SetParent(parent, false);
            footerObj.GetComponent<LayoutElement>().preferredHeight = 120f;

            if (!TryCreateTextMeshPro(footerObj, textValue, 38f, 514))
            {
                Text text = footerObj.AddComponent<Text>();
                text.text = textValue;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 38;
                text.color = new Color(0.84f, 0.88f, 0.92f, 1f);
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        private static bool TryCreateTextMeshPro(GameObject go, string textValue, float fontSize, int alignment)
        {
            Type tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (tmpType == null)
            {
                return false;
            }

            Component tmpComponent = go.AddComponent(tmpType);
            tmpType.GetProperty("text")?.SetValue(tmpComponent, textValue);
            tmpType.GetProperty("fontSize")?.SetValue(tmpComponent, fontSize);
            tmpType.GetProperty("alignment")?.SetValue(tmpComponent, alignment);
            tmpType.GetProperty("color")?.SetValue(tmpComponent, Color.white);
            return true;
        }
    }
}
