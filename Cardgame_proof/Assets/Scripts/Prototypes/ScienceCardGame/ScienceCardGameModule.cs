using UnityEngine;
using CardgameProof.App;
using CardgameProof.Prototypes.ScienceCardGame.Runtime;

namespace CardgameProof.Prototypes.ScienceCardGame
{
    public sealed class ScienceCardGameModule : IPrototypeModule
    {
        private GameObject bootstrapObject;
        private ScienceCardGameBootstrap bootstrap;

        public void StartPrototype(PrototypeRuntimeContext context)
        {
            if (context?.SceneRoot?.FullScreenRoot == null)
            {
                Debug.LogWarning("[ScienceCardGame] Cannot start prototype: runtime context is missing.");
                return;
            }

            bootstrapObject = new GameObject("ScienceCardGameBootstrap");
            bootstrap = bootstrapObject.AddComponent<ScienceCardGameBootstrap>();
            bootstrap.Initialize(context, new ScienceCardGameState());
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
