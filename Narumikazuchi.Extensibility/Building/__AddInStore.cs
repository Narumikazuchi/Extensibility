namespace Narumikazuchi.Extensibility;

internal sealed partial class __AddInStore
{ }

// Non-Public
partial class __AddInStore
{
    internal __AddInStore(__ConfigurationInfo configuration)
    {
        this._instances = new Dictionary<IAddInDefinition, IAddIn>(comparer: Singleton<__AddInEqualityComparer>.Instance);
        this._trustLevel = configuration.TrustLevel;
        this._shouldFailWhenNotSystemTrusted = configuration.ShouldFailWhenNotSystemTrusted;
        this._shouldFailWhenNotUserTrusted = configuration.ShouldFailWhenNotUserTrusted;
        this._userNotificationDelegate = configuration.UserNotificationDelegate;
        this._userPromptDelegate = configuration.UserPromptDelegate;
        foreach (Guid item in configuration.TrustedAddIns)
        {
            this._trustedAddInList
                .Add(item);
        }
        foreach (Guid item in configuration.UserTrustedAddIns)
        {
            this._userTrustedAddInList
                .Add(item);
        }

        AppDomain.CurrentDomain
                 .ProcessExit += this.OnShutdown;
    }

    private void OnShutdown(Object? sender, 
                            EventArgs e)
    {
        foreach (IAddIn item in this._instances
                                    .Values)
        {
            if (item is not __InactiveAddIn &&
                !item.IsShuttingDown)
            {
                item.SilentShutdown();
            }
        }
    }

    private void ShutdownPassThrough(IAddIn sender,
                                     EventArgs? e)
    {
        this.AddInShuttingDown?.Invoke(sender: sender, 
                                       eventArgs: e);
    }

    private void OnAddInShutdown(IAddIn sender,
                                 EventArgs? e)
    {
        __AddInDefinition addIn = new(sender);
        this._instances[addIn] = new __InactiveAddIn();
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

    private readonly IDictionary<IAddInDefinition, IAddIn> _instances;
    private readonly Boolean _shouldFailWhenNotSystemTrusted;
    private readonly Boolean _shouldFailWhenNotUserTrusted;
    private readonly TrustLevel _trustLevel;
    private readonly Action<IAddInDefinition>? _userNotificationDelegate;
    private readonly Func<IAddInDefinition, Boolean>? _userPromptDelegate;
    private readonly ISet<Guid> _trustedAddInList = new HashSet<Guid>();
    private readonly ISet<Guid> _userTrustedAddInList = new HashSet<Guid>();

#pragma warning disable IDE1006
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String COULD_NOT_FIND_TYPE = "The type of the AddIn could not be found in any of the loaded assemblies.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String TYPES_WERE_INCOMPATIBLE = "The specified AddIn type is not assignable to the actual type of the AddIn to activate.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String FORCED_FAIL = "Critical failure while trying to discover untrusted addin.";
#pragma warning restore
}

// IAddInActivator
partial class __AddInStore : IAddInActivator
{
    public Boolean CanAddInBeActivated(IAddInDefinition addIn)
    {
        ExceptionHelpers.ThrowIfArgumentNull(addIn);
        return this.CanAddInBeActivated(addInIdentifier: addIn.UniqueIdentifier);
    }
    public Boolean CanAddInBeActivated(in Guid addInIdentifier)
    {
        if (this._trustLevel is TrustLevel.NONE)
        {
            return false;
        }
        if (this._trustLevel is TrustLevel.ALL)
        {
            return true;
        }

        Boolean result = false;
        if (this._trustLevel
                .HasFlag(TrustLevel.TRUSTED_ONLY))
        {
            result |= this._trustedAddInList
                          .Contains(item: addInIdentifier);
        }
        if (result)
        {
            return true;
        }

        if (this._trustLevel
                .HasFlag(TrustLevel.USER_CONFIRMED_ONLY))
        {
            result |= this._userTrustedAddInList
                          .Contains(item: addInIdentifier);
        }

        if (this._trustLevel is TrustLevel.NOT_TRUSTED)
        {
            result |= !this._trustedAddInList
                           .Contains(item: addInIdentifier);
            result |= !this._userTrustedAddInList
                           .Contains(item: addInIdentifier);
        }
        return result;
    }

    public Boolean IsAddInActive(IAddInDefinition addIn) =>
        this._instances
            .ContainsKey(addIn) &&
        this._instances[addIn] is not __InactiveAddIn;

