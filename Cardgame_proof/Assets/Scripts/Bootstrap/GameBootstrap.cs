using UnityEngine;
using CardgameProof.App;

namespace CardgameProof.Bootstrap
{
    /// <summary>
    /// Scene-facing bootstrap kept on Main.unity for compatibility.
    /// It now delegates project startup to the root ProjectBootstrap.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[Bootstrap] GameBootstrap delegating to ProjectBootstrap");
            ProjectBootstrap projectBootstrap = GetComponent<ProjectBootstrap>();
            if (projectBootstrap == null)
            {
                projectBootstrap = gameObject.AddComponent<ProjectBootstrap>();
            }

            projectBootstrap.Initialize();
        }
    }
}
