namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of the <see cref="IAddInCache"/> to write it's cached data onto a disk or into a stream.
/// </summary>
public interface IAddInWriteCache
{
    /// <summary>
    /// Writes the cache with the default cache key the path of the default cache key.
    /// </summary>
    /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public Boolean WriteCacheToDisk();
    /// <summary>
    /// Writes the cache with the cache file as key to the path of the cache file.
    /// </summary>
    /// <param name="cacheFile">The file to write the cache to.</param>
    /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public Boolean WriteCacheToDisk([DisallowNull] FileInfo cacheFile);
    /// <summary>
    /// Writes the cache with the file path as key to the specified path.
    /// </summary>
    /// <param name="cacheFilePath">The path to write the cache to.</param>
    /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="InvalidOperationException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public Boolean WriteCacheToDisk([DisallowNull] String cacheFilePath);
}
