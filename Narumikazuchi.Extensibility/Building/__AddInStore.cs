namespace Narumikazuchi.Extensibility;

internal sealed partial class __AddInStore
{ }

// Non-Public
partial class __AddInStore
{
    internal __AddInStore(__ConfigurationInfo configuration)
    {
        this._instances = new Dictionary<AddInDefinition, IAddIn>();
        this._cache = new Dictionary<String, IList<AddInDefinition>>();
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
        this._defaultCache = configuration.DefaultCachePath;
        this.LoadCacheFromDisk();

        AppDomain.CurrentDomain
                 .ProcessExit += this.OnShutdown;
    }

    private IEnumerable<AddInDefinition> LoadCacheFromStream([DisallowNull] String key,
                                                             [DisallowNull] Stream stream)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(key);
        ExceptionHelpers.ThrowIfArgumentNull(stream);

        Int64 start = stream.Position;
        IByteDeserializer<__AddInDefinitionCollection> serializer = CreateByteSerializer
                                                                   .ForDeserialization()
                                                                   .ConfigureForOwnedType<__AddInDefinitionCollection>()
                                                                   .UseDefaultStrategies()
                                                                   .UseStrategyForType<AddInDefinition[]>(Singleton<__AddInDefinitionSerializationStrategy>.Instance)
                                                                   .UseStrategyForType<Guid[]>(Singleton<__GuidSerializationStrategy>.Instance)
                                                                   .Construct();
        serializer.TryDeserialize(stream: stream,
                                  result: out __AddInDefinitionCollection? collection);
        if (collection is null)
        {
            collection = new();
        }

        List<AddInDefinition> result = new();
        foreach (AddInDefinition definition in (IEnumerable<AddInDefinition>)collection)
        {
            if (this._instances
                    .ContainsKey(definition))
            {
                continue;
            }
            this._instances
                .Add(key: definition,
                     value: new __InactiveAddIn());
            result.Add(definition);
        }
        foreach (Guid trusted in (IEnumerable<Guid>)collection)
        {
            this._userTrustedAddInList
                .Add(item: trusted);
        }

        if (this._cache
                .ContainsKey(key))
        {
            IList<AddInDefinition> cache = this._cache[key];
            for (Int32 i = 0;
                 i < result.Count; 
                 i++)
            {
                cache.Add(result[i]);
            }
        }
        else
        {
            this._cache
                .Add(key: key,
                     value: result);
        }

        stream.Position = start;
        return result;
    }

    private Boolean WriteCacheToStream([DisallowNull] String key,
                                       [DisallowNull] Stream stream)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(key);
        ExceptionHelpers.ThrowIfArgumentNull(stream);

        if (!this._cache
                 .ContainsKey(key))
        {
            return false;
        }

        IByteSerializer<__AddInDefinitionCollection> serializer = CreateByteSerializer
                                                                 .ForSerialization()
                                                                 .ConfigureForOwnedType<__AddInDefinitionCollection>()
                                                                 .UseDefaultStrategies()
                                                                 .UseStrategyForType<AddInDefinition[]>(Singleton<__AddInDefinitionSerializationStrategy>.Instance)
                                                                 .UseStrategyForType<Guid[]>(Singleton<__GuidSerializationStrategy>.Instance)
                                                                 .Construct();
        __AddInDefinitionCollection collection = new(definitions: this._cache[key],
                                                     userTrusted: this._userTrustedAddInList);
        serializer.Serialize(stream: stream,
                             graph: collection);
        return true;
    }

    private Boolean RegisterAddIn(in AddInDefinition addIn,
                                  String cacheFilePath)
    {
        // Cache not loaded
        if (!this._cache
                 .ContainsKey(cacheFilePath))
        {
            return false;
        }

        // AddIn already registered
        if (this._cache[cacheFilePath]
                .Contains(item: addIn))
        {
            return false;
        }

        // Not registered? Then do so
        if (!this._instances
                 .ContainsKey(addIn))
        {
            this._cache[cacheFilePath]
                .Add(addIn);
            this._instances[addIn] = new __InactiveAddIn();
            return true;
        }

        // No branch triggered? Somehow we end up here, should actually be quite impossible but just to be safe.
        return false;
    }

    private void OnShutdown(Object? sender, 
                            EventArgs e)
    {
        foreach (IAddIn item in this._instances
                                    .Values)
        {
            if (item is not __InactiveAddIn)
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
        AddInDefinition addIn = new(sender);
        this._instances[addIn] = new __InactiveAddIn();
        sender.ShutdownInitiated -= this.ShutdownPassThrough;
        sender.ShutdownFinished -= this.OnAddInShutdown;
        this.AddInShutdown?.Invoke(sender: this, 
                                   eventArgs: new(addIn));
    }

    private static Assembly? FindAssembly(FileInfo assemblyLocation,
                                          String assemblyName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain
                                               .GetAssemblies()
                                               .Where(a => !a.FullName!.StartsWith("System", StringComparison.InvariantCultureIgnoreCase))
                                               .Where(a => !a.FullName!.StartsWith("Microsoft", StringComparison.InvariantCultureIgnoreCase)))
        {
            if (!assembly.IsDynamic &&
                assembly.Location == assemblyLocation.FullName &&
                assembly.FullName == assemblyName)
            {
                return assembly;
            }
        }
        return null;
    }

    private readonly IDictionary<String, IList<AddInDefinition>> _cache;
    private readonly IDictionary<AddInDefinition, IAddIn> _instances;
    private readonly String _defaultCache;
    private readonly Boolean _shouldFailWhenNotSystemTrusted;
    private readonly Boolean _shouldFailWhenNotUserTrusted;
    private readonly TrustLevel _trustLevel;
    private readonly Action<AddInDefinition>? _userNotificationDelegate;
    private readonly Func<AddInDefinition, Boolean>? _userPromptDelegate;
    private readonly ISet<Guid> _trustedAddInList = new HashSet<Guid>();
    private readonly ISet<Guid> _userTrustedAddInList = new HashSet<Guid>();

