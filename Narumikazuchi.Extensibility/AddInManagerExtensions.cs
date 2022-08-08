namespace Narumikazuchi.Extensibility;

public static class AddInManagerExtensions
{
    [return: NotNull]
    public static AddInDefinitionCollection DiscoverAddInsContainedInAssemblies(this AddInManager manager,
                                                                                [DisallowNull] String directoryPath) =>
        manager.DiscoverAddInsContainedInAssemblies(directoryPath: directoryPath,
                                                    searchPrivateTypes: false);
    [return: NotNull]
    public static AddInDefinitionCollection DiscoverAddInsContainedInAssemblies(this AddInManager manager,
                                                                                [DisallowNull] String directoryPath,
                                                                                Boolean searchPrivateTypes)
    {
        directoryPath.ThrowIfNullOrEmpty(asArgumentException: true);

        return manager.DiscoverAddInsContainedInAssemblies(assemblies: Directory.EnumerateFiles(path: directoryPath,
                                                                                                searchPattern: "*.dll")
                                                                                .Select(File.ReadAllBytes)
                                                                                .Select(Assembly.Load),
                                                           searchPrivateTypes: searchPrivateTypes);
    }
    [return: NotNull]
    public static AddInDefinitionCollection DiscoverAddInsContainedInAssemblies<TAssemblies>(this AddInManager manager,
                                                                                             [DisallowNull] TAssemblies assemblies)
        where TAssemblies : IEnumerable<Assembly> =>
            manager.DiscoverAddInsContainedInAssemblies(assemblies: assemblies,
                                                        searchPrivateTypes: false);
    [return: NotNull]
    public static AddInDefinitionCollection DiscoverAddInsContainedInAssemblies<TAssemblies>(this AddInManager manager,
                                                                                             [DisallowNull] TAssemblies assemblies,
                                                                                             Boolean searchPrivateTypes)
        where TAssemblies : IEnumerable<Assembly>
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        AddInDefinitionCollection result = new();
        foreach (Assembly assembly in assemblies)
        {
            result.Add(manager.DiscoverAddInsContainedInAssembly(assembly: assembly,
                                                                 searchPrivateTypes: searchPrivateTypes));
        }
        return result;
    }

    [return: NotNull]
    public static AddInDefinitionCollection DiscoverAddInsContainedInAssemblyFiles<TFiles>(this AddInManager manager,
                                                                                           [DisallowNull] TFiles assemblyFiles)
        where TFiles : IEnumerable<FileInfo> =>
            manager.DiscoverAddInsContainedInAssemblyFiles(assemblyFiles: assemblyFiles,
                                                           searchPrivateTypes: false);
    [return: NotNull]
    public static AddInDefinitionCollection DiscoverAddInsContainedInAssemblyFiles<TFiles>(this AddInManager manager,
                                                                                           [DisallowNull] TFiles assemblyFiles,
                                                                                           Boolean searchPrivateTypes)
        where TFiles : IEnumerable<FileInfo>
    {
        ArgumentNullException.ThrowIfNull(assemblyFiles);

        AddInDefinitionCollection result = new();
        foreach (FileInfo assemblyFile in assemblyFiles)
        {
            if (!assemblyFile.Exists)
            {
                continue;
            }

            Assembly assembly = Assembly.Load(File.ReadAllBytes(assemblyFile.FullName));
            result.Add(manager.DiscoverAddInsContainedInAssembly(assembly: assembly,
                                                                 searchPrivateTypes: searchPrivateTypes));
        }
        return result;
    }
}
