using UnityEngine;
using CardgameProof.App;
using CardgameProof.Core;

namespace CardgameProof.Prototypes.ArchiveInvestigation
{
    public sealed class ArchiveInvestigationPrototypeModule : IPrototypeModule
    {
        private GameController controller;
        private GameObject controllerObject;

        public void StartPrototype(PrototypeRuntimeContext context)
        {
            if (context == null || context.SceneRoot == null)
            {
                Debug.LogWarning("[Prototype] Cannot start Archive Investigation: runtime context is missing.");
                return;
            }

            controller = Object.FindFirstObjectByType<GameController>();
            if (controller == null)
            {
                controllerObject = new GameObject("ArchiveInvestigationController");
                controller = controllerObject.AddComponent<GameController>();
            }
            else
            {
                controllerObject = controller.gameObject;
            }

            controller.InitializeMainMenu(context.SceneRoot);
        }

        public void StopPrototype()
        {
            if (controllerObject != null)
            {
                Object.Destroy(controllerObject);
                controllerObject = null;
                controller = null;
            }
        }
    }
}
