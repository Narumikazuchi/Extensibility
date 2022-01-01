namespace Narumikazuchi.Extensibility;

/// <summary>
/// A class used to discover AddIns in assemblies and directories (that is the assemblies in that directory).
/// </summary>
public static partial class AddInDiscoverer
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
    public static IEnumerable<AddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] FileInfo assembly)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly);

        Assembly ass = Assembly.LoadFrom(assemblyFile: assembly.FullName);
        return new __AddInEnumerator(ass);
    }
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
    public static IEnumerable<AddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] String assemblyPath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assemblyPath);

        Assembly assembly = Assembly.LoadFrom(assemblyFile: assemblyPath);
        return new __AddInEnumerator(assembly);
    }
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
    public static IEnumerable<AddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Assembly assembly)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly);
        return new __AddInEnumerator(assembly);
    }
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
    public static IEnumerable<AddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] IEnumerable<Assembly> assemblies)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assemblies);

        foreach (Assembly assembly in assemblies)
        {
            if (assembly is null)
            {
                continue;
            }

            IEnumerable<AddInDefinition> enumerable = new __AddInEnumerator(assembly);
            foreach (AddInDefinition item in enumerable)
            {
                yield return item;
            }
        }
        yield break;
    }
    /// <summary>
    /// Discovers all AddIns that are available in the specified dll files.
    /// </summary>
    /// <param name="assemblyFiles">The files to search for AddIns.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified paired with the path of their origin file</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="BadImageFormatException"/>
    /// <exception cref="FileLoadException"/>
    /// <exception cref="FileNotFoundException"/>
    public static IEnumerable<KeyValuePair<AddInDefinition, String>> DiscoverAddInsContainedInAssemblies([DisallowNull] IEnumerable<FileInfo> assemblyFiles)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assemblyFiles);

        foreach (FileInfo dll in assemblyFiles)
        {
            if (dll is null)
            {
                continue;
            }
            if (dll.Extension != ".dll")
            {
                continue;
            }

            Assembly assembly = Assembly.LoadFrom(assemblyFile: dll.FullName);
            IEnumerable<AddInDefinition> enumerable = new __AddInEnumerator(assembly);
            foreach (AddInDefinition item in enumerable)
            {
                yield return new(key: item,
                                 value: dll.FullName);
            }
        }
        yield break;
    }
    /// <summary>
    /// Discovers all AddIns that are available in the dll files in the specified directory.
    /// </summary>
    /// <param name="directory">The directoy to search through.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified paired with the path of their origin file</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="BadImageFormatException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileLoadException"/>
    /// <exception cref="FileNotFoundException"/>
    public static IEnumerable<KeyValuePair<AddInDefinition, String>> DiscoverAddInsContainedInAssemblies([DisallowNull] DirectoryInfo directory)
    {
        ExceptionHelpers.ThrowIfArgumentNull(directory);
        if (!directory.Exists)
        {
            DirectoryNotFoundException exception = new(message: DIRECTORY_DOES_NOT_EXIST);
            exception.Data.Add(key: "Fullpath",
                               value: directory.FullName);
            throw exception;
        }

        return DiscoverAddInsContainedInAssemblies(directory.GetFiles()
                                                            .Where(f => f.Extension == ".dll"));
    }
    /// <summary>
    /// Discovers all AddIns that are available in the dll files in the specified directory path.
    /// </summary>
    /// <param name="directoryPath">The path to the directoy to search through.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified paired with the path of their origin file</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="BadImageFormatException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    /// <exception cref="FileLoadException"/>
    /// <exception cref="FileNotFoundException"/>
    public static IEnumerable<KeyValuePair<AddInDefinition, String>> DiscoverAddInsContainedInAssemblies([DisallowNull] String directoryPath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(directoryPath);
        return DiscoverAddInsContainedInAssemblies(directory: new(path: directoryPath));
    }
}

// Non-Public
partial class AddInDiscoverer
{
#pragma warning disable IDE1006
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String DIRECTORY_DOES_NOT_EXIST = "The specified directory does not exist at this location.";
#pragma warning restore
}