    public Boolean TryActivate(IAddInDefinition definition,
                               [NotNullWhen(true)] out IAddIn? addIn)
    {
        addIn = default;
        if (!this._instances
                 .ContainsKey(definition))
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

        if (this._instances[definition] is __InactiveAddIn)
        {
            this.AddInActivating?.Invoke(sender: this,
                                         eventArgs: new(definition));

            MethodInfo method = addInType.GetMethod(name: "Activate",
                                                    bindingAttr: BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                                                    types: Type.EmptyTypes)!;
            addIn = (IAddIn)method.Invoke(obj: null,
                                          parameters: Array.Empty<Object>())!;
            this._instances[definition] = addIn;
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
        if (!this._instances
                 .ContainsKey(definition))
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

        if (this._instances[definition] is __InactiveAddIn)
        {
            this.AddInActivating?.Invoke(sender: this,
                                         eventArgs: new(definition));
            this._instances[definition] = TAddIn.Activate();
            addIn = (TAddIn)this._instances[definition];
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
        if (!this._instances
                 .ContainsKey(definition))
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

        if (this._instances[definition] is __InactiveAddIn)
        {
            this.AddInActivating?.Invoke(sender: this,
                                         eventArgs: new(definition));

            MethodInfo method = addInType.GetMethod(name: "Activate",
                                                    bindingAttr: BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
            addIn = (IAddIn)method.Invoke(obj: null,
                                          parameters: new Object?[] { options })!;
            this._instances[definition] = addIn;
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
        if (!this._instances
                 .ContainsKey(definition))
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

        if (this._instances[definition] is __InactiveAddIn)
        {
            this.AddInActivating?.Invoke(sender: this,
                                         eventArgs: new(definition));
            this._instances[definition] = TAddIn.Activate(options: options!);
            addIn = (TAddIn)this._instances[definition];
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
        ExceptionHelpers.ThrowIfArgumentNull(assemblies);

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
        ExceptionHelpers.ThrowIfArgumentNull(assemblyFiles);

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
        ExceptionHelpers.ThrowIfArgumentNull(directory);

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
        ExceptionHelpers.ThrowIfArgumentNull(directoryPath);

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
        ExceptionHelpers.ThrowIfArgumentNull(assemblyFile);

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
        ExceptionHelpers.ThrowIfArgumentNull(assemblyPath);

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
        ExceptionHelpers.ThrowIfArgumentNull(rawAssembly);

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
        ExceptionHelpers.ThrowIfArgumentNull(assemblyStream);

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
        ExceptionHelpers.ThrowIfArgumentNull(assembly);

        if (this._trustLevel is TrustLevel.NONE)
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
            if (this._trustLevel is TrustLevel.ALL)
            {
                trusted = true;
            }

            if (!trusted &&
                this._trustLevel
                    .HasFlag(TrustLevel.USER_CONFIRMED_ONLY) &&
                this._userTrustedAddInList
                    .Contains(item: addIn.UniqueIdentifier))
            {
                trusted = true;
            }

            if (!trusted &&
                this._trustLevel
                    .HasFlag(TrustLevel.TRUSTED_ONLY) &&
                this._trustedAddInList
                    .Contains(item: addIn.UniqueIdentifier))
            {
                trusted = true;
            }

            if (!trusted)
            {
                if (this._trustLevel
                        .HasFlag(TrustLevel.TRUSTED_ONLY))
                {
                    if (this._userPromptDelegate is not null &&
                        this._userPromptDelegate
                            .Invoke(addIn))
                    {
                        this._userTrustedAddInList
                            .Add(item: addIn.UniqueIdentifier);
                        result.Add(addIn);
                    }
                    if (this._shouldFailWhenNotUserTrusted)
                    {
                        InvalidOperationException exception = new(message: FORCED_FAIL);
                        exception.Data
                                 .Add(key: "AddIn",
                                      value: addIn);
                        throw exception;
                    }
                    continue;

                }
                else if (this._trustLevel
                             .HasFlag(TrustLevel.TRUSTED_ONLY))
                {
                    if (this._userNotificationDelegate is not null)
                    {
                        this._userNotificationDelegate
                            .Invoke(addIn);
                    }
                    if (this._shouldFailWhenNotSystemTrusted)
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

            if (!this._instances
                     .ContainsKey(addIn))
            {
                this._instances
                    .Add(key: addIn,
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
        ExceptionHelpers.ThrowIfArgumentNull(addIn);

        return this._instances
                   .ContainsKey(addIn);
    }
}

// IAddInStore
partial class __AddInStore : IAddInStore
{
    public IEnumerable<IAddInDefinition> EnumerateActiveAddIns()
    {
        foreach (IAddInDefinition addIn in this._instances.Keys)
        {
            if (this._instances[addIn] is not __InactiveAddIn)
            {
                yield return addIn;
            }
        }
        yield break;
    }

    public IEnumerable<IAddInDefinition> EnumerateAllAddIns()
    {
        foreach (IAddInDefinition addIn in this._instances.Keys)
        {
            yield return addIn;
        }
        yield break;
    }

    public IEnumerable<IAddInDefinition> EnumerateInactiveAddIns()
    {
        foreach (IAddInDefinition addIn in this._instances.Keys)
        {
            if (this._instances[addIn] is __InactiveAddIn)
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
        this._userTrustedAddInList
            .Clear();

    public void ReadUserTrustedListFrom([DisallowNull] FileInfo file)
    {
        ExceptionHelpers.ThrowIfArgumentNull(file);

        this.ReadUserTrustedListFrom(filePath: file.FullName);
    }
    public void ReadUserTrustedListFrom([DisallowNull] String filePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(filePath);

        using FileStream stream = File.OpenRead(filePath);
        this.ReadUserTrustedListFrom(stream: stream);
    }
    public void ReadUserTrustedListFrom([DisallowNull] Stream stream)
    {
        ExceptionHelpers.ThrowIfArgumentNull(stream);

        IByteDeserializer<Guid[]> serializer = CreateByteSerializer
                                              .ForDeserialization()
                                              .ConfigureForForeignType<Guid[]>(strategy: Singleton<__GuidSerializationStrategy>.Instance)
                                              .UseDefaultStrategies()
                                              .Construct();
        serializer.TryDeserialize(stream: stream,
                                  result: out Guid[]? trusted);
        if (trusted is null)
        {
            trusted = Array.Empty<Guid>();
        }

        foreach (Guid guid in trusted)
        {
            this._userTrustedAddInList
                .Add(guid);
        }
    }

    public void RemoveFromUserTrustedList(in Guid guid) => 
        this._userTrustedAddInList
            .Remove(guid);

    public void WriteUserTrustedListTo([DisallowNull] FileInfo file)
    {
        ExceptionHelpers.ThrowIfArgumentNull(file);

        this.WriteUserTrustedListTo(filePath: file.FullName);
    }
    public void WriteUserTrustedListTo([DisallowNull] String filePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(filePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using FileStream stream = File.Create(filePath);
        this.WriteUserTrustedListTo(stream: stream);
    }
    public void WriteUserTrustedListTo([DisallowNull] Stream stream)
    {
        ExceptionHelpers.ThrowIfArgumentNull(stream);

        IByteSerializer<Guid[]> serializer = CreateByteSerializer
                                            .ForSerialization()
                                            .ConfigureForForeignType<Guid[]>(Singleton<__GuidSerializationStrategy>.Instance)
                                            .UseDefaultStrategies()
                                            .Construct();
        Guid[] trusted = this._userTrustedAddInList
                             .ToArray();
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
        this._instances
            .GetEnumerator();
}

// IReadOnlyCollection<T>
partial class __AddInStore : IReadOnlyCollection<KeyValuePair<IAddInDefinition, IAddIn>>
{
    public Int32 Count => 
        this._instances
            .Count;
}

// IReadOnlyDictionary<T, U>
partial class __AddInStore : IReadOnlyDictionary<IAddInDefinition, IAddIn>
{
    public Boolean ContainsKey(IAddInDefinition key)
    {
        ExceptionHelpers.ThrowIfArgumentNull(key);
        return this._instances
                   .ContainsKey(key);
    }

    public Boolean TryGetValue(IAddInDefinition key, 
                               [MaybeNullWhen(false)] out IAddIn value)
    {
        ExceptionHelpers.ThrowIfArgumentNull(key);
        return this._instances
                   .TryGetValue(key: key,
                                value: out value);
    }

    public IAddIn this[IAddInDefinition key]
    {
        get
        {
            ExceptionHelpers.ThrowIfArgumentNull(key);
            return this._instances[key];
        }
    }

    public IEnumerable<IAddInDefinition> Keys =>
        this._instances
            .Keys;

    public IEnumerable<IAddIn> Values =>
        this._instances
            .Values;
}