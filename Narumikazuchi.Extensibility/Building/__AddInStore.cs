namespace Narumikazuchi.Extensibility;

internal sealed partial class __AddInStore
{ }

// Non-Public
partial class __AddInStore
{
    internal __AddInStore(__ConfigurationInfo configuration)
    {
        m_Instances = new Dictionary<IAddInDefinition, IAddIn>(comparer: __AddInEqualityComparer.Instance);
        m_TrustLevel = configuration.TrustLevel;
        m_ShouldFailWhenNotSystemTrusted = configuration.ShouldFailWhenNotSystemTrusted;
        m_ShouldFailWhenNotUserTrusted = configuration.ShouldFailWhenNotUserTrusted;
        m_UserNotificationDelegate = configuration.UserNotificationDelegate;
        m_UserPromptDelegate = configuration.UserPromptDelegate;
        foreach (Guid item in configuration.TrustedAddIns)
        {
            m_TrustedAddInList.Add(item);
        }
        foreach (Guid item in configuration.UserTrustedAddIns)
        {
            m_UserTrustedAddInList.Add(item);
        }

        AppDomain.CurrentDomain
                 .ProcessExit += this.OnShutdown;
    }

    private void OnShutdown(Object? sender, 
                            EventArgs e)
    {
        foreach (IAddIn item in m_Instances.Values)
        {
            if (item is not __InactiveAddIn &&
                !item.IsShuttingDown)
            {
                item.SilentShutdown();
            }
        }
    }

    private void ShutdownPassThrough(IAddIn sender,
                                     EventArgs? eventArgs) => 
        this.AddInShuttingDown?
            .Invoke(sender: sender,
                    eventArgs: eventArgs);

    private void OnAddInShutdown(IAddIn sender,
                                 EventArgs? e)
    {
        __AddInDefinition addIn = new(sender);
        m_Instances[addIn] = new __InactiveAddIn();
        sender.ShutdownInitiated -= this.ShutdownPassThrough;
        sender.ShutdownFinished -= this.OnAddInShutdown;
        this.AddInShutdown?.Invoke(sender: this, 
                                   eventArgs: new(addIn));
    }

    private static Assembly? FindAssembly(String assemblyName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain
                                               .GetAssemblies()
                                               .Where(a => !a.FullName!.StartsWith("System", StringComparison.InvariantCultureIgnoreCase))
                                               .Where(a => !a.FullName!.StartsWith("Microsoft", StringComparison.InvariantCultureIgnoreCase)))
        {
            if (!assembly.IsDynamic &&
                assembly.GetName()
                        .FullName == assemblyName)
            {
                return assembly;
            }
        }
        return null;
    }

    private readonly IDictionary<IAddInDefinition, IAddIn> m_Instances;
    private readonly Boolean m_ShouldFailWhenNotSystemTrusted;
    private readonly Boolean m_ShouldFailWhenNotUserTrusted;
    private readonly __TrustLevel m_TrustLevel;
    private readonly Action<IAddInDefinition>? m_UserNotificationDelegate;
    private readonly Func<IAddInDefinition, Boolean>? m_UserPromptDelegate;
    private readonly ISet<Guid> m_TrustedAddInList = new HashSet<Guid>();
    private readonly ISet<Guid> m_UserTrustedAddInList = new HashSet<Guid>();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String COULD_NOT_FIND_TYPE = "The type of the AddIn could not be found in any of the loaded assemblies.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String TYPES_WERE_INCOMPATIBLE = "The specified AddIn type is not assignable to the actual type of the AddIn to activate.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String FORCED_FAIL = "Critical failure while trying to discover untrusted addin.";
}

