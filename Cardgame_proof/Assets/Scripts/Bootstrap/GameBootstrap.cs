using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardgameProof.Bootstrap
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const string RootCanvasName = "RootCanvas";

        private void Start()
        {
            try
            {
                EnsurePortraitResolution();
                EnsureEventSystem();
                Core.AudioManager.EnsureInstance();

                Canvas rootCanvas = EnsureRootCanvas();
                SceneRootBuilder rootBuilder = rootCanvas.gameObject.GetComponent<SceneRootBuilder>();
                if (rootBuilder == null)
                {
                    rootBuilder = rootCanvas.gameObject.AddComponent<SceneRootBuilder>();
                }
                rootBuilder.Build();

                Core.GameController gameController = FindFirstObjectByType<Core.GameController>();
                if (gameController == null)
                {
                    GameObject controllerObject = new GameObject("GameController");
                    gameController = controllerObject.AddComponent<Core.GameController>();
                }

                gameController.InitializeMainMenu(rootBuilder);
            }
            catch (System.Exception ex)
            {
                ShowBootError(ex.Message);
            }
        }
        
        private static void ShowBootError(string message)
        {
            Canvas canvas = EnsureRootCanvas();
            GameObject panel = new GameObject("BootErrorPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(32f, 64f); rt.offsetMax = new Vector2(-32f, -64f);
            panel.GetComponent<Image>().color = new Color(0.2f, 0.05f, 0.05f, 0.95f);
            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 16f;
            CreateErrorText(panel.transform, "Falha ao inicializar o protótipo.", 48);
            CreateErrorText(panel.transform, "Reinicie o app. Se persistir, verifique logs de build.", 32);
            CreateErrorText(panel.transform, message, 24);
        }
        private static void CreateErrorText(Transform parent, string text, int size)
        {
            GameObject go = new GameObject("ErrorText", typeof(RectTransform), typeof(LayoutElement), typeof(Text));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 180f;
            Text t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.text = text;
        }

        private static void EnsurePortraitResolution()
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

            if (System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem") != null)
            {
                eventSystemObject.AddComponent(
                    System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"));
            }
            else
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
        }

        private static Canvas EnsureRootCanvas()
        {
            Canvas existingCanvas = FindFirstObjectByType<Canvas>();
            if (existingCanvas != null)
            {
                EnsureCanvasScaler(existingCanvas);
                EnsureGraphicRaycaster(existingCanvas.gameObject);
                return existingCanvas;
            }

            GameObject canvasObject = new GameObject(RootCanvasName);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            EnsureCanvasScaler(canvas);
            EnsureGraphicRaycaster(canvasObject);

            return canvas;
        }

        private static void EnsureCanvasScaler(Canvas canvas)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
        }

        private static void EnsureGraphicRaycaster(GameObject canvasObject)
        {
            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }
        }
    }
}
