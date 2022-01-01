namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInCache"/> to clear data that has been cached in memory.
/// </summary>
public interface IAddInClearCache
{
    /// <summary>
    /// Clears the in memory cache with the default cache path as key.
    /// </summary>
    /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
    public Boolean ClearInMemoryCache();
    /// <summary>
    /// Clears the in memory cache with the specified cache file as key.
    /// </summary>
    /// <param name="cache">The file providing the path as key for the cache to clear.</param>
    /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    public Boolean ClearInMemoryCache([DisallowNull] FileInfo cache);
    /// <summary>
    /// Clears the in memory cache with the specified cache path as key.
    /// </summary>
    /// <remarks>
    /// The key for a cache is usually it's filename. In rare cases where the cache has been streamed from memory or
    /// a different <see cref="Stream"/> the key will be determined by the code which called the method.
    /// </remarks>
    /// <param name="key">The key for the cache to clear.</param>
    /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    public Boolean ClearInMemoryCache([DisallowNull] String key);
}
