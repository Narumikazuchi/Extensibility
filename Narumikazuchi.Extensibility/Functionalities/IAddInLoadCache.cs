namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInCache"/> to load cache data from disks or streams.
/// </summary>
public interface IAddInLoadCache
{
    /// <summary>
    /// Loads the file from the default cache path into memory.
    /// </summary>
    /// <returns>All known AddIns registered in the cache file</returns>
    public IEnumerable<AddInDefinition> LoadCacheFromDisk();
    /// <summary>
    /// Loads the file from the specified cache file into memory.
    /// </summary>
    /// <param name="cache">The cache file to load.</param>
    /// <returns>All known AddIns registered in the cache file</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public IEnumerable<AddInDefinition> LoadCacheFromDisk([DisallowNull] FileInfo cache);
    /// <summary>
    /// Loads the file from the specified cache path into memory.
    /// </summary>
    /// <param name="cacheFilePath">The path to the cache file to load.</param>
    /// <returns>All known AddIns registered in the cache file</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="IOException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="PathTooLongException"/>
    /// <exception cref="UnauthorizedAccessException"/>
    public IEnumerable<AddInDefinition> LoadCacheFromDisk([DisallowNull] String cacheFilePath);
}
