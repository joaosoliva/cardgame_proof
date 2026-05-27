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
            Debug.Log("[Bootstrap] GameBootstrap.Start begin");
            EnsurePortraitResolution();
            EnsureEventSystem();
            Core.AudioManager audioManager = Core.AudioManager.EnsureInstance();
            if (audioManager == null)
            {
                Debug.LogWarning("[Bootstrap] AudioManager unavailable. Continuing without audio.");
            }
            else
            {
                Debug.Log("[Bootstrap] AudioManager ready");
            }

            Canvas rootCanvas = EnsureRootCanvas();
            SceneRootBuilder rootBuilder = rootCanvas.gameObject.GetComponent<SceneRootBuilder>();
            if (rootBuilder == null)
            {
                rootBuilder = rootCanvas.gameObject.AddComponent<SceneRootBuilder>();
            }
            rootBuilder.Build();
            Debug.Log("[Bootstrap] SceneRootBuilder.Build complete");

            Core.GameController gameController = FindFirstObjectByType<Core.GameController>();
            if (gameController == null)
            {
                GameObject controllerObject = new GameObject("GameController");
                gameController = controllerObject.AddComponent<Core.GameController>();
            }

            Debug.Log("[Bootstrap] Initializing main menu flow");
            gameController.InitializeMainMenu(rootBuilder);
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
