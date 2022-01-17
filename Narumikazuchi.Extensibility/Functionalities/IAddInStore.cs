namespace Narumikazuchi.Extensibility;

/// <summary>
/// This class is the core of the AddIn functionality. It caches known AddIns and only instantiates them once they are activated.
/// </summary>
public interface IAddInStore :
    IAddInActivator,
    IAddInDiscoverer,
    IAddInTrustList,
    IReadOnlyDictionary<IAddInDefinition, IAddIn>
{
    /// <summary>
    /// Enumerates through the active AddIns. 
    /// </summary>
    /// <returns>An iterator that iterates over all active AddIns</returns>
    public IEnumerable<IAddInDefinition> EnumerateActiveAddIns();
    /// <summary>
    /// Enumerates through the all AddIns. 
    /// </summary>
    /// <returns>An iterator that iterates over all active AddIns</returns>
    public IEnumerable<IAddInDefinition> EnumerateAllAddIns();
    /// <summary>
    /// Enumerates through the inactive AddIns. 
    /// </summary>
    /// <returns>An iterator that iterates over all active AddIns</returns>
    public IEnumerable<IAddInDefinition> EnumerateInactiveAddIns();

    /// <summary>
    /// Occurs when an AddIn activation has begun.
    /// </summary>
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInActivating;
    /// <summary>
    /// Occurs when an AddIn activation has finished.
    /// </summary>
    public event EventHandler<IAddIn>? AddInActivated;
    /// <summary>
    /// Occurs when the registering of an AddIn has begun.
    /// </summary>
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInDiscovering;
    /// <summary>
    /// Occurs when the registering of an AddIn has finished.
    /// </summary>
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInDiscovered;
    /// <summary>
    /// Occurs when an AddIn shutdown has begun.
    /// </summary>
    public event EventHandler<IAddIn>? AddInShuttingDown;
    /// <summary>
    /// Occurs when an AddIn shutdown has finished.
    /// </summary>
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInShutdown;
}