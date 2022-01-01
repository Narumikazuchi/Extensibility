namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of data that has been cached for an <see cref="IAddInStore"/>.
/// </summary>
public interface IAddInCache :
    IAddInClearCache,
    IAddInLoadCache,
    IAddInUnloadCache,
    IAddInWriteCache,
    IReadOnlyCollection<AddInDefinition>
{
    /// <summary>
    /// Enumerates through the cached <see cref="AddInDefinition"/>s starting from the one that was first loaded. 
    /// </summary>
    /// <returns>An iterator that iterates over all cached <see cref="AddInDefinition"/>s</returns>
    public IEnumerable<AddInDefinition> EnumerateCachedAddIns();
}
