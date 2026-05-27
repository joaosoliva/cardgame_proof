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

        public void Build()
        {
            RectTransform parentRect = GetComponent<RectTransform>();
            if (parentRect == null)
            {
                parentRect = gameObject.AddComponent<RectTransform>();
            }

            FullScreenRoot = CreateOrGetChild(parentRect, "FullScreenRoot", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            float topHeight = 220f;
            float actionHeight = 220f;
            float bottomHeight = 360f;

            TopArea = CreateOrGetChild(FullScreenRoot, "TopArea", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, topHeight));

            BottomCardTray = CreateOrGetChild(FullScreenRoot, "BottomCardTray", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, bottomHeight));

            ActionArea = CreateOrGetChild(FullScreenRoot, "ActionArea", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, bottomHeight), new Vector2(0f, bottomHeight + actionHeight));

            CenterBoardArea = CreateOrGetChild(FullScreenRoot, "CenterBoardArea", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, bottomHeight + actionHeight), new Vector2(0f, -topHeight));

            OverlayLayer = CreateOrGetChild(FullScreenRoot, "OverlayLayer", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            OverlayLayer.SetAsLastSibling();
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
