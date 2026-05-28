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
        private Button returnToSelectorButton;
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
            HideReturnToSelectorButton();
            StopActivePrototype();
            ClearSceneRootChildren();
            rootBuilder.Build();

            if (selectorView == null)
            {
                GameObject selectorObject = new GameObject("PrototypeSelectorView");
                selectorObject.transform.SetParent(rootCanvas.transform, false);
                selectorView = selectorObject.AddComponent<PrototypeSelectorView>();
            }

            selectorView.Show(rootCanvas.GetComponent<RectTransform>(), PrototypeRegistry.All, StartPrototype, QuitApplication);
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
            ShowReturnToSelectorButton();
        }

        private void StopActivePrototype()
        {
            if (activePrototype == null) return;

            Debug.Log("[ProjectBootstrap] Stopping active prototype");
            activePrototype.StopPrototype();
            activePrototype = null;
        }

        private void ShowReturnToSelectorButton()
        {
            if (rootCanvas == null) return;

            if (returnToSelectorButton == null)
            {
                GameObject buttonObject = new GameObject("DebugReturnToPrototypeSelectorButton", typeof(RectTransform), typeof(Image), typeof(Button));
                RectTransform rect = buttonObject.GetComponent<RectTransform>();
                rect.SetParent(rootCanvas.transform, false);
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(24f, -24f);
                rect.sizeDelta = new Vector2(300f, 76f);
                buttonObject.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.07f, 0.82f);

                returnToSelectorButton = buttonObject.GetComponent<Button>();
                returnToSelectorButton.onClick.AddListener(ShowPrototypeSelector);

                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(buttonObject.transform, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
                label.text = "← Protótipos";
                label.fontSize = 24;
                label.fontStyle = FontStyles.Bold;
                label.alignment = TextAlignmentOptions.Center;
                label.color = Color.white;
                label.raycastTarget = false;
            }

            returnToSelectorButton.gameObject.SetActive(true);
            returnToSelectorButton.transform.SetAsLastSibling();
        }

        private void HideReturnToSelectorButton()
        {
            if (returnToSelectorButton != null)
            {
                returnToSelectorButton.gameObject.SetActive(false);
            }
        }

        private static void QuitApplication()
        {
            Debug.Log("[ProjectBootstrap] Quit requested from prototype selector.");
            Application.Quit();
        }

        private void ClearSceneRootChildren()
        {
            if (rootBuilder?.FullScreenRoot == null) return;

            RectTransform fullRoot = rootBuilder.FullScreenRoot;
            for (int i = fullRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = fullRoot.GetChild(i);
                if (child == rootBuilder.SafeAreaRoot) continue;
                child.gameObject.SetActive(false);
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
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
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
