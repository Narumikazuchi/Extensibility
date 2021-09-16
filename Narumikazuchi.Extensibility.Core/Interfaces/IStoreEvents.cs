namespace Narumikazuchi.Extensibility
{
    internal interface IStoreEvents : IStoreIsolation
    {
        internal void OnActivating(IAddInController controller);
        internal void OnActivated(IAddInController controller);
        internal void OnShuttingDown(IAddInController controller);
        internal void OnShutdown(IAddInController controller);
        internal void OnAddInCrashed(IAddInController controller,
                                     System.Collections.Generic.IEnumerable<System.String?> exceptions);
    }
}
