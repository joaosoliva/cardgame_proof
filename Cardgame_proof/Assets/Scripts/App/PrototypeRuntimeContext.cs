using System;
using UnityEngine;
using UnityEngine.UI;
using CardgameProof.Bootstrap;

namespace CardgameProof.App
{
    public sealed class PrototypeRuntimeContext
    {
        public PrototypeRuntimeContext(Canvas rootCanvas, SceneRootBuilder sceneRoot, Action returnToSelector)
        {
            RootCanvas = rootCanvas;
            SceneRoot = sceneRoot;
            ReturnToSelector = returnToSelector;
        }

        public Canvas RootCanvas { get; }
        public SceneRootBuilder SceneRoot { get; }
        public Action ReturnToSelector { get; }
    }
}
