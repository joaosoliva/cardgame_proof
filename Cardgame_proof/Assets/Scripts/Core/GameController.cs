using UnityEngine;
using UnityEngine.UI;
using CardgameProof.Bootstrap;

namespace CardgameProof.Core
{
    public sealed class GameController : MonoBehaviour
    {
        public void InitializeMainMenu(SceneRootBuilder sceneRoot)
        {
            if (sceneRoot == null || sceneRoot.TopArea == null)
            {
                Debug.LogWarning("Main menu initialization skipped: SceneRootBuilder is not ready.");
                return;
            }

            RectTransform topArea = sceneRoot.TopArea;
            Transform existing = topArea.Find("MainMenuTitle");
            if (existing != null)
            {
                return;
            }

            GameObject titleObject = new GameObject("MainMenuTitle", typeof(RectTransform));
            RectTransform titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.SetParent(topArea, false);
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(24f, 24f);
            titleRect.offsetMax = new Vector2(-24f, -24f);

            if (!TryCreateTextMeshPro(titleObject))
            {
                Text fallbackText = titleObject.AddComponent<Text>();
                fallbackText.text = "Main Menu";
                fallbackText.alignment = TextAnchor.MiddleLeft;
                fallbackText.fontSize = 64;
                fallbackText.color = Color.white;
                fallbackText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        private static bool TryCreateTextMeshPro(GameObject go)
        {
            System.Type tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (tmpType == null)
            {
                return false;
            }

            Component tmpComponent = go.AddComponent(tmpType);
            tmpType.GetProperty("text")?.SetValue(tmpComponent, "Main Menu");
            tmpType.GetProperty("fontSize")?.SetValue(tmpComponent, 64f);
            tmpType.GetProperty("alignment")?.SetValue(tmpComponent, 513);
            tmpType.GetProperty("color")?.SetValue(tmpComponent, Color.white);
            return true;
        }
    }
}
