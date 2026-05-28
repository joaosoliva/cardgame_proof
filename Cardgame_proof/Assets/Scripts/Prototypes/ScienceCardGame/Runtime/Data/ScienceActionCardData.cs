using System;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{
    public enum ScienceActionKind
    {
        Draw,
        Connect,
        Research,
        Reframe
    }

    [Serializable]
    public sealed class ScienceActionCardData : ScienceCardData
    {
        public ScienceActionCardData(string id, string displayName, string description, ScienceActionKind actionKind)
            : base(id, displayName, description, ScienceCardType.Action)
        {
            ActionKind = actionKind;
        }

        public ScienceActionKind ActionKind { get; }
    }
}
