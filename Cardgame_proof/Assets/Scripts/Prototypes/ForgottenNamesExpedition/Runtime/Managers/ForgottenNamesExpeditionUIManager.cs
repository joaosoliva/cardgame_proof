using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardgameProof.App;

namespace CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime.Managers
{
    public sealed class ForgottenNamesExpeditionUIManager
    {
        private const string PrototypeTitle = "A Expedição dos Nomes Esquecidos";
        private const string PrototypeSubtitle = "Um jogo narrativo sobre ciência, memória e nomes que quase esquecemos.";
        private const string PrototypeDescription = "Em 15 minutos, crie uma pequena expedição, encontre figuras científicas pouco conhecidas e decida como lembrar suas contribuições.";

        private PrototypeRuntimeContext context;
        private GameObject root;
        private GameObject modalOverlay;

        public void Initialize(PrototypeRuntimeContext runtimeContext)
        {
            context = runtimeContext;
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

            context = null;
        }

        private void ShowRootScreen()
        {
            EnsureRoot();
            RectTransform screen = root.GetComponent<RectTransform>();
            ClearChildren(screen);

            CreatePanel(screen, "HeroPanel", new Vector2(0.07f, 0.56f), new Vector2(0.93f, 0.92f), new Color(0.10f, 0.13f, 0.20f, 0.96f));
            CreateText(screen, PrototypeTitle, 48, new Vector2(0.10f, 0.79f), new Vector2(0.90f, 0.90f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, PrototypeSubtitle, 28, new Vector2(0.12f, 0.69f), new Vector2(0.88f, 0.78f), FontStyles.Italic, TextAlignmentOptions.Center);
            CreateText(screen, PrototypeDescription, 27, new Vector2(0.11f, 0.57f), new Vector2(0.89f, 0.68f), FontStyles.Normal, TextAlignmentOptions.Center);

            CreateText(screen, "Protótipo vazio: esta tela valida o fluxo de entrada, leitura em retrato e retorno seguro ao seletor.", 24, new Vector2(0.12f, 0.49f), new Vector2(0.88f, 0.55f), FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.82f, 0.88f, 0.96f, 1f));

            CreateButton(screen, "Iniciar teste rápido", new Vector2(0.5f, 0.40f), ShowQuickTestSetup);
            CreateButton(screen, "Como jogar", new Vector2(0.5f, 0.30f), ShowHowToPlayModal);
            CreateButton(screen, "Guia de Campo", new Vector2(0.5f, 0.20f), ShowFieldGuideScreen);
            CreateButton(screen, "Voltar", new Vector2(0.5f, 0.10f), ReturnToSelector, new Vector2(640f, 86f), new Color(0.24f, 0.27f, 0.33f, 1f));
        }

        private void ShowQuickTestSetup()
        {
            EnsureRoot();
            RectTransform screen = root.GetComponent<RectTransform>();
            ClearChildren(screen);

            CreateText(screen, "Teste rápido", 52, new Vector2(0.10f, 0.78f), new Vector2(0.90f, 0.90f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, "Placeholder de preparação", 30, new Vector2(0.12f, 0.70f), new Vector2(0.88f, 0.77f), FontStyles.Italic, TextAlignmentOptions.Center);
            CreatePanel(screen, "SetupPlaceholderPanel", new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.66f), new Color(0.12f, 0.16f, 0.24f, 0.96f));
            CreateText(screen, "Em breve: escolher duração, montar uma pequena expedição e preparar as primeiras pistas.\n\nPor enquanto, esta tela confirma que o novo protótipo abre sem tocar nos protótipos existentes.", 30, new Vector2(0.13f, 0.38f), new Vector2(0.87f, 0.62f), FontStyles.Normal, TextAlignmentOptions.Center);

            CreateButton(screen, "Voltar ao início", new Vector2(0.5f, 0.22f), ShowRootScreen);
            CreateButton(screen, "Voltar", new Vector2(0.5f, 0.12f), ReturnToSelector, new Vector2(640f, 86f), new Color(0.24f, 0.27f, 0.33f, 1f));
        }

        private void ShowFieldGuideScreen()
        {
            EnsureRoot();
            RectTransform screen = root.GetComponent<RectTransform>();
            ClearChildren(screen);

            CreateText(screen, "Guia de Campo", 52, new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.90f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(screen, "Placeholder", 30, new Vector2(0.12f, 0.73f), new Vector2(0.88f, 0.79f), FontStyles.Italic, TextAlignmentOptions.Center);
            CreatePanel(screen, "GuidePlaceholderPanel", new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.68f), new Color(0.12f, 0.16f, 0.24f, 0.96f));
            CreateText(screen, "Aqui ficará o guia com nomes, áreas de pesquisa, pistas narrativas e perguntas para lembrar contribuições científicas pouco conhecidas.", 31, new Vector2(0.13f, 0.38f), new Vector2(0.87f, 0.62f), FontStyles.Normal, TextAlignmentOptions.Center);

            CreateButton(screen, "Voltar ao início", new Vector2(0.5f, 0.20f), ShowRootScreen);
            CreateButton(screen, "Voltar", new Vector2(0.5f, 0.10f), ReturnToSelector, new Vector2(640f, 86f), new Color(0.24f, 0.27f, 0.33f, 1f));
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

            RectTransform panel = CreatePanel(overlayRect, "HowToPlayPanel", new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.82f), new Color(0.08f, 0.11f, 0.17f, 0.98f));
            CreateText(panel, "Como jogar", 46, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.92f), FontStyles.Bold, TextAlignmentOptions.Center);
            CreateText(panel, "Placeholder: cada jogador irá explorar pistas, propor conexões e decidir como registrar uma contribuição científica esquecida.\n\nNesta etapa ainda não há regras finais, cartas ou pontuação implementadas.", 28, new Vector2(0.10f, 0.28f), new Vector2(0.90f, 0.74f), FontStyles.Normal, TextAlignmentOptions.Center);
            CreateButton(panel, "Fechar", new Vector2(0.5f, 0.16f), CloseModal, new Vector2(420f, 82f), new Color(0.18f, 0.48f, 0.86f, 1f));
        }

        private void ReturnToSelector()
        {
            context?.ReturnToSelector?.Invoke();
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
            root.GetComponent<Image>().color = new Color(0.055f, 0.075f, 0.12f, 1f);
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
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            return CreateButton(parent, label, anchor, onClick, new Vector2(720f, 92f), new Color(0.18f, 0.48f, 0.86f, 1f));
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick, Vector2 size, Color color)
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

            CreateText(rect, label, 30, Vector2.zero, Vector2.one, FontStyles.Bold, TextAlignmentOptions.Center);
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
    }
}