#pragma warning disable IDE1006
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String COULD_NOT_FIND_TYPE = "The type of the AddIn could not be found in any of the loaded assemblies.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String TYPES_WERE_INCOMPATIBLE = "The specified AddIn type is not assignable to the actual type of the AddIn to activate.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String CACHE_HAS_NOT_BEEN_LOADED = "The specified cache file has not been loaded into the store.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String ADDIN_HAS_NOT_YET_BEEN_REGISTERED = "The AddIn with the specified Guid has not been registered in the store yet.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String ADDIN_TYPE_DOES_NOT_INHERIT_STORE_TYPEPARAM = "The specified type can not be used in this store since it does not inherit from it's type parameter.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String ADDIN_TYPE_IS_NOT_MARKED = "The specified type can not be used in this store since it is not marked with the 'AddInAttribute' attribute.";
#pragma warning restore
}

// IAddInActivator
partial class __AddInStore : IAddInActivator
{
    public Boolean IsAddInActive(in AddInDefinition addIn)
    {
        if (!this.IsAddInRegistered(addIn: addIn))
        {
            return false;
        }

        IAddIn obj = this._instances[addIn];
        return obj is not __InactiveAddIn;
    }

    public Boolean TryActivate(in AddInDefinition definition,
                               [DisallowNull] out IAddIn addIn)
    {
        if (!this._instances
                 .ContainsKey(definition))
        {
            NotAllowed exception = new(message: ADDIN_HAS_NOT_YET_BEEN_REGISTERED);
            exception.Data
                     .Add(key: "AddIn definition",
                          value: definition);
            throw exception;
        }

        Assembly? assembly = FindAssembly(assemblyLocation: definition.Assembly,
                                          assemblyName: definition.AssemblyName);
        if (assembly is null)
        {
            assembly = Assembly.LoadFrom(assemblyFile: definition.Assembly
                                                                 .FullName);
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
        addIn = this._instances[definition];
        return false;
    }
    public Boolean TryActivate(in Guid guid,
                               in AlphanumericVersion version,
                               [DisallowNull] out IAddIn addIn)
    {
        ExceptionHelpers.ThrowIfArgumentNull(version);

        foreach (AddInDefinition key in this._instances
                                            .Keys)
        {
            if (key.UniqueIdentifier == guid &&
                key.Version == version)
            {
                return this.TryActivate(definition: key,
                                        addIn: out addIn);
            }
        }

        NotAllowed exception = new(message: ADDIN_HAS_NOT_YET_BEEN_REGISTERED);
        exception.Data
                 .Add(key: "AddIn GUID",
                      value: guid);
        exception.Data
                 .Add(key: "AddIn Version",
                      value: version);
        throw exception;
    }
    public Boolean TryActivate<TAddIn>(in AddInDefinition definition,
                                       [DisallowNull] out TAddIn addIn)
        where TAddIn : IAddIn<TAddIn>
    {
        if (!this._instances
                 .ContainsKey(definition))
        {
            NotAllowed exception = new(message: ADDIN_HAS_NOT_YET_BEEN_REGISTERED);
            exception.Data
                     .Add(key: "AddIn definition",
                          value: definition);
            throw exception;
        }

        Assembly? assembly = FindAssembly(assemblyLocation: definition.Assembly,
                                          assemblyName: definition.AssemblyName);
        if (assembly is null)
        {
            assembly = Assembly.LoadFrom(assemblyFile: definition.Assembly
                                                                 .FullName);
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
        addIn = (TAddIn)this._instances[definition];
        return false;
    }
    public Boolean TryActivate<TAddIn>(in Guid guid,
                                       in AlphanumericVersion version,
                                       [DisallowNull] out TAddIn addIn)
        where TAddIn : IAddIn<TAddIn>
    {
        ExceptionHelpers.ThrowIfArgumentNull(version);

        foreach (AddInDefinition key in this._instances
                                            .Keys)
        {
            if (key.UniqueIdentifier == guid &&
                key.Version == version)
            {
                return this.TryActivate(definition: key,
                                        addIn: out addIn);
            }
        }

        NotAllowed exception = new(message: ADDIN_HAS_NOT_YET_BEEN_REGISTERED);
        exception.Data
                 .Add(key: "AddIn GUID",
                      value: guid);
        exception.Data
                 .Add(key: "AddIn Version",
                      value: version);
        throw exception;
    }
    public Boolean TryActivate<TConfigurationOptions>(in AddInDefinition definition,
                                                      [AllowNull] TConfigurationOptions? options,
                                                      [DisallowNull] out IAddIn addIn)
    {
        if (!this._instances
                 .ContainsKey(definition))
        {
            NotAllowed exception = new(message: ADDIN_HAS_NOT_YET_BEEN_REGISTERED);
            exception.Data
                     .Add(key: "AddIn definition",
                          value: definition);
            throw exception;
        }

        Assembly? assembly = FindAssembly(definition.Assembly,
                                          definition.AssemblyName);
        if (assembly is null)
        {
            assembly = Assembly.LoadFrom(assemblyFile: definition.Assembly
                                                                 .FullName);
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
        addIn = this._instances[definition];
        return false;
    }
    public Boolean TryActivate<TConfigurationOptions>(in Guid guid,
                                                      in AlphanumericVersion version,
                                                      [AllowNull] TConfigurationOptions? options,
                                                      [DisallowNull] out IAddIn addIn)
    {
        ExceptionHelpers.ThrowIfArgumentNull(version);

        foreach (AddInDefinition key in this._instances
                                            .Keys)
        {
            if (key.UniqueIdentifier == guid &&
                key.Version == version)
            {
                return this.TryActivate(definition: key,
                                        options: options,
                                        addIn: out addIn);
            }
        }

        NotAllowed exception = new(message: ADDIN_HAS_NOT_YET_BEEN_REGISTERED);
        exception.Data
                 .Add(key: "AddIn GUID",
                      value: guid);
        exception.Data
                 .Add(key: "AddIn Version",
                      value: version);
        throw exception;
    }
    public Boolean TryActivate<TAddIn, TConfigurationOptions>(in AddInDefinition definition,
                                                              [AllowNull] TConfigurationOptions? options,
                                                              [DisallowNull] out TAddIn addIn)
        where TAddIn : IAddIn<TAddIn, TConfigurationOptions>
    {
        if (!this._instances
                 .ContainsKey(definition))
        {
            NotAllowed exception = new(message: ADDIN_HAS_NOT_YET_BEEN_REGISTERED);
            exception.Data
                     .Add(key: "AddIn definition",
                          value: definition);
            throw exception;
        }

        Assembly? assembly = FindAssembly(definition.Assembly,
                                          definition.AssemblyName);
        if (assembly is null)
        {
            assembly = Assembly.LoadFrom(assemblyFile: definition.Assembly
                                                                 .FullName);
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
        addIn = (TAddIn)this._instances[definition];
        return false;
    }
    public Boolean TryActivate<TAddIn, TConfigurationOptions>(in Guid guid,
                                                              in AlphanumericVersion version,
                                                              [AllowNull] TConfigurationOptions? options,
                                                              [DisallowNull] out TAddIn addIn)
        where TAddIn : IAddIn<TAddIn, TConfigurationOptions>
    {
        ExceptionHelpers.ThrowIfArgumentNull(version);

        foreach (AddInDefinition key in this._instances
                                            .Keys)
        {
            if (key.UniqueIdentifier == guid &&
                key.Version == version)
            {
                return this.TryActivate(definition: key,
                                        options: options,
                                        addIn: out addIn);
            }
        }

        NotAllowed exception = new(message: ADDIN_HAS_NOT_YET_BEEN_REGISTERED);
        exception.Data
                 .Add(key: "AddIn GUID",
                      value: guid);
        exception.Data
                 .Add(key: "AddIn Version",
                      value: version);
        throw exception;
    }
}

// IAddInCache
partial class __AddInStore : IAddInCache
{
    public IEnumerable<AddInDefinition> EnumerateCachedAddIns() =>
        this;
}

// IAddInClearCache
partial class __AddInStore : IAddInClearCache
{
    public Boolean ClearInMemoryCache() =>
        this.ClearInMemoryCache(key: this._defaultCache);
    public Boolean ClearInMemoryCache([DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.ClearInMemoryCache(key: cache.FullName);
    }
    public Boolean ClearInMemoryCache([DisallowNull] String key)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(key);
        if (!this._cache
                 .ContainsKey(key))
        {
            return false;
        }

        this._cache[key]
            .Clear();
        return true;
    }
}

// IAddInLoadCache
partial class __AddInStore : IAddInLoadCache
{
    public IEnumerable<AddInDefinition> LoadCacheFromDisk() =>
        this.LoadCacheFromDisk(this._defaultCache);
    public IEnumerable<AddInDefinition> LoadCacheFromDisk([DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.LoadCacheFromDisk(cache.FullName);
    }
    public IEnumerable<AddInDefinition> LoadCacheFromDisk([DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(cacheFilePath);

        if (!File.Exists(cacheFilePath))
        {
            using FileStream stream = File.Create(path: cacheFilePath);
            this._cache
                .Add(key: cacheFilePath,
                     value: new List<AddInDefinition>());
            this.WriteCacheToStream(key: cacheFilePath, 
                                    stream: stream);
            return Array.Empty<AddInDefinition>();
        }
        else
        {
            using FileStream stream = File.OpenRead(cacheFilePath);
            return this.LoadCacheFromStream(key: cacheFilePath,
                                            stream: stream);
        }
    }
}

// IAddInRegister
partial class __AddInStore : IAddInRegister
{
    public Boolean CanAddInBeRegistered(in AddInDefinition addIn) =>
        this.CanAddInBeRegistered(addInIdentifier: addIn.UniqueIdentifier);
    public Boolean CanAddInBeRegistered(in Guid addInIdentifier)
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

    public Boolean IsAddInRegistered(in AddInDefinition addIn) =>
        this._instances
            .ContainsKey(addIn);
    public Boolean IsAddInRegistered(in Guid guid,
                                     in AlphanumericVersion version)
    {
        ExceptionHelpers.ThrowIfArgumentNull(version);

        foreach (AddInDefinition addIn in this._instances
                                              .Keys)
        {
            if (addIn.UniqueIdentifier == guid &&
                addIn.Version == version)
            {
                return true;
            }
        }
        return false;
    }
    public Boolean IsAddInRegistered(in AddInDefinition addIn,
                                     [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.IsAddInRegistered(addIn: addIn,
                                      key: cache.FullName);
    }
    public Boolean IsAddInRegistered(in Guid guid,
                                     in AlphanumericVersion version,
                                     [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.IsAddInRegistered(guid: guid,
                                      version: version,   
                                      key: cache.FullName);
    }
    public Boolean IsAddInRegistered(in AddInDefinition addIn,
                                     [DisallowNull] String key)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(key);
        if (!this._cache
                 .ContainsKey(key))
        {
            KeyNotFoundException exception = new(message: CACHE_HAS_NOT_BEEN_LOADED);
            exception.Data
                     .Add(key: "Fullpath",
                          value: key);
            throw exception;
        }

        for (Int32 i = 0;
             i < this._cache[key]
                     .Count;
             i++)
        {
            if (this._cache[key][i]
                    .Equals(other: addIn))
            {
                return true;
            }
        }
        return false;
    }
    public Boolean IsAddInRegistered(in Guid guid,
                                     in AlphanumericVersion version,
                                     [DisallowNull] String key)
    {
        ExceptionHelpers.ThrowIfArgumentNull(version);
        ExceptionHelpers.ThrowIfArgumentNull(key);
        if (!this._cache
                 .ContainsKey(key))
        {
            KeyNotFoundException exception = new(message: CACHE_HAS_NOT_BEEN_LOADED);
            exception.Data
                     .Add(key: "Fullpath",
                          value: key);
            throw exception;
        }

        for (Int32 i = 0;
             i < this._cache[key]
                     .Count;
             i++)
        {
            if (this._cache[key][i]
                    .UniqueIdentifier == guid &&
                this._cache[key][i]
                    .Version == version)
            {
                return true;
            }
        }
        return false;
    }
}

// IAddInRegistrator
partial class __AddInStore : IAddInRegistrator
{
    public Boolean TryRegisterAddIn(in AddInDefinition addIn) =>
        this.TryRegisterAddIn(addIn: addIn,
                              cacheFilePath: this._defaultCache);
    public Boolean TryRegisterAddIn(in AddInDefinition addIn,
                                    [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryRegisterAddIn(addIn: addIn,
                                     cacheFilePath: cache.FullName);
    }
    public Boolean TryRegisterAddIn(in AddInDefinition addIn,
                                    [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(cacheFilePath);
        if (!File.Exists(cacheFilePath))
        {
            FileNotFoundException exception = new();
            exception.Data
                     .Add(key: "Fullpath",
                          value: cacheFilePath);
            throw exception;
        }

        if (this._trustLevel is TrustLevel.NONE)
        {
            return false;
        }

        if (this._trustLevel is TrustLevel.ALL)
        {
            return this.RegisterAddIn(addIn: addIn,
                                      cacheFilePath: cacheFilePath);
        }

        Boolean trusted = false;
        if (this._trustLevel
                .HasFlag(TrustLevel.USER_CONFIRMED_ONLY) &&
            this._userTrustedAddInList
                .Contains(item: addIn.UniqueIdentifier))
        {
            trusted = true;
        }

        if (this._trustLevel
                .HasFlag(TrustLevel.TRUSTED_ONLY) &&
            this._trustedAddInList
                .Contains(item: addIn.UniqueIdentifier))
        {
            trusted = true;
        }

        if (this._trustLevel is TrustLevel.NOT_TRUSTED &&
            this._trustedAddInList
                .Contains(item: addIn.UniqueIdentifier) ||
            this._userTrustedAddInList
                .Contains(item: addIn.UniqueIdentifier))
        {
            return false;
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
                    return this.RegisterAddIn(addIn: addIn,
                                              cacheFilePath: cacheFilePath);
                }
                if (this._shouldFailWhenNotUserTrusted)
                {
                    InvalidOperationException exception = new();
                    throw exception;
                }

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
                    InvalidOperationException exception = new();
                    throw exception;
                }
            }
            return false;
        }

        return this.RegisterAddIn(addIn: addIn,
                                  cacheFilePath: cacheFilePath);
    }
    public Boolean TryRegisterAddIn([DisallowNull] Type addInType)
    {
        ExceptionHelpers.ThrowIfArgumentNull(addInType);
        return this.TryRegisterAddIn(addInType: addInType,
                                     cacheFilePath: this._defaultCache);
    }
    public Boolean TryRegisterAddIn([DisallowNull] Type addInType,
                                    [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(addInType);
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryRegisterAddIn(addInType: addInType,
                                     cacheFilePath: cache.FullName);
    }
    public Boolean TryRegisterAddIn([DisallowNull] Type addInType,
                                    [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(addInType);
        ExceptionHelpers.ThrowIfNullOrEmpty(cacheFilePath);
        if (!AttributeResolver.HasAttribute<AddInAttribute>(addInType))
        {
            NotAllowed exception = new(message: ADDIN_TYPE_IS_NOT_MARKED,
                                       ("Typename", addInType.FullName));
            throw exception;
        }
        Boolean implements = false;
        foreach (Type @interface in addInType.GetInterfaces())
        {
            if (@interface.GetGenericTypeDefinition() == typeof(IAddIn<>))
            {
                implements = true;
                break;
            }
            if (@interface.GetGenericTypeDefinition() == typeof(IAddIn<,>))
            {
                implements = true;
                break;
            }
        }
        if (!implements)
        {
            NotAllowed exception = new(message: ADDIN_TYPE_DOES_NOT_INHERIT_STORE_TYPEPARAM,
                                       ("Expected Types", typeof(IAddIn<>).FullName + " or " + typeof(IAddIn<,>).FullName),
                                       ("Received Type", addInType.FullName));
            throw exception;
        }

        AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(info: addInType);
        AddInDefinition definition = new(attribute.UniqueIdentifier,
                                         attribute.Name,
                                         attribute.Version,
                                         addInType);
        return this.TryRegisterAddIn(addIn: definition,
                                     cacheFilePath: cacheFilePath);
    }

    public Boolean TryRegisterAddIns([DisallowNull] FileInfo assembly)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly);
        return this.TryRegisterAddIns(assemblyFilePath: assembly.FullName,
                                      cacheFilePath: this._defaultCache);
    }
    public Boolean TryRegisterAddIns([DisallowNull] FileInfo assembly,
                                     [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryRegisterAddIns(assemblyFilePath: assembly.FullName,
                                      cacheFilePath: cache.FullName);
    }
    public Boolean TryRegisterAddIns([DisallowNull] FileInfo assembly,
                                     [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly);
        ExceptionHelpers.ThrowIfNullOrEmpty(cacheFilePath);
        return this.TryRegisterAddIns(assemblyFilePath: assembly.FullName,
                                      cacheFilePath: cacheFilePath);
    }
    public Boolean TryRegisterAddIns([DisallowNull] String assemblyFilePath)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(assemblyFilePath);
        return this.TryRegisterAddIns(assemblyFilePath: assemblyFilePath,
                                      cacheFilePath: this._defaultCache);
    }
    public Boolean TryRegisterAddIns([DisallowNull] String assemblyFilePath,
                                     [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryRegisterAddIns(assemblyFilePath: assemblyFilePath,
                                      cacheFilePath: cache.FullName);
    }
    public Boolean TryRegisterAddIns([DisallowNull] String assemblyFilePath,
                                     [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(assemblyFilePath);
        ExceptionHelpers.ThrowIfNullOrEmpty(cacheFilePath);
        if (!File.Exists(assemblyFilePath))
        {
            FileNotFoundException exception = new();
            exception.Data
                     .Add(key: "Fullpath",
                          value: assemblyFilePath);
            throw exception;
        }
        if (!File.Exists(cacheFilePath))
        {
            FileNotFoundException exception = new();
            exception.Data
                     .Add(key: "Fullpath",
                          value: cacheFilePath);
            throw exception;
        }

        Assembly assembly = Assembly.LoadFrom(assemblyFile: assemblyFilePath);
        Boolean result = false;
        IEnumerable<AddInDefinition> enumerable = new __AddInEnumerator(assembly);
        foreach (AddInDefinition item in enumerable)
        {
            result |= this.TryRegisterAddIn(addIn: item,
                                            cacheFilePath: cacheFilePath);
        }
        return result;
    }

    public Boolean TryRegisterAddIns([DisallowNull] Byte[] rawAssembly) =>
        this.TryRegisterAddIns(rawAssembly: rawAssembly,
                               cacheFilePath: this._defaultCache);
    public Boolean TryRegisterAddIns([DisallowNull] Byte[] rawAssembly, 
                                     [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryRegisterAddIns(rawAssembly: rawAssembly,
                                      cacheFilePath: cache.FullName);
    }
    public Boolean TryRegisterAddIns([DisallowNull] Byte[] rawAssembly, 
                                     [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(rawAssembly);
        ExceptionHelpers.ThrowIfArgumentNull(cacheFilePath);
        if (!File.Exists(cacheFilePath))
        {
            FileNotFoundException exception = new();
            exception.Data
                     .Add(key: "Fullpath",
                          value: cacheFilePath);
            throw exception;
        }

        Assembly assembly = Assembly.Load(rawAssembly: rawAssembly);
        Boolean result = false;
        IEnumerable<AddInDefinition> enumerable = new __AddInEnumerator(assembly);
        foreach (AddInDefinition item in enumerable)
        {
            result |= this.TryRegisterAddIn(addIn: item,
                                            cacheFilePath: cacheFilePath);
        }
        return result;
    }

    public Boolean TryRegisterAddIns([DisallowNull] Stream assemblyStream) =>
        this.TryRegisterAddIns(assemblyStream: assemblyStream,
                               cacheFilePath: this._defaultCache);
    public Boolean TryRegisterAddIns([DisallowNull] Stream assemblyStream, 
                                     [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryRegisterAddIns(assemblyStream: assemblyStream,
                                      cacheFilePath: cache.FullName);
    }
    public Boolean TryRegisterAddIns([DisallowNull] Stream assemblyStream, 
                                     [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assemblyStream);

        Int32 count = (Int32)(assemblyStream.Length - assemblyStream.Position);
        Byte[] assembly = new Byte[count];
        assemblyStream.Read(buffer: assembly,
                            offset: 0,
                            count: count);
        return this.TryRegisterAddIns(rawAssembly: assembly,
                                      cacheFilePath: cacheFilePath);
    }

}

// IAddInStore
partial class __AddInStore : IAddInStore
{
    public IEnumerable<IAddIn> EnumerateActiveAddIns()
    {
        foreach (IAddIn addIn in this._instances.Values)
        {
            if (addIn is not __InactiveAddIn)
            {
                yield return addIn;
            }
        }
        yield break;
    }

    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInActivating;
    public event EventHandler<IAddIn>? AddInActivated;
    public event EventHandler<IAddIn>? AddInShuttingDown;
    public event EventHandler<IAddInStore, AddInDefinitionEventArgs>? AddInShutdown;
}

// IAddInUnloadCache
partial class __AddInStore : IAddInUnloadCache
{
    public Boolean UnloadCacheFromMemory() =>
        this.UnloadCacheFromMemory(cacheFilePath: this._defaultCache);
    public Boolean UnloadCacheFromMemory([DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.UnloadCacheFromMemory(cacheFilePath: cache.FullName);
    }
    public Boolean UnloadCacheFromMemory([DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(cacheFilePath);

        if (!this._cache
                 .ContainsKey(cacheFilePath))
        {
            return false;
        }

        foreach (AddInDefinition item in this._cache[cacheFilePath])
        {
            if (this._instances
                    .ContainsKey(item))
            {
                this._instances[item]
                    .Shutdown();
                this._instances
                    .Remove(key: item);
            }
        }
        this._cache.Remove(key: cacheFilePath);
        return true;
    }
}

// IAddInUnregistrator
partial class __AddInStore : IAddInUnregistrator
{
    public Boolean TryUnregisterAddIn([DisallowNull] FileInfo assembly)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly);
        return this.TryUnregisterAddIn(assemblyFilePath: assembly.FullName,
                                       cacheFilePath: this._defaultCache);
    }
    public Boolean TryUnregisterAddIn([DisallowNull] FileInfo assembly,
                                      [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly);
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryUnregisterAddIn(assemblyFilePath: assembly.FullName,
                                       cacheFilePath: cache.FullName);
    }
    public Boolean TryUnregisterAddIn([DisallowNull] FileInfo assembly,
                                      [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly);
        return this.TryUnregisterAddIn(assemblyFilePath: assembly.FullName,
                                       cacheFilePath: cacheFilePath);
    }
    public Boolean TryUnregisterAddIn(in AddInDefinition addIn) =>
        this.TryUnregisterAddIn(addIn: addIn,
                        cacheFilePath: this._defaultCache);
    public Boolean TryUnregisterAddIn(in AddInDefinition addIn,
                                      [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryUnregisterAddIn(addIn: addIn,
                             cacheFilePath: cache.FullName);
    }
    public Boolean TryUnregisterAddIn(in AddInDefinition addIn,
                                      [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(cacheFilePath);
        if (!File.Exists(cacheFilePath))
        {
            FileNotFoundException exception = new();
            exception.Data
                     .Add(key: "Fullpath",
                          value: cacheFilePath);
            throw exception;
        }

        if (!this._cache
                 .ContainsKey(cacheFilePath) ||
            !this._cache[cacheFilePath]
                 .Contains(item: addIn))
        {
            return false;
        }

        Int32 index = this._cache[cacheFilePath]
                          .IndexOf(addIn);
        if (this._instances
                .ContainsKey(addIn) &&
            this._instances[addIn] is not __InactiveAddIn)
        {
            this._instances[addIn]
                .Shutdown();
        }
        this._cache[cacheFilePath]
            .RemoveAt(index);
        this._instances
            .Remove(key: addIn);
        return true;
    }
    public Boolean TryUnregisterAddIn([DisallowNull] String assemblyFilePath)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(assemblyFilePath);
        return this.TryUnregisterAddIn(assemblyFilePath: assemblyFilePath,
                                       cacheFilePath: this._defaultCache);
    }
    public Boolean TryUnregisterAddIn([DisallowNull] String assemblyFilePath,
                                      [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(assemblyFilePath);
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryUnregisterAddIn(assemblyFilePath: assemblyFilePath,
                                       cacheFilePath: cache.FullName);
    }
    public Boolean TryUnregisterAddIn([DisallowNull] String assemblyFilePath,
                                      [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assemblyFilePath);
        ExceptionHelpers.ThrowIfArgumentNull(cacheFilePath);
        if (!File.Exists(cacheFilePath))
        {
            FileNotFoundException exception = new();
            exception.Data
                     .Add(key: "Fullpath",
                          value: cacheFilePath);
            throw exception;
        }
        if (!this._cache
                 .ContainsKey(cacheFilePath))
        {
            return false;
        }

        Assembly? assembly = null;
        foreach (Assembly item in AppDomain.CurrentDomain
                                           .GetAssemblies())
        {
            if (item.IsDynamic)
            {
                continue;
            }
            if (String.IsNullOrWhiteSpace(item.Location) ||
                !File.Exists(item.Location))
            {
                continue;
            }

            if (item.Location == assemblyFilePath)
            {
                assembly = item;
                break;
            }
        }

        if (assembly is null)
        {
            // Assembly not loaded
            return false;
        }

        IEnumerable<AddInDefinition> enumerator = new __AddInEnumerator(assembly);
        foreach (AddInDefinition item in enumerator)
        {
            Int32 index = this._cache[cacheFilePath]
                              .IndexOf(item);
            if (this._instances
                    .ContainsKey(item) &&
                this._instances[item] is not __InactiveAddIn)
            {
                this._instances[item]
                    .Shutdown();
            }
            this._cache[cacheFilePath]
                .RemoveAt(index);
            this._instances
                .Remove(key: item);
        }
        return true;
    }
    public Boolean TryUnregisterAddIn([DisallowNull] Type addInType)
    {
        ExceptionHelpers.ThrowIfArgumentNull(addInType);
        return this.TryUnregisterAddIn(addInType: addInType,
                                       cacheFilePath: this._defaultCache);
    }
    public Boolean TryUnregisterAddIn([DisallowNull] Type addInType,
                                      [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(addInType);
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryUnregisterAddIn(addInType: addInType,
                                       cacheFilePath: cache.FullName);
    }
    public Boolean TryUnregisterAddIn([DisallowNull] Type addInType,
                                      [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(addInType);
        ExceptionHelpers.ThrowIfNullOrEmpty(cacheFilePath);
        Boolean implements = false;
        foreach (Type @interface in addInType.GetInterfaces())
        {
            if (@interface.GetGenericTypeDefinition() == typeof(IAddIn<>))
            {
                implements = true;
                break;
            }
            if (@interface.GetGenericTypeDefinition() == typeof(IAddIn<,>))
            {
                implements = true;
                break;
            }
        }
        if (!implements)
        {
            NotAllowed exception = new(message: ADDIN_TYPE_DOES_NOT_INHERIT_STORE_TYPEPARAM,
                                       ("Expected Type", typeof(IAddIn<>).FullName + " or " + typeof(IAddIn<,>).FullName),
                                       ("Received Type", addInType.FullName));
            throw exception;
        }
        if (!AttributeResolver.HasAttribute<AddInAttribute>(addInType))
        {
            NotAllowed exception = new(message: ADDIN_TYPE_IS_NOT_MARKED,
                                       ("Typename", addInType.FullName));
            throw exception;
        }

        AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(addInType);
        AddInDefinition definition = new(attribute.UniqueIdentifier,
                                         attribute.Name,
                                         attribute.Version,
                                         addInType);
        return this.TryUnregisterAddIn(addIn: definition,
                                       cacheFilePath: cacheFilePath);
    }

    public Boolean TryUnregisterAddIns([DisallowNull] Byte[] rawAssembly) =>
        this.TryUnregisterAddIns(rawAssembly: rawAssembly,
                                 cacheFilePath: this._defaultCache);
    public Boolean TryUnregisterAddIns([DisallowNull] Byte[] rawAssembly, 
                                       [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryUnregisterAddIns(rawAssembly: rawAssembly,
                                        cacheFilePath: cache.FullName);
    }
    public Boolean TryUnregisterAddIns([DisallowNull] Byte[] rawAssembly, 
                                       [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(rawAssembly);
        ExceptionHelpers.ThrowIfArgumentNull(cacheFilePath);
        if (!File.Exists(cacheFilePath))
        {
            FileNotFoundException exception = new();
            exception.Data
                     .Add(key: "Fullpath",
                          value: cacheFilePath);
            throw exception;
        }
        if (!this._cache
                 .ContainsKey(cacheFilePath))
        {
            return false;
        }

        Assembly? assembly = null;
        Assembly? temp = Assembly.Load(rawAssembly: rawAssembly);
        foreach (Assembly item in AppDomain.CurrentDomain
                                           .GetAssemblies())
        {
            if (item.IsDynamic)
            {
                continue;
            }

            if (item.FullName == temp.FullName)
            {
                assembly = item;
                break;
            }
        }

        if (assembly is null)
        {
            // Assembly not loaded
            return false;
        }

        IEnumerable<AddInDefinition> enumerator = new __AddInEnumerator(assembly);
        foreach (AddInDefinition item in enumerator)
        {
            Int32 index = this._cache[cacheFilePath]
                              .IndexOf(item);
            if (this._instances
                    .ContainsKey(item) &&
                this._instances[item] is not __InactiveAddIn)
            {
                this._instances[item]
                    .Shutdown();
            }
            this._cache[cacheFilePath]
                .RemoveAt(index);
            this._instances
                .Remove(key: item);
        }
        return true;
    }

    public Boolean TryUnregisterAddIns([DisallowNull] Stream assemblyStream) =>
        this.TryUnregisterAddIns(assemblyStream: assemblyStream,
                                 cacheFilePath: this._defaultCache);
    public Boolean TryUnregisterAddIns([DisallowNull] Stream assemblyStream, 
                                       [DisallowNull] FileInfo cache)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cache);
        return this.TryUnregisterAddIns(assemblyStream: assemblyStream,
                                        cacheFilePath: cache.FullName);
    }
    public Boolean TryUnregisterAddIns([DisallowNull] Stream assemblyStream, 
                                       [DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assemblyStream);
        ExceptionHelpers.ThrowIfArgumentNull(cacheFilePath);

        Int32 count = (Int32)(assemblyStream.Length - assemblyStream.Position);
        Byte[] assembly = new Byte[count];
        assemblyStream.Read(buffer: assembly,
                            offset: 0,
                            count: count);
        return this.TryUnregisterAddIns(rawAssembly: assembly,
                                        cacheFilePath: cacheFilePath);
    }
}

// IAddInWriteCache
partial class __AddInStore : IAddInWriteCache
{
    public Boolean WriteCacheToDisk() =>
        this.WriteCacheToDisk(cacheFilePath: this._defaultCache);
    public Boolean WriteCacheToDisk([DisallowNull] FileInfo cacheFile)
    {
        ExceptionHelpers.ThrowIfArgumentNull(cacheFile);
        return this.WriteCacheToDisk(cacheFilePath: cacheFile.FullName);
    }
    public Boolean WriteCacheToDisk([DisallowNull] String cacheFilePath)
    {
        ExceptionHelpers.ThrowIfNullOrEmpty(cacheFilePath);

        if (!this._cache
                 .ContainsKey(cacheFilePath))
        {
            return false;
        }

        using FileStream stream = File.Create(path: cacheFilePath);
        return this.WriteCacheToStream(key: cacheFilePath,
                                       stream: stream);
    }
}

// IEnumerable
partial class __AddInStore : IEnumerable
{
    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable<AddInDefinition>)this).GetEnumerator();
}

// IEnumerable<T>
partial class __AddInStore : IEnumerable<AddInDefinition>
{
    IEnumerator<AddInDefinition> IEnumerable<AddInDefinition>.GetEnumerator()
    {
        foreach (IList<AddInDefinition> definitions in this._cache.Values)
        {
            for (Int32 i = 0; i < definitions.Count; i++)
            {
                yield return definitions[i];
            }
        }
        yield break;
    }
}

// IReadOnlyCollection<T>
partial class __AddInStore : IReadOnlyCollection<AddInDefinition>
{
    public Int32 Count =>
        this._cache
            .SelectMany(c => c.Value)
            .Count();
}