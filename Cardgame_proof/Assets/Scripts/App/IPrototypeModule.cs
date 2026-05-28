namespace CardgameProof.App
{
    public interface IPrototypeModule
    {
        void StartPrototype(PrototypeRuntimeContext context);
        void StopPrototype();
    }
}
