using UnityEngine;
using CardgameProof.App;
using CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime;

namespace CardgameProof.Prototypes.ForgottenNamesExpedition
{
    public sealed class ForgottenNamesExpeditionModule : IPrototypeModule
    {
        private GameObject bootstrapObject;
        private ForgottenNamesExpeditionBootstrap bootstrap;

        public void StartPrototype(PrototypeRuntimeContext context)
        {
            if (context?.SceneRoot?.FullScreenRoot == null)
            {
                Debug.LogWarning("[ForgottenNamesExpedition] Cannot start prototype: runtime context is missing.");
                return;
            }

            bootstrapObject = new GameObject("ForgottenNamesExpeditionBootstrap");
            bootstrap = bootstrapObject.AddComponent<ForgottenNamesExpeditionBootstrap>();
            bootstrap.Initialize(context);
        }

        public void StopPrototype()
        {
            if (bootstrap != null)
            {
                bootstrap.Cleanup();
            }

            if (bootstrapObject != null)
            {
                Object.Destroy(bootstrapObject);
            }

            bootstrap = null;
            bootstrapObject = null;
        }
    }
}
