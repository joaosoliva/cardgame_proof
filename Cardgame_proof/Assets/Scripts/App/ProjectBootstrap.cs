using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CardgameProof.Bootstrap;

namespace CardgameProof.App
{
    public sealed class ProjectBootstrap : MonoBehaviour
    {
        private const string RootCanvasName = "RootCanvas";

        private Canvas rootCanvas;
        private SceneRootBuilder rootBuilder;
        private PrototypeSelectorView selectorView;
        private IPrototypeModule activePrototype;
        private PrototypeRuntimeContext runtimeContext;

        public void Initialize()
        {
            Debug.Log("[ProjectBootstrap] Initialize begin");
            ValidateTextMeshProResources();
            EnsurePortraitResolution();
            EnsureEventSystem();

            Core.AudioManager audioManager = Core.AudioManager.EnsureInstance();
            Debug.Log(audioManager == null ? "[ProjectBootstrap] AudioManager unavailable. Continuing without audio." : "[ProjectBootstrap] AudioManager ready");

            rootCanvas = EnsureRootCanvas();
            rootBuilder = rootCanvas.gameObject.GetComponent<SceneRootBuilder>();
            if (rootBuilder == null)
            {
                rootBuilder = rootCanvas.gameObject.AddComponent<SceneRootBuilder>();
            }

            rootBuilder.Build();
            runtimeContext = new PrototypeRuntimeContext(rootCanvas, rootBuilder, ShowPrototypeSelector);
            ShowPrototypeSelector();
        }

        private void ShowPrototypeSelector()
        {
            StopActivePrototype();
            ClearSceneRootChildren();
            rootBuilder.Build();

            if (selectorView == null)
            {
                GameObject selectorObject = new GameObject("PrototypeSelectorView");
                selectorObject.transform.SetParent(rootCanvas.transform, false);
                selectorView = selectorObject.AddComponent<PrototypeSelectorView>();
            }

            selectorView.Show(rootCanvas.GetComponent<RectTransform>(), PrototypeRegistry.All, StartPrototype);
        }

        private void StartPrototype(PrototypeDefinition definition)
        {
            if (definition == null || definition.CreateModule == null) return;

            selectorView?.Hide();
            ClearSceneRootChildren();
            rootBuilder.Build();

            activePrototype = definition.CreateModule.Invoke();
            Debug.Log($"[ProjectBootstrap] Starting prototype: {definition.Id}");
            activePrototype.StartPrototype(runtimeContext);
        }

        private void StopActivePrototype()
        {
            if (activePrototype == null) return;

            Debug.Log("[ProjectBootstrap] Stopping active prototype");
            activePrototype.StopPrototype();
            activePrototype = null;
        }

        private void ClearSceneRootChildren()
        {
            if (rootBuilder?.FullScreenRoot == null) return;

            RectTransform fullRoot = rootBuilder.FullScreenRoot;
            for (int i = fullRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = fullRoot.GetChild(i);
                if (child == rootBuilder.SafeAreaRoot) continue;
                Destroy(child.gameObject);
            }

            ClearChildren(rootBuilder.TopArea);
            ClearChildren(rootBuilder.CenterBoardArea);
            ClearChildren(rootBuilder.ActionArea);
            ClearChildren(rootBuilder.BottomCardTray);
            ClearChildren(rootBuilder.OverlayLayer);
        }

        private void ClearChildren(RectTransform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private static void ValidateTextMeshProResources()
        {
            if (TMP_Settings.instance == null || TMP_Settings.defaultFontAsset == null)
            {
                Debug.LogError("TextMeshPro resources missing. Import TMP Essential Resources from Window > TextMeshPro > Import TMP Essential Resources.");
            }
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

            System.Type inputSystemModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                eventSystemObject.AddComponent(inputSystemModuleType);
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