// IAddInActivator
partial class __AddInStore : IAddInActivator
{
    public Boolean CanAddInBeActivated(IAddInDefinition addIn)
    {
        ArgumentNullException.ThrowIfNull(addIn);
        return this.CanAddInBeActivated(addInIdentifier: addIn.UniqueIdentifier);
    }
    public Boolean CanAddInBeActivated(in Guid addInIdentifier)
    {
        if (m_TrustLevel is __TrustLevel.NONE)
        {
            return false;
        }
        if (m_TrustLevel is __TrustLevel.ALL)
        {
            return true;
        }

        Boolean result = false;
        if (m_TrustLevel.HasFlag(__TrustLevel.TRUSTED_ONLY))
        {
            result |= m_TrustedAddInList.Contains(item: addInIdentifier);
        }
        if (result)
        {
            return true;
        }

        if (m_TrustLevel.HasFlag(__TrustLevel.USER_CONFIRMED_ONLY))
        {
            result |= m_UserTrustedAddInList.Contains(item: addInIdentifier);
        }

        if (m_TrustLevel is __TrustLevel.NOT_TRUSTED)
        {
            result |= !m_TrustedAddInList.Contains(item: addInIdentifier);
            result |= !m_UserTrustedAddInList.Contains(item: addInIdentifier);
        }
        return result;
    }

    public Boolean IsAddInActive(IAddInDefinition addIn) =>
        m_Instances.ContainsKey(addIn) &&
        m_Instances[addIn] is not __InactiveAddIn;

    public Boolean TryActivate(IAddInDefinition definition,
                               [NotNullWhen(true)] out IAddIn? addIn)
    {
        addIn = default;
        if (!m_Instances.ContainsKey(definition))
        {
            return false;
        }

        Assembly? assembly = FindAssembly(assemblyName: definition.AssemblyName);
        if (assembly is null)
        {
            return false;
        }

        Type? addInType = assembly.GetType(name: definition.TypeName);
        if (addInType is null)
        {
            NullReferenceException exception = new(message: COULD_NOT_FIND_TYPE);
            exception.Data
                     .Add(key: "Typename",
                          value: definition.TypeName);
            throw exception;
        }

        if (!addInType.GetInterfaces()
                      .Any(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IAddIn<>)))
        {
            InvalidCastException exception = new(message: TYPES_WERE_INCOMPATIBLE);
            exception.Data
                     .Add(key: "Received Typename",
                          value: addInType.FullName);
            exception.Data
                     .Add(key: "Expected Typename",
                          value: typeof(IAddIn<>).FullName);
            throw exception;
        }

