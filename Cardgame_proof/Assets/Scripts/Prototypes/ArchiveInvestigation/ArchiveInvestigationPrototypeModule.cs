using UnityEngine;
using CardgameProof.App;
using CardgameProof.Core;

namespace CardgameProof.Prototypes.ArchiveInvestigation
{
    public sealed class ArchiveInvestigationPrototypeModule : IPrototypeModule
    {
        private GameController controller;
        private GameObject controllerObject;
        private bool ownsControllerObject;

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
                ownsControllerObject = true;
            }
            else
            {
                controllerObject = controller.gameObject;
                ownsControllerObject = false;
            }

            controllerObject.SetActive(true);
            controller.InitializeMainMenu(context.SceneRoot);
        }

        public void StopPrototype()
        {
            if (controller != null)
            {
                controller.CleanupForPrototypeSwitch();
            }

            if (controllerObject != null && ownsControllerObject)
            {
                Object.Destroy(controllerObject);
            }
            else if (controllerObject != null)
            {
                controllerObject.SetActive(false);
            }

            controllerObject = null;
            controller = null;
            ownsControllerObject = false;
        }
    }
}
