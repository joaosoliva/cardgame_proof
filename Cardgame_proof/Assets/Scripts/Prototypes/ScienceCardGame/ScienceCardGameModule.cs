using UnityEngine;
using CardgameProof.App;
using CardgameProof.Prototypes.ScienceCardGame.Runtime;

namespace CardgameProof.Prototypes.ScienceCardGame
{
    public sealed class ScienceCardGameModule : IPrototypeModule
    {
        private readonly bool debugRevealAllHands;
        private GameObject bootstrapObject;
        private ScienceCardGameBootstrap bootstrap;

        public ScienceCardGameModule(bool debugRevealAllHands = false)
        {
            this.debugRevealAllHands = debugRevealAllHands;
        }

        public void StartPrototype(PrototypeRuntimeContext context)
        {
            if (context?.SceneRoot?.FullScreenRoot == null)
            {
                Debug.LogWarning("[ScienceCardGame] Cannot start prototype: runtime context is missing.");
                return;
            }

            bootstrapObject = new GameObject("ScienceCardGameBootstrap");
            bootstrap = bootstrapObject.AddComponent<ScienceCardGameBootstrap>();
            bootstrap.Initialize(context, new ScienceCardGameState(), debugRevealAllHands);
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
