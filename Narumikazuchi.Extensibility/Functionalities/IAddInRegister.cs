namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInStore"/> to register AddIns.
/// </summary>
public interface IAddInRegister :
    IAddInRegistrator
{
    /// <summary>
    /// Gets whether the AddIn that is described by the specified <see cref="AddInDefinition"/> can be registered in the <see cref="IAddInStore"/> with or without user prompt.
    /// </summary>
    /// <remarks>
    /// If the trust level of this <see cref="IAddInStore"/> has been configured for only system-trusted,
    /// the method will ingore the possible result of user prompts and return <see langword="false"/> for AddIns that are not trusted.
    /// </remarks>
    /// <param name="addIn">The definition which describes the AddIn to register.</param>
    /// <returns><see langword="true"/> if the AddIn can be registered with or without user prompt; otherwise, <see langword="false"/></returns>
    public Boolean CanAddInBeRegistered(in AddInDefinition addIn);
    /// <summary>
    /// Gets whether the AddIn that is described by the specified <see cref="AddInDefinition"/> can be registered in the <see cref="IAddInStore"/> with or without user prompt.
    /// </summary>
    /// <remarks>
    /// If the trust level of this <see cref="IAddInStore"/> has been configured for only system-trusted,
    /// the method will ingore the possible result of user prompts and return <see langword="false"/> for AddIns that are not trusted.
    /// </remarks>
    /// <param name="addInUniqueIdentifier">The guid of the AddIn to register.</param>
    /// <returns><see langword="true"/> if the AddIn can be registered with or without user prompt; otherwise, <see langword="false"/></returns>
    public Boolean CanAddInBeRegistered(in Guid addInUniqueIdentifier);

    /// <summary>
    /// Gets whether the AddIn described by the specified <see cref="AddInDefinition"/> is currently registered in any cache.
    /// </summary>
    /// <param name="addIn">The definition which describes the AddIn to request the registration status of.</param>
    /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
    public Boolean IsAddInRegistered(in AddInDefinition addIn);
    /// <summary>
    /// Gets whether the AddIn described by the specified <see cref="AddInDefinition"/> is currently registered in any cache.
    /// </summary>
    /// <param name="guid">The guid of the AddIn to request the registration status of.</param>
    /// <param name="version">The version of the AddIn to request the registration status of.</param>
    /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
    public Boolean IsAddInRegistered(in Guid guid,
                                     in AlphanumericVersion version);
    /// <summary>
    /// Gets whether the AddIn described by the specified <see cref="AddInDefinition"/> is currently registered in the cache with the specified file as key.
    /// </summary>
    /// <param name="addIn">The definition which describes the AddIn to search for.</param>
    /// <param name="cache">The cache file whose full path serves as the key for the cache to search through.</param>
    /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="KeyNotFoundException"/>
    public Boolean IsAddInRegistered(in AddInDefinition addIn,
                                     [DisallowNull] FileInfo cache);
    /// <summary>
    /// Gets whether the AddIn described by the specified <see cref="AddInDefinition"/> is currently registered in the cache with the specified file as key.
    /// </summary>
    /// <param name="guid">The guid of the AddIn to request the registration status of.</param>
    /// <param name="version">The version of the AddIn to request the registration status of.</param>
    /// <param name="cache">The cache file whose full path serves as the key for the cache to search through.</param>
    /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="KeyNotFoundException"/>
    public Boolean IsAddInRegistered(in Guid guid,
                                     in AlphanumericVersion version,
                                     [DisallowNull] FileInfo cache);
    /// <summary>
    /// Gets whether the AddIn described by the specified <see cref="AddInDefinition"/> is currently registered in the cache with the specified key.
    /// </summary>
    /// <remarks>
    /// The key for a cache is usually it's filename. In rare cases where the cache has been streamed from memory or
    /// a different <see cref="Stream"/> the key will be determined by the code which called the method.
    /// </remarks>
    /// <param name="addIn">The definition which describes the AddIn to search for.</param>
    /// <param name="key">The key of the cache to search through.</param>
    /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="KeyNotFoundException"/>
    public Boolean IsAddInRegistered(in AddInDefinition addIn,
                                     [DisallowNull] String key);
    /// <summary>
    /// Gets whether the AddIn described by the specified <see cref="AddInDefinition"/> is currently registered in the cache with the specified key.
    /// </summary>
    /// <remarks>
    /// The key for a cache is usually it's filename. In rare cases where the cache has been streamed from memory or
    /// a different <see cref="Stream"/> the key will be determined by the code which called the method.
    /// </remarks>
    /// <param name="guid">The guid of the AddIn to request the registration status of.</param>
    /// <param name="version">The version of the AddIn to request the registration status of.</param>
    /// <param name="key">The key of the cache to search through.</param>
    /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="KeyNotFoundException"/>
    public Boolean IsAddInRegistered(in Guid guid,
                                     in AlphanumericVersion version,
                                     [DisallowNull] String key);
}