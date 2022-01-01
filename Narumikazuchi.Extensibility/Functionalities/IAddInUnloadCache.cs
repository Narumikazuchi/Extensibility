namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInCache"/> to unload cached data.
/// </summary>
public interface IAddInUnloadCache
{
    /// <summary>
    /// Unloads the cache with the default cache file path as key from memory.
    /// </summary>
    /// <returns><see langword="true"/> if the operation succeeded; otherwise <see langword="false"/></returns>
    public Boolean UnloadCacheFromMemory();
    /// <summary>
    /// Unloads the cache with the cache file path as key from memory.
    /// </summary>
    /// <param name="cache">The file whose full path to use as key.</param>
    /// <returns><see langword="true"/> if the operation succeeded; otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    public Boolean UnloadCacheFromMemory([DisallowNull] FileInfo cache);
    /// <summary>
    /// Unloads the cache with the specified key from memory.
    /// </summary>
    /// <param name="key">The key of the cache to unload.</param>
    /// <returns><see langword="true"/> if the operation succeeded; otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    public Boolean UnloadCacheFromMemory([DisallowNull] String key);
}
