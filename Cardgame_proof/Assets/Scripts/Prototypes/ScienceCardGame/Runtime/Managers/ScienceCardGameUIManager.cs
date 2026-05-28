using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardgameProof.App;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Managers
{
    public sealed class ScienceCardGameUIManager
    {
        private PrototypeRuntimeContext context;
        private ScienceCardGameState state;
        private ScienceDeckManager deckManager;
        private ScienceTurnManager turnManager;
        private ScienceTelemetryManager telemetry;
        private GameObject root;
        private TextMeshProUGUI selectedPlayerCountText;
        private int selectedPlayerCount = 2;

        public GameObject Root => root;

        public void Initialize(
            PrototypeRuntimeContext runtimeContext,
            ScienceCardGameState gameState,
            ScienceDeckManager scienceDeckManager,
            ScienceTurnManager scienceTurnManager,
            ScienceTelemetryManager telemetryManager,
            Action<int> onStartGame)
        {
            context = runtimeContext;
            state = gameState;
            deckManager = scienceDeckManager;
            turnManager = scienceTurnManager;
            telemetry = telemetryManager;
            selectedPlayerCount = state?.SelectedPlayerCount ?? 2;

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
            ClearChildren(root.transform);
            RectTransform rect = root.GetComponent<RectTransform>();

            CreateText(rect, "Distribuição de Cartas", 54, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.86f), FontStyles.Bold);
            CreateText(rect, BuildDistributionSummary(), 30, new Vector2(0.10f, 0.42f), new Vector2(0.90f, 0.68f), FontStyles.Normal);
            CreateText(rect, "Próxima etapa: posicionamento no tabuleiro ainda não implementado.", 24, new Vector2(0.12f, 0.32f), new Vector2(0.88f, 0.42f), FontStyles.Italic);
            CreateButton(rect, "Back to Prototype Selection", new Vector2(0.5f, 0.20f), () => context?.ReturnToSelector?.Invoke());
            telemetry?.LogEvent("science_ui_screen_changed", "screen=card_distribution");
        }

        public void Cleanup()
        {
            if (root != null)
            {
                root.SetActive(false);
                Object.Destroy(root);
                root = null;
            }

            selectedPlayerCountText = null;
            context = null;
            state = null;
            deckManager = null;
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

        private string BuildDistributionSummary()
        {
            int playerCount = state?.Players?.Count ?? 0;
            int deckCount = deckManager?.DrawPile?.Count ?? 0;
            int turn = turnManager?.TurnNumber ?? 0;
            int handSize = state?.InitialHandSize ?? 0;
            return $"{playerCount} jogadores inicializados. Cada jogador recebeu {handSize} cartas. Cartas restantes no deck: {deckCount}. Turno inicial: {turn}.";
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

        private static TextMeshProUGUI CreateText(RectTransform parent, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style)
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
            text.alignment = TextAlignmentOptions.Center;
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
            text.fontSize = size.x < 300f ? 24 : 34;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
        }
    }
}
