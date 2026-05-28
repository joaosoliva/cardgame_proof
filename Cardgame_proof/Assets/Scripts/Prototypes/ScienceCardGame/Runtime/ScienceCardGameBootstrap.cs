using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardgameProof.App;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime
{
    public sealed class ScienceCardGameBootstrap : MonoBehaviour
    {
        private PrototypeRuntimeContext context;
        private ScienceCardGameState state;
        private GameObject root;

        public void Initialize(PrototypeRuntimeContext runtimeContext, ScienceCardGameState initialState)
        {
            context = runtimeContext;
            state = initialState ?? new ScienceCardGameState();

            if (context?.SceneRoot?.FullScreenRoot == null)
            {
                Debug.LogWarning("[ScienceCardGame] Bootstrap initialization failed: missing scene root.");
                return;
            }

            BuildPlaceholderScreen(context.SceneRoot.FullScreenRoot);
        }

        public void Cleanup()
        {
            if (root != null)
            {
                root.SetActive(false);
                Destroy(root);
                root = null;
            }

            context = null;
            state = null;
        }

        private void BuildPlaceholderScreen(RectTransform parent)
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

            CreateText(rect, state.PrototypeTitle, 56, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.82f), FontStyles.Bold);
            CreateText(rect, state.Description, 30, new Vector2(0.10f, 0.38f), new Vector2(0.90f, 0.62f), FontStyles.Normal);
            CreateText(rect, "Este módulo está isolado do protótipo existente e será expandido em etapas futuras.", 24, new Vector2(0.12f, 0.30f), new Vector2(0.88f, 0.38f), FontStyles.Italic);
            CreateButton(rect, "Voltar para seleção", new Vector2(0.5f, 0.20f), () => context?.ReturnToSelector?.Invoke());
        }

        private static void CreateText(RectTransform parent, string value, int size, Vector2 anchorMin, Vector2 anchorMax, FontStyles style)
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
        }

        private static void CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 120f);

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
            text.fontSize = 34;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
        }
    }
}