        if (m_Instances[definition] is __InactiveAddIn)
        {
            this.AddInActivating?.Invoke(sender: this,
                                         eventArgs: new(definition));

            MethodInfo method = addInType.GetMethod(name: "Activate",
                                                    bindingAttr: BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                                                    types: Type.EmptyTypes)!;
            addIn = (IAddIn)method.Invoke(obj: null,
                                          parameters: Array.Empty<Object>())!;
            m_Instances[definition] = addIn;
            addIn.ShutdownInitiated += this.ShutdownPassThrough;
            addIn.ShutdownFinished += this.OnAddInShutdown;
            this.AddInActivated?.Invoke(sender: addIn,
                                        eventArgs: EventArgs.Empty);
            return true;
        }
        return false;
    }
    public Boolean TryActivate<TAddIn>(IAddInDefinition definition,
                                       [NotNullWhen(true)] out TAddIn? addIn)
        where TAddIn : IAddIn<TAddIn>
    {
        addIn = default;
        if (!m_Instances.ContainsKey(definition))
        {
            return false;
        }

        Assembly? assembly = FindAssembly(assemblyName: definition.AssemblyName);
        if (assembly is null)
        {
            return false;
        }

        Type? addInType = assembly.GetType(name: definition.TypeName);
        if (addInType is null)
        {
            NullReferenceException exception = new(message: COULD_NOT_FIND_TYPE);
            exception.Data
                     .Add(key: "Typename",
                          value: definition.TypeName);
            throw exception;
        }
        if (!addInType.IsAssignableTo(typeof(TAddIn)))
        {
            InvalidCastException exception = new(message: TYPES_WERE_INCOMPATIBLE);
            exception.Data
                     .Add(key: "Received Typename",
                          value: addInType.FullName);
            exception.Data
                     .Add(key: "Expected Typename",
                          value: typeof(TAddIn).FullName);
            throw exception;
        }

        if (m_Instances[definition] is __InactiveAddIn)
        {
            this.AddInActivating?.Invoke(sender: this,
                                         eventArgs: new(definition));
            m_Instances[definition] = TAddIn.Activate();
            addIn = (TAddIn)m_Instances[definition];
            addIn.ShutdownInitiated += this.ShutdownPassThrough;
            addIn.ShutdownFinished += this.OnAddInShutdown;
            this.AddInActivated?.Invoke(sender: addIn,
                                        eventArgs: EventArgs.Empty);
            return true;
        }
        return false;
    }
    public Boolean TryActivate<TConfigurationOptions>(IAddInDefinition definition,
                                                      TConfigurationOptions? options,
                                                      [NotNullWhen(true)] out IAddIn? addIn)
    {
        addIn = default;
        if (!m_Instances.ContainsKey(definition))
        {
            return false;
        }

        Assembly? assembly = FindAssembly(assemblyName: definition.AssemblyName);
        if (assembly is null)
        {
            return false;
        }

        Type? addInType = assembly.GetType(definition.TypeName);
        if (addInType is null)
        {
            NullReferenceException exception = new(message: COULD_NOT_FIND_TYPE);
            exception.Data
                     .Add(key: "Typename",
                          value: definition.TypeName);
            throw exception;
        }
        if (!addInType.GetInterfaces()
                      .Any(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IAddIn<,>)))
        {
            InvalidCastException exception = new(message: TYPES_WERE_INCOMPATIBLE);
            exception.Data
                     .Add(key: "Received Typename",
                          value: addInType.FullName);
            exception.Data
                     .Add(key: "Expected Typename",
                          value: typeof(IAddIn<,>).FullName);
            throw exception;
        }

        if (m_Instances[definition] is __InactiveAddIn)
        {
            this.AddInActivating?.Invoke(sender: this,
                                         eventArgs: new(definition));

            MethodInfo method = addInType.GetMethod(name: "Activate",
                                                    bindingAttr: BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
            addIn = (IAddIn)method.Invoke(obj: null,
                                          parameters: new Object?[] { options })!;
            m_Instances[definition] = addIn;
            addIn.ShutdownInitiated += this.ShutdownPassThrough;
            addIn.ShutdownFinished += this.OnAddInShutdown;
            this.AddInActivated?.Invoke(sender: addIn,
                                        eventArgs: EventArgs.Empty);
            return true;
        }
        return false;
    }
    public Boolean TryActivate<TAddIn, TConfigurationOptions>(IAddInDefinition definition,
                                                              TConfigurationOptions? options,
                                                              [NotNullWhen(true)] out TAddIn? addIn)
        where TAddIn : IAddIn<TAddIn, TConfigurationOptions>
    {
        addIn = default;
        if (!m_Instances.ContainsKey(definition))
        {
            return false;
        }

        Assembly? assembly = FindAssembly(assemblyName: definition.AssemblyName);
        if (assembly is null)
        {
            return false;
        }

        Type? addInType = assembly.GetType(definition.TypeName);
        if (addInType is null)
        {
            NullReferenceException exception = new(message: COULD_NOT_FIND_TYPE);
            exception.Data
                     .Add(key: "Typename",
                          value: definition.TypeName);
            throw exception;
        }
        if (!addInType.IsAssignableTo(typeof(TAddIn)))
        {
            InvalidCastException exception = new(message: TYPES_WERE_INCOMPATIBLE);
            exception.Data
                     .Add(key: "Received Typename",
                          value: addInType.FullName);
            exception.Data
                     .Add(key: "Expected Typename",
                          value: typeof(TAddIn).FullName);
            throw exception;
        }

        if (m_Instances[definition] is __InactiveAddIn)
        {
            this.AddInActivating?.Invoke(sender: this,
                                         eventArgs: new(definition));
            m_Instances[definition] = TAddIn.Activate(options: options!);
            addIn = (TAddIn)m_Instances[definition];
            addIn.ShutdownInitiated += this.ShutdownPassThrough;
            addIn.ShutdownFinished += this.OnAddInShutdown;
            this.AddInActivated?.Invoke(sender: addIn,
                                        eventArgs: EventArgs.Empty);
            return true;
        }
        return false;
    }
}

