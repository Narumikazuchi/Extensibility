namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInStore"/> to discover AddIns.
/// </summary>
public interface IAddInDiscoverer
{
    /// <summary>
    /// Discovers all AddIns that are available in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly file to analyze.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="InvalidOperationException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="ReflectionTypeLoadException"/>
    /// <exception cref="TargetInvocationException"/>
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] FileInfo assembly);
    /// <summary>
    /// Discovers all AddIns that are available in the specified assembly.
    /// </summary>
    /// <param name="assemblyPath">The path to the assembly to analyze.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="InvalidOperationException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="ReflectionTypeLoadException"/>
    /// <exception cref="TargetInvocationException"/>
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] String assemblyPath);
    /// <summary>
    /// Discovers all AddIns that are available in the specified assembly.
    /// </summary>
    /// <param name="rawAssembly">The raw bytes representing the assembly to analyze.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="InvalidOperationException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="ReflectionTypeLoadException"/>
    /// <exception cref="TargetInvocationException"/>
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Byte[] rawAssembly);
    /// <summary>
    /// Discovers all AddIns that are available in the specified assembly.
    /// </summary>
    /// <param name="assemblyStream">The stream that contains the assembly to analyze.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="InvalidOperationException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="ReflectionTypeLoadException"/>
    /// <exception cref="TargetInvocationException"/>
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Stream assemblyStream);
    /// <summary>
    /// Discovers all AddIns that are available in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to analyze.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="InvalidOperationException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="ReflectionTypeLoadException"/>
    /// <exception cref="TargetInvocationException"/>
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Assembly assembly);

    /// <summary>
    /// Discovers all AddIns that are available in the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to analyze.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="InvalidOperationException"/>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="ReflectionTypeLoadException"/>
    /// <exception cref="TargetInvocationException"/>
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] IEnumerable<Assembly> assemblies);
    /// <summary>
    /// Discovers all AddIns that are available in the specified dll files.
    /// </summary>
    /// <param name="assemblyFiles">The files to search for AddIns.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified, paired with the path of their origin file</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="BadImageFormatException"/>
    /// <exception cref="FileLoadException"/>
    /// <exception cref="FileNotFoundException"/>
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] IEnumerable<FileInfo> assemblyFiles);
    /// <summary>
    /// Discovers all AddIns that are available in the dll files in the specified directory.
    /// </summary>
    /// <param name="directory">The directoy to search through.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified, paired with the path of their origin file</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="BadImageFormatException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileLoadException"/>
    /// <exception cref="FileNotFoundException"/>
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] DirectoryInfo directory);
    /// <summary>
    /// Discovers all AddIns that are available in the dll files in the specified directory path.
    /// </summary>
    /// <param name="directoryPath">The path to the directoy to search through.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified, paired with the path of their origin file</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="BadImageFormatException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileLoadException"/>
    /// <exception cref="FileNotFoundException"/>
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] String directoryPath);

    /// <summary>
    /// Gets whether the AddIn described by the specified <see cref="IAddInDefinition"/> is currently registered in any cache.
    /// </summary>
    /// <param name="addIn">The definition which describes the AddIn to request the registration status of.</param>
    /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
    public Boolean IsAddInDiscovered([DisallowNull] IAddInDefinition addIn);
}