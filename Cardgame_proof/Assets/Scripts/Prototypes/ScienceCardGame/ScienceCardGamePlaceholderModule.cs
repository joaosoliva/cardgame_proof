using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardgameProof.App;

namespace CardgameProof.Prototypes.ScienceCardGame
{
    public sealed class ScienceCardGamePlaceholderModule : IPrototypeModule
    {
        private GameObject root;
        private PrototypeRuntimeContext context;

        public void StartPrototype(PrototypeRuntimeContext runtimeContext)
        {
            context = runtimeContext;
            if (context?.SceneRoot?.FullScreenRoot == null)
            {
                Debug.LogWarning("[Prototype] Cannot start Science Card Game placeholder: runtime context is missing.");
                return;
            }

            root = new GameObject("ScienceCardGamePlaceholderRoot", typeof(RectTransform), typeof(Image));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.SetParent(context.SceneRoot.FullScreenRoot, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.13f, 1f);

            CreateText(rect, "Science Card Game", 58, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.82f), FontStyles.Bold);
            CreateText(rect, "Placeholder registrado para a futura simulação tabletop de conexões entre personagens científicos.\n\nA arquitetura de múltiplos protótipos já está pronta; o jogo completo ainda não foi implementado.", 30, new Vector2(0.1f, 0.36f), new Vector2(0.9f, 0.64f), FontStyles.Normal);
            CreateButton(rect, "Voltar para seleção", new Vector2(0.5f, 0.22f), () => context.ReturnToSelector?.Invoke());
        }

        public void StopPrototype()
        {
            if (root != null)
            {
                Object.Destroy(root);
                root = null;
            }

            context = null;
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
        }

        private static void CreateButton(RectTransform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObj = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 120f);
            buttonObj.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.82f, 1f);

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.SetParent(buttonObj.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = labelObj.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 34;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
        }
    }
}