// IAddInDiscoverer
partial class __AddInStore : IAddInDiscoverer
{
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] IEnumerable<Assembly> assemblies) =>
        this.DiscoverAddInsContainedInAssemblies(assemblies: assemblies,
                                                 searchPrivateTypes: false);
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] IEnumerable<Assembly> assemblies,
                                                                             in Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        List<IAddInDefinition> result = new();
        foreach (Assembly assembly in assemblies)
        {
            result.AddRange(this.DiscoverAddInsContainedInAssembly(assembly: assembly,
                                                                   searchPrivateTypes: searchPrivateTypes));
        }
        return result;
    }
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] IEnumerable<FileInfo> assemblyFiles) =>
        this.DiscoverAddInsContainedInAssemblies(assemblyFiles: assemblyFiles,
                                                 searchPrivateTypes: false);
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] IEnumerable<FileInfo> assemblyFiles,
                                                                             in Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(assemblyFiles);

        List<IAddInDefinition> result = new();
        foreach (FileInfo assemblyFile in assemblyFiles)
        {
            if (!assemblyFile.Exists)
            {
                continue;
            }
            result.AddRange(this.DiscoverAddInsContainedInAssembly(assemblyFile: assemblyFile,
                                                                   searchPrivateTypes: searchPrivateTypes));
        }
        return result;
    }
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] DirectoryInfo directory) =>
        this.DiscoverAddInsContainedInAssemblies(directory: directory,
                                                 searchPrivateTypes: false);
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] DirectoryInfo directory,
                                                                             in Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(directory);

        List<IAddInDefinition> result = new();
        foreach (FileInfo assemblyFile in directory.EnumerateFiles(searchPattern: "*.dll"))
        {
            if (!assemblyFile.Exists)
            {
                continue;
            }
            result.AddRange(this.DiscoverAddInsContainedInAssembly(assemblyFile: assemblyFile,
                                                                   searchPrivateTypes: searchPrivateTypes));
        }
        return result;
    }
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] String directoryPath) =>
        this.DiscoverAddInsContainedInAssemblies(directoryPath: directoryPath,
                                                 searchPrivateTypes: false);
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssemblies([DisallowNull] String directoryPath,
                                                                             in Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);

        List<IAddInDefinition> result = new();
        foreach (String assemblyFile in Directory.EnumerateFiles(path: directoryPath, 
                                                                   searchPattern: "*.dll"))
        {
            if (!File.Exists(assemblyFile))
            {
                continue;
            }
            result.AddRange(this.DiscoverAddInsContainedInAssembly(assemblyPath: assemblyFile,
                                                                   searchPrivateTypes: searchPrivateTypes));
        }
        return result;
    }

    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] FileInfo assemblyFile) =>
        this.DiscoverAddInsContainedInAssembly(assemblyFile: assemblyFile,
                                               searchPrivateTypes: false);
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] FileInfo assemblyFile,
                                                                           in Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(assemblyFile);

        return this.DiscoverAddInsContainedInAssembly(assemblyPath: assemblyFile.FullName,
                                                      searchPrivateTypes: searchPrivateTypes);
    }
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] String assemblyPath) =>
        this.DiscoverAddInsContainedInAssembly(assemblyPath: assemblyPath,
                                               searchPrivateTypes: false);
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] String assemblyPath,
                                                                           in Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(assemblyPath);

        if (!File.Exists(assemblyPath))
        {
            return Array.Empty<IAddInDefinition>();
        }
        Assembly assembly = Assembly.LoadFrom(assemblyFile: assemblyPath);
        return this.DiscoverAddInsContainedInAssembly(assembly: assembly,
                                                      searchPrivateTypes: searchPrivateTypes);
    }
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Byte[] rawAssembly) =>
        this.DiscoverAddInsContainedInAssembly(rawAssembly: rawAssembly,
                                               searchPrivateTypes: false);
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Byte[] rawAssembly,
                                                                           in Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(rawAssembly);

        Assembly assembly = Assembly.Load(rawAssembly: rawAssembly);
        return this.DiscoverAddInsContainedInAssembly(assembly: assembly,
                                                      searchPrivateTypes: searchPrivateTypes);
    }
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Stream assemblyStream) =>
        this.DiscoverAddInsContainedInAssembly(assemblyStream: assemblyStream,
                                               searchPrivateTypes: false);
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Stream assemblyStream,
                                                                           in Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(assemblyStream);

        if (!assemblyStream.CanRead)
        {
            return Array.Empty<IAddInDefinition>();
        }
        using MemoryStream stream = new();
        assemblyStream.CopyTo(stream);
        return this.DiscoverAddInsContainedInAssembly(rawAssembly: stream.ToArray(),
                                                      searchPrivateTypes: searchPrivateTypes);
    }
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Assembly assembly) =>
        this.DiscoverAddInsContainedInAssembly(assembly: assembly,
                                               searchPrivateTypes: false);
    [return: NotNull]
    public IEnumerable<IAddInDefinition> DiscoverAddInsContainedInAssembly([DisallowNull] Assembly assembly,
                                                                           in Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        if (m_TrustLevel is __TrustLevel.NONE)
        {
            return Array.Empty<IAddInDefinition>();
        }

        Collection<IAddInDefinition> result = new();
        foreach (Type type in assembly.ExportedTypes)
        {
            if (!AttributeResolver.HasAttribute<AddInAttribute>(info: type))
            {
                continue;
            }
            Boolean cancel = true;
            foreach (Type @interface in type.GetInterfaces())
            {
                if (!@interface.IsGenericType)
                {
                    continue;
                }

                if (@interface.GetGenericTypeDefinition() == typeof(IAddIn<>))
                {
                    cancel = false;
                    break;
                }
                if (@interface.GetGenericTypeDefinition() == typeof(IAddIn<,>))
                {
                    cancel = false;
                    break;
                }
            }
            if (cancel)
            {
                continue;
            }

            AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(type);
            IAddInDefinition addIn = new __AddInDefinition(guid: attribute.UniqueIdentifier,
                                                           name: attribute.Name,
                                                           version: attribute.Version,
                                                           type: type);

            this.AddInDiscovering?
                .Invoke(sender: this,
                        eventArgs: new(addIn));

            Boolean trusted = false;
            if (m_TrustLevel is __TrustLevel.ALL)
            {
                trusted = true;
            }

            if (!trusted &&
                m_TrustLevel.HasFlag(__TrustLevel.USER_CONFIRMED_ONLY) &&
                m_UserTrustedAddInList.Contains(item: addIn.UniqueIdentifier))
            {
                trusted = true;
            }

            if (!trusted &&
                m_TrustLevel.HasFlag(__TrustLevel.TRUSTED_ONLY) &&
                m_TrustedAddInList.Contains(item: addIn.UniqueIdentifier))
            {
                trusted = true;
            }

            if (!trusted)
            {
                if (m_TrustLevel.HasFlag(__TrustLevel.TRUSTED_ONLY))
                {
                    if (m_UserPromptDelegate is not null &&
                        m_UserPromptDelegate.Invoke(addIn))
                    {
                        m_UserTrustedAddInList.Add(item: addIn.UniqueIdentifier);
                        result.Add(addIn);
                    }
                    if (m_ShouldFailWhenNotUserTrusted)
                    {
                        InvalidOperationException exception = new(message: FORCED_FAIL);
                        exception.Data
                                 .Add(key: "AddIn",
                                      value: addIn);
                        throw exception;
                    }
                    continue;

                }
                else if (m_TrustLevel.HasFlag(__TrustLevel.TRUSTED_ONLY))
                {
                    if (m_UserNotificationDelegate is not null)
                    {
                        m_UserNotificationDelegate.Invoke(addIn);
                    }
                    if (m_ShouldFailWhenNotSystemTrusted)
                    {
                        InvalidOperationException exception = new(message: FORCED_FAIL);
                        exception.Data
                                 .Add(key: "AddIn",
                                      value: addIn);
                        throw exception;
                    }
                    continue;
                }
            }

            if (!m_Instances.ContainsKey(addIn))
            {
                m_Instances.Add(key: addIn,
                                value: new __InactiveAddIn());
            }

            result.Add(addIn);

            this.AddInDiscovered?
                .Invoke(sender: this,
                        eventArgs: new(addIn));
        }
        return result;
    }

    public Boolean IsAddInDiscovered([DisallowNull] IAddInDefinition addIn)
    {
        ArgumentNullException.ThrowIfNull(addIn);

        return m_Instances.ContainsKey(addIn);
    }
}

