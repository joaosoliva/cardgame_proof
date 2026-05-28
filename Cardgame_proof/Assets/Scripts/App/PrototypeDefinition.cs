using System;

namespace CardgameProof.App
{
    public sealed class PrototypeDefinition
    {
        public PrototypeDefinition(PrototypeId id, string displayName, string shortDescription, Func<IPrototypeModule> createModule)
        {
            Id = id;
            DisplayName = displayName;
            ShortDescription = shortDescription;
            CreateModule = createModule;
        }

        public PrototypeId Id { get; }
        public string DisplayName { get; }
        public string ShortDescription { get; }
        public Func<IPrototypeModule> CreateModule { get; }
    }
}
