namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInStore"/> to unregister AddIns.
/// </summary>
public interface IAddInUnregistrator
{
    /// <summary>
    /// Tries to unregister the specified AddIn.
    /// </summary>
    /// <param name="addIn">The definition which describes the AddIn to unregister.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn(in AddInDefinition addIn);
    /// <summary>
    /// Tries to unregister the specified AddIn from the specified cache. The cache needs to be loaded for this to work.
    /// </summary>
    /// <param name="addIn">The definition which describes the AddIn to unregister.</param>
    /// <param name="cache">The cache file from which to unregfister the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn(in AddInDefinition addIn,
                                      [DisallowNull] FileInfo cache);
    /// <summary>
    /// Tries to unregister the specified AddIn from the specified cache. The cache needs to be loaded for this to work.
    /// </summary>
    /// <param name="addIn">The definition which describes the AddIn to unregister.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    /// <param name="cacheFilePath">The path to the cache file from which to unregfister the AddIn.</param>
    public Boolean TryUnregisterAddIn(in AddInDefinition addIn,
                                      [DisallowNull] String cacheFilePath);
    /// <summary>
    /// Tries to unregister the specified AddIn.
    /// </summary>
    /// <param name="addInType">The type of the AddIn to unregister.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn([DisallowNull] Type addInType);
    /// <summary>
    /// Tries to unregister the specified AddIn from the specified cache. The cache needs to be loaded for this to work.
    /// </summary>
    /// <param name="addInType">The type of the AddIn to unregister.</param>
    /// <param name="cache">The cache file from which to unregfister the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn([DisallowNull] Type addInType,
                                      [DisallowNull] FileInfo cache);
    /// <summary>
    /// Tries to unregister the specified AddIn from the specified cache. The cache needs to be loaded for this to work.
    /// </summary>
    /// <param name="addInType">The type of the AddIn to unregister.</param>
    /// <param name="cacheFilePath">The path to the cache file from which to unregfister the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn([DisallowNull] Type addInType,
                                      [DisallowNull] String cacheFilePath);

    /// <summary>
    /// Tries to unregister the AddIns that are declared in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly from which to get the AddIns to unregister.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn([DisallowNull] FileInfo assembly);
    /// <summary>
    /// Tries to unregister the specified AddIn.
    /// </summary>
    /// <param name="assembly">The assembly from which to get the AddIns to unregister.</param>
    /// <param name="cache">The cache file from which to unregfister the AddIns.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn([DisallowNull] FileInfo assembly,
                                      [DisallowNull] FileInfo cache);
    /// <summary>
    /// Tries to unregister the specified AddIn.
    /// </summary>
    /// <param name="assembly">The assembly from which to get the AddIns to unregister.</param>
    /// <param name="cacheFilePath">The path to the cache file from which to unregfister the AddIns.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn([DisallowNull] FileInfo assembly,
                                      [DisallowNull] String cacheFilePath);
    /// <summary>
    /// Tries to unregister the specified AddIn.
    /// </summary>
    /// <param name="assemblyFilePath">The path to the assembly from which to get the AddIns to unregister.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn([DisallowNull] String assemblyFilePath);
    /// <summary>
    /// Tries to unregister the specified AddIn.
    /// </summary>
    /// <param name="assemblyFilePath">The path to the assembly from which to get the AddIns to unregister.</param>
    /// <param name="cache">The cache file from which to unregfister the AddIns.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn([DisallowNull] String assemblyFilePath,
                                      [DisallowNull] FileInfo cache);
    /// <summary>
    /// Tries to unregister the specified AddIn.
    /// </summary>
    /// <param name="assemblyFilePath">The path to the assembly from which to get the AddIns to unregister.</param>
    /// <param name="cacheFilePath">The path to the cache file from which to unregfister the AddIns.</param>
    /// <returns><see langword="true"/> if the AddIn has been unregistered; otherwise, <see langword="false"/></returns>
    public Boolean TryUnregisterAddIn([DisallowNull] String assemblyFilePath,
                                      [DisallowNull] String cacheFilePath);
}