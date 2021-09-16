namespace Narumikazuchi.Extensibility
{
    internal interface IStoreIsolation
    {
        internal System.Diagnostics.Process CreateIsolatedProcess(AddInDefinition definition);

        internal IsolationSettings IsolationSettings { get; }
    }
}
