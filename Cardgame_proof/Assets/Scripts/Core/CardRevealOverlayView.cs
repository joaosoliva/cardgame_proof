using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardgameProof.Core
{
    public enum CardRevealType { NoRecord, Archive, CharacterFound, CharacterIdentified }

    public sealed class CardRevealPayload
    {
        public string Title;
        public string Subtitle;
        public string Body;
        public CardRevealType RevealType;
        public bool RequireTapToContinue;
        public float AutoContinueDelay = 1f;
        public bool Celebratory;
    }

    public sealed class CardRevealOverlayView : MonoBehaviour
    {
        private GameObject root;
        private CanvasGroup rootCg;
        private RectTransform cardRt;
        private Image cardBg;
        private TextMeshProUGUI title;
        private TextMeshProUGUI subtitle;
        private TextMeshProUGUI body;
        private TextMeshProUGUI hint;
        private Button continueBtn;
        private Action onComplete;
        private bool canContinue;

        public bool IsVisible => root != null && root.activeSelf;

        public void Initialize(RectTransform parent)
        {
            if (root != null || parent == null) return;
            root = new GameObject("CardRevealOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rt = root.GetComponent<RectTransform>();
            rt.SetParent(parent, false); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            root.GetComponent<Image>().color = new Color(0,0,0,0.72f);
            rootCg = root.GetComponent<CanvasGroup>();

            var card = new GameObject("FocusCard", typeof(RectTransform), typeof(Image), typeof(Outline));
            cardRt = card.GetComponent<RectTransform>();
            cardRt.SetParent(rt, false); cardRt.anchorMin=cardRt.anchorMax=new Vector2(0.5f,0.5f); cardRt.sizeDelta = new Vector2(700, 980);
            cardBg = card.GetComponent<Image>(); cardBg.color = new Color(0.95f,0.92f,0.83f,1f);
            var ol = card.GetComponent<Outline>(); ol.effectColor = new Color(0.2f,0.18f,0.12f,0.8f); ol.effectDistance = new Vector2(3,-3);

            title = MakeText(cardRt, "Title", 48, TextAlignmentOptions.Center, new Vector2(0.08f,0.84f), new Vector2(0.92f,0.96f));
            subtitle = MakeText(cardRt, "Subtitle", 28, TextAlignmentOptions.Center, new Vector2(0.08f,0.75f), new Vector2(0.92f,0.84f));
            body = MakeText(cardRt, "Body", 28, TextAlignmentOptions.TopLeft, new Vector2(0.1f,0.24f), new Vector2(0.9f,0.72f));
            hint = MakeText(cardRt, "Hint", 22, TextAlignmentOptions.Center, new Vector2(0.1f,0.14f), new Vector2(0.9f,0.2f));

            continueBtn = CreateButton(cardRt, "Continuar", new Vector2(0.25f,0.03f), new Vector2(0.75f,0.12f));
            continueBtn.onClick.AddListener(Continue);
            root.SetActive(false);
        }

        public void PlayReveal(CardRevealPayload payload, Action complete)
        {
            if (root == null || payload == null) return;
            onComplete = complete;
            ApplyPayload(payload);
            root.SetActive(true);
            canContinue = false;
            StartCoroutine(Play(payload));
        }

        private IEnumerator Play(CardRevealPayload payload)
        {
            Debug.Log($"[REVEAL] Starting reveal type: {payload.RevealType}, title: {payload.Title}");
            Debug.Log("[REVEAL] Blocking input during reveal: true");
            rootCg.alpha = 0f; cardRt.localScale = Vector3.one*0.6f;
            float t=0f;
            while(t<0.15f){t+=Time.deltaTime; rootCg.alpha=Mathf.Lerp(0,1,t/0.15f); yield return null;}
            t=0f; while(t<0.18f){t+=Time.deltaTime; cardRt.localScale=Vector3.one*Mathf.Lerp(0.6f,1.08f,t/0.18f); yield return null;}
            t=0f; while(t<0.10f){t+=Time.deltaTime; cardRt.localScale=Vector3.one*Mathf.Lerp(1.08f,1f,t/0.10f); yield return null;}
            if(payload.Celebratory){t=0f; while(t<0.12f){t+=Time.deltaTime; cardRt.localScale=Vector3.one*Mathf.Lerp(1f,1.03f,t/0.12f); yield return null;} cardRt.localScale=Vector3.one;}
            canContinue = true;
            if (!payload.RequireTapToContinue)
            {
                yield return new WaitForSeconds(payload.AutoContinueDelay);
                Continue();
            }
            Debug.Log($"[REVEAL] Completed reveal type: {payload.RevealType}");
        }

        private void Continue()
        {
            if (!canContinue) return;
            canContinue=false;
            root.SetActive(false);
            Debug.Log("[REVEAL] Blocking input during reveal: false");
            onComplete?.Invoke();
        }

        private void ApplyPayload(CardRevealPayload p)
        {
            title.text = p.Title;
            subtitle.text = string.IsNullOrEmpty(p.Subtitle) ? string.Empty : p.Subtitle;
            body.text = p.Body;
            hint.text = p.RequireTapToContinue ? "Toque para continuar" : string.Empty;
            continueBtn.gameObject.SetActive(p.RequireTapToContinue);
            cardBg.color = p.RevealType switch
            {
                CardRevealType.NoRecord => new Color(0.83f,0.82f,0.8f,1f),
                CardRevealType.Archive => new Color(0.79f,0.86f,0.95f,1f),
                CardRevealType.CharacterFound => new Color(0.77f,0.79f,0.86f,1f),
                _ => new Color(0.95f,0.88f,0.66f,1f)
            };
        }

        private static TextMeshProUGUI MakeText(RectTransform parent, string n, int fs, TextAlignmentOptions a, Vector2 min, Vector2 max)
        {
            var go=new GameObject(n, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt=go.GetComponent<RectTransform>(); rt.SetParent(parent,false); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            var t=go.GetComponent<TextMeshProUGUI>(); t.fontSize=fs; t.alignment=a; t.color=new Color(0.15f,0.12f,0.1f,1f); t.enableWordWrapping=true; t.overflowMode=TextOverflowModes.Overflow; return t;
        }
        private static Button CreateButton(RectTransform parent, string label, Vector2 min, Vector2 max)
        {
            var go=new GameObject("ContinueButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt=go.GetComponent<RectTransform>(); rt.SetParent(parent,false); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.19f,0.46f,0.88f,1f);
            var b=go.GetComponent<Button>();
            var l=new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)); var lrt=l.GetComponent<RectTransform>(); lrt.SetParent(rt,false); lrt.anchorMin=Vector2.zero; lrt.anchorMax=Vector2.one;
            var t=l.GetComponent<TextMeshProUGUI>(); t.text=label; t.fontSize=30; t.alignment=TextAlignmentOptions.Center; t.color=Color.white; t.raycastTarget=false;
            return b;
        }
    }
}
