using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.Bootstrap
{
    public sealed class SceneRootBuilder : MonoBehaviour
    {
        public RectTransform FullScreenRoot { get; private set; }
        public RectTransform TopArea { get; private set; }
        public RectTransform CenterBoardArea { get; private set; }
        public RectTransform ActionArea { get; private set; }
        public RectTransform BottomCardTray { get; private set; }
        public RectTransform OverlayLayer { get; private set; }
        public RectTransform SafeAreaRoot { get; private set; }

        public void Build()
        {
            RectTransform parentRect = GetComponent<RectTransform>();
            if (parentRect == null)
            {
                parentRect = gameObject.AddComponent<RectTransform>();
            }

            FullScreenRoot = CreateOrGetChild(parentRect, "FullScreenRoot", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            SafeAreaRoot = CreateOrGetChild(FullScreenRoot, "SafeAreaRoot", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ApplySafeArea(SafeAreaRoot);

            float topHeight = 150f;
            float actionHeight = 170f;
            float bottomHeight = 330f;

            TopArea = CreateOrGetChild(SafeAreaRoot, "TopArea", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            ConfigureTopBarArea(TopArea, topHeight, SafeAreaRoot);

            BottomCardTray = CreateOrGetChild(SafeAreaRoot, "BottomCardTray", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, bottomHeight));

            ActionArea = CreateOrGetChild(SafeAreaRoot, "ActionArea", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, bottomHeight), new Vector2(0f, bottomHeight + actionHeight));

            CenterBoardArea = CreateOrGetChild(SafeAreaRoot, "CenterBoardArea", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, bottomHeight + actionHeight), new Vector2(0f, -topHeight));

            OverlayLayer = CreateOrGetChild(SafeAreaRoot, "OverlayLayer", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            OverlayLayer.SetAsLastSibling();
        }

        public static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max) { if (rect == null) return; rect.anchorMin = min; rect.anchorMax = max; }
        public static void SetPivot(RectTransform rect, Vector2 pivot) { if (rect == null) return; rect.pivot = pivot; }
        public static void SetSize(RectTransform rect, float width, float height) { if (rect == null) return; rect.sizeDelta = new Vector2(width, height); }
        public static void SetAnchoredPosition(RectTransform rect, float x, float y) { if (rect == null) return; rect.anchoredPosition = new Vector2(x, y); }


        private static void ConfigureTopBarArea(RectTransform topArea, float height, RectTransform safeAreaRoot)
        {
            if (topArea == null) return;

            Vector2 oldAnchorMin = topArea.anchorMin;
            Vector2 oldAnchorMax = topArea.anchorMax;
            Vector2 oldPivot = topArea.pivot;
            Vector2 oldSizeDelta = topArea.sizeDelta;
            Vector2 oldAnchoredPosition = topArea.anchoredPosition;
            string parentName = topArea.parent != null ? topArea.parent.name : "<none>";
            Vector2 parentSize = topArea.parent is RectTransform parentRect ? parentRect.rect.size : Vector2.zero;

            topArea.anchorMin = new Vector2(0f, 1f);
            topArea.anchorMax = new Vector2(1f, 1f);
            topArea.pivot = new Vector2(0.5f, 1f);
            topArea.sizeDelta = new Vector2(0f, height);
            topArea.anchoredPosition = Vector2.zero;
            topArea.offsetMin = new Vector2(0f, -height);
            topArea.offsetMax = Vector2.zero;

            float screenHeight = Mathf.Max(1f, Screen.height);
            Rect safe = Screen.safeArea;
            float topInset = screenHeight - (safe.y + safe.height);
            float convertedInset = 0f;
            if (safeAreaRoot == null)
            {
                convertedInset = topInset;
                topArea.anchoredPosition = new Vector2(0f, -convertedInset);
            }

            Debug.Log($"[LAYOUT] TopBar old anchorMin={oldAnchorMin} anchorMax={oldAnchorMax} pivot={oldPivot} sizeDelta={oldSizeDelta} anchoredPosition={oldAnchoredPosition} parent={parentName} parentRect={parentSize}");
            Debug.Log($"[LAYOUT] TopBar anchored to safe top. topInset={topInset:F1} convertedInset={convertedInset:F1} finalY={topArea.anchoredPosition.y:F1} anchorMin={topArea.anchorMin} anchorMax={topArea.anchorMax} pivot={topArea.pivot} sizeDelta={topArea.sizeDelta} parent={parentName} parentRect={parentSize}");
        }

        private static void ApplySafeArea(RectTransform root)
        {
            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            root.anchorMin = anchorMin;
            root.anchorMax = anchorMax;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
        }

        private static RectTransform CreateOrGetChild(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            Transform existing = parent.Find(name);
            RectTransform rect;
            if (existing != null)
            {
                rect = existing.GetComponent<RectTransform>();
            }
            else
            {
                GameObject child = new GameObject(name, typeof(RectTransform), typeof(Image));
                rect = child.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image image = rect.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0f, 0f, 0f, 0.02f);
                image.raycastTarget = false;
            }

            return rect;
        }
    }
}
