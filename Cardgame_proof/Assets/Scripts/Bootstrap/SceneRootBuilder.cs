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

            float topHeight = 220f;
            float actionHeight = 220f;
            float bottomHeight = 360f;

            TopArea = CreateOrGetChild(SafeAreaRoot, "TopArea", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, topHeight));

            BottomCardTray = CreateOrGetChild(SafeAreaRoot, "BottomCardTray", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, bottomHeight));

            ActionArea = CreateOrGetChild(SafeAreaRoot, "ActionArea", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, bottomHeight), new Vector2(0f, bottomHeight + actionHeight));

            CenterBoardArea = CreateOrGetChild(SafeAreaRoot, "CenterBoardArea", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, bottomHeight + actionHeight), new Vector2(0f, -topHeight));

            OverlayLayer = CreateOrGetChild(SafeAreaRoot, "OverlayLayer", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            OverlayLayer.SetAsLastSibling();
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