// IAddInStore
partial class __AddInStore : IAddInStore
{
    public IEnumerable<IAddInDefinition> EnumerateActiveAddIns()
    {
        foreach (IAddInDefinition addIn in m_Instances.Keys)
        {
            if (m_Instances[addIn] is not __InactiveAddIn)
            {
                yield return addIn;
            }
        }
        yield break;
    }

    public IEnumerable<IAddInDefinition> EnumerateAllAddIns()
    {
        foreach (IAddInDefinition addIn in m_Instances.Keys)
        {
            yield return addIn;
        }
        yield break;
    }

    public IEnumerable<IAddInDefinition> EnumerateInactiveAddIns()
    {
        foreach (IAddInDefinition addIn in m_Instances.Keys)
        {
            if (m_Instances[addIn] is __InactiveAddIn)
            {
                yield return addIn;
            }
        }
        yield break;
    }

    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInActivating;
    public event EventHandler<IAddIn>? AddInActivated;
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInDiscovering;
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInDiscovered;
    public event EventHandler<IAddIn>? AddInShuttingDown;
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInShutdown;
}

// IAddInTrustList
partial class __AddInStore : IAddInTrustList
{
    public void ClearUserTrustedList() =>
        m_UserTrustedAddInList.Clear();

    public void ReadUserTrustedListFrom([DisallowNull] FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.ReadUserTrustedListFrom(filePath: file.FullName);
    }
    public void ReadUserTrustedListFrom([DisallowNull] String filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        using FileStream stream = File.OpenRead(filePath);
        this.ReadUserTrustedListFrom(stream: stream);
    }
    public void ReadUserTrustedListFrom([DisallowNull] Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        IByteDeserializer<Guid[]> serializer = CreateByteSerializer
                                              .ConfigureForForeignType<Guid[]>()
                                              .ForDeserialization(__GuidSerializationStrategy.Instance)
                                              .UseDefaultStrategies()
                                              .Create();
        serializer.TryDeserialize(stream: stream,
                                  result: out Guid[]? trusted);
        if (trusted is null)
        {
            trusted = Array.Empty<Guid>();
        }

        foreach (Guid guid in trusted)
        {
            m_UserTrustedAddInList.Add(guid);
        }
    }

