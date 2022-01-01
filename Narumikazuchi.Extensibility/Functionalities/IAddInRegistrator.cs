namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInStore"/> to register AddIns.
/// </summary>
public interface IAddInRegistrator
{
    /// <summary>
    /// Tries to register the specified AddIn in the store.
    /// </summary>
    /// <param name="addIn">The AddIn to register.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIn(in AddInDefinition addIn);
    /// <summary>
    /// Tries to register the specified AddIn in the store.
    /// </summary>
    /// <param name="addIn">The AddIn to register.</param>
    /// <param name="cache">The cache where to register the AddIn in.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIn(in AddInDefinition addIn,
                                    [DisallowNull] FileInfo cache);
    /// <summary>
    /// Tries to register the specified AddIn in the store.
    /// </summary>
    /// <param name="addIn">The AddIn to register.</param>
    /// <param name="cacheFilePath">The path to the cache where to register the AddIn in.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIn(in AddInDefinition addIn,
                                    [DisallowNull] String cacheFilePath);
    /// <summary>
    /// Tries to register the specified AddIn in the store.
    /// </summary>
    /// <param name="addInType">The type of the AddIn to register.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIn([DisallowNull] Type addInType);
    /// <summary>
    /// Tries to register the specified AddIn in the store.
    /// </summary>
    /// <param name="addInType">The type of the AddIn to register.</param>
    /// <param name="cache">The cache where to register the AddIn in.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIn([DisallowNull] Type addInType,
                                    [DisallowNull] FileInfo cache);
    /// <summary>
    /// Tries to register the specified AddIn in the store.
    /// </summary>
    /// <param name="addInType">The type of the AddIn to register.</param>
    /// <param name="cacheFilePath">The path to the cache where to register the AddIn in.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIn([DisallowNull] Type addInType,
                                    [DisallowNull] String cacheFilePath);

    /// <summary>
    /// Tries to register the AddIns that are declared in the specified assembly in the store.
    /// </summary>
    /// <param name="assembly">The assembly from which to filter out the AddIns to register.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIns([DisallowNull] FileInfo assembly);
    /// <summary>
    /// Tries to register the AddIns that are declared in the specified assembly in the store.
    /// </summary>
    /// <param name="assembly">The assembly from which to filter out the AddIns to register.</param>
    /// <param name="cache">The cache where to register the AddIns in.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIns([DisallowNull] FileInfo assembly,
                                     [DisallowNull] FileInfo cache);
    /// <summary>
    /// Tries to register the AddIns that are declared in the specified assembly in the store.
    /// </summary>
    /// <param name="assembly">The assembly from which to filter out the AddIns to register.</param>
    /// <param name="cacheFilePath">The path to the cache where to register the AddIns in.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIns([DisallowNull] FileInfo assembly,
                                     [DisallowNull] String cacheFilePath);
    /// <summary>
    /// Tries to register the AddIns that are declared in the specified assembly in the store.
    /// </summary>
    /// <param name="assemblyFilePath">The path to the assembly from which to filter out the AddIns to register.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIns([DisallowNull] String assemblyFilePath);
    /// <summary>
    /// Tries to register the AddIns that are declared in the specified assembly in the store.
    /// </summary>
    /// <param name="assemblyFilePath">The path to the assembly from which to filter out the AddIns to register.</param>
    /// <param name="cache">The cache where to register the AddIns in.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIns([DisallowNull] String assemblyFilePath,
                                     [DisallowNull] FileInfo cache);
    /// <summary>
    /// Tries to register the AddIns that are declared in the specified assembly in the store.
    /// </summary>
    /// <param name="assemblyFilePath">The path to the assembly from which to filter out the AddIns to register.</param>
    /// <param name="cacheFilePath">The path to the cache where to register the AddIns in.</param>
    /// <returns><see langword="true"/> if the registration succeeded; otherwise, <see langword="false"/></returns>
    [return: NotNull]
    public Boolean TryRegisterAddIns([DisallowNull] String assemblyFilePath,
                                     [DisallowNull] String cacheFilePath);
}
