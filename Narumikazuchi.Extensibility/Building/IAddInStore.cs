namespace Narumikazuchi.Extensibility;

/// <summary>
/// This class is the core of the AddIn functionality. It caches known AddIns and only instantiates them once they are activated.
/// </summary>
public interface IAddInStore :
    IAddInActivator,
    IAddInCache,
    IAddInRegister,
    IAddInUnregistrator
{
    /// <summary>
    /// Enumerates through the active AddIns starting from the one that was first activated. 
    /// </summary>
    /// <returns>An iterator that iterates over all active AddIns</returns>
    public IEnumerable<IAddIn> EnumerateActiveAddIns();

    /// <summary>
    /// Occurs when an AddIn activation has begun.
    /// </summary>
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInActivating;
    /// <summary>
    /// Occurs when an AddIn activation has finished.
    /// </summary>
    public event EventHandler<IAddIn>? AddInActivated;
    /// <summary>
    /// Occurs when an AddIn shutdown has begun.
    /// </summary>
    public event EventHandler<IAddIn>? AddInShuttingDown;
    /// <summary>
    /// Occurs when an AddIn shutdown has finished.
    /// </summary>
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInShutdown;
}