    public void RemoveFromUserTrustedList(in Guid guid) => 
        m_UserTrustedAddInList.Remove(guid);

    public void WriteUserTrustedListTo([DisallowNull] FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.WriteUserTrustedListTo(filePath: file.FullName);
    }
    public void WriteUserTrustedListTo([DisallowNull] String filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using FileStream stream = File.Create(filePath);
        this.WriteUserTrustedListTo(stream: stream);
    }
    public void WriteUserTrustedListTo([DisallowNull] Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        IByteSerializer<Guid[]> serializer = CreateByteSerializer
                                            .ConfigureForForeignType<Guid[]>()
                                            .ForSerialization(__GuidSerializationStrategy.Instance)
                                            .UseDefaultStrategies()
                                            .Create();
        Guid[] trusted = m_UserTrustedAddInList.ToArray();
        serializer.Serialize(stream: stream,
                             graph: trusted);
    }
}

// IEnumerable
partial class __AddInStore : IEnumerable
{
    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable<KeyValuePair<IAddInDefinition, IAddIn>>)this).GetEnumerator();
}

// IEnumerable<T>
partial class __AddInStore : IEnumerable<KeyValuePair<IAddInDefinition, IAddIn>>
{
    IEnumerator<KeyValuePair<IAddInDefinition, IAddIn>> IEnumerable<KeyValuePair<IAddInDefinition, IAddIn>>.GetEnumerator() =>
        m_Instances.GetEnumerator();
}

// IReadOnlyCollection<T>
partial class __AddInStore : IReadOnlyCollection<KeyValuePair<IAddInDefinition, IAddIn>>
{
    public Int32 Count => 
        m_Instances.Count;
}

// IReadOnlyDictionary<T, U>
partial class __AddInStore : IReadOnlyDictionary<IAddInDefinition, IAddIn>
{
    public Boolean ContainsKey(IAddInDefinition key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return m_Instances.ContainsKey(key);
    }

    public Boolean TryGetValue(IAddInDefinition key, 
                               [MaybeNullWhen(false)] out IAddIn value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return m_Instances.TryGetValue(key: key,
                                       value: out value);
    }

    public IAddIn this[IAddInDefinition key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);
            return m_Instances[key];
        }
    }

    public IEnumerable<IAddInDefinition> Keys =>
        m_Instances
            .Keys;

    public IEnumerable<IAddIn> Values =>
        m_Instances
            .Values;
}