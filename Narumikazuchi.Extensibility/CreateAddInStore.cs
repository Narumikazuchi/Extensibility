namespace Narumikazuchi.Extensibility;

/// <summary>
/// A builder class to build an <see cref="IAddInStore"/> for your application.
/// </summary>
public static class CreateAddInStore
{
    /// <summary>
    /// Uses the standard path for the default cache file path.
    /// </summary>
    public static IAddInTrustConfigurator WithDefaultCachePathFromStandardImplementation()
    {
        String path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!
                                                                 .Location)!,
                                   "addins.cache");
        return new __ConfigurationInfo(path);
    }

    /// <summary>
    /// Uses the specified path as path for the default cache file.
    /// </summary>
    /// <param name="cachePath">The path to the default cache file.</param>
    /// <exception cref="ArgumentNullException"/>
    public static IAddInTrustConfigurator WithDefaultCachePath([DisallowNull] String cachePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cachePath);
        return new __ConfigurationInfo(cachePath);
    }
    /// <summary>
    /// Uses the specified file as path for the default cache file.
    /// </summary>
    /// <param name="cache">The file to be the default cache file.</param>
    /// <exception cref="ArgumentNullException"/>
    public static IAddInTrustConfigurator WithDefaultCachePath([DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return new __ConfigurationInfo(cache.FullName);
    }
}