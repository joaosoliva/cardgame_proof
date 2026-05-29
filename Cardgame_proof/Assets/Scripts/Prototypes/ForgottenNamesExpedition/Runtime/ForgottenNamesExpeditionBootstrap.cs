using UnityEngine;
using CardgameProof.App;
using CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime.Managers;

namespace CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime
{
    public sealed class ForgottenNamesExpeditionBootstrap : MonoBehaviour
    {
        private PrototypeRuntimeContext context;
        private ForgottenNamesExpeditionUIManager uiManager;

        public void Initialize(PrototypeRuntimeContext runtimeContext)
        {
            Debug.Log("[ForgottenNamesExpedition] Bootstrap initialize begin");
            context = runtimeContext;

            if (context?.SceneRoot?.FullScreenRoot == null)
            {
                Debug.LogWarning("[ForgottenNamesExpedition] Bootstrap initialization failed: missing scene root.");
                return;
            }

            uiManager = new ForgottenNamesExpeditionUIManager();
            uiManager.Initialize(context);
            Debug.Log("[ForgottenNamesExpedition] Bootstrap initialize complete");
        }

        public void Cleanup()
        {
            Debug.Log("[ForgottenNamesExpedition] Bootstrap cleanup begin");
            uiManager?.Cleanup();
            uiManager = null;
            context = null;
            Debug.Log("[ForgottenNamesExpedition] Bootstrap cleanup complete");
        }
    }
}
