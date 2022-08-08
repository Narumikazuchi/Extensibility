namespace Narumikazuchi.Extensibility;

public sealed partial class AddInManager
{
    public static AddInManager Create(TrustLevel trustLevel) =>
        Create(serviceProvider: (IServiceProvider?)null,
               trustLevel: trustLevel,
               systemTrustedAddIns: Array.Empty<Guid>(),
               userTrustedAddIns: Array.Empty<Guid>(),
               requestTrustPrompt: null);
    public static AddInManager Create<TSystemTrusted, TUserTrusted>(TrustLevel trustLevel,
                                                                    [DisallowNull] TSystemTrusted systemTrustedAddIns,
                                                                    [DisallowNull] TUserTrusted userTrustedAddIns)
        where TSystemTrusted : IEnumerable<Guid>
        where TUserTrusted : IEnumerable<Guid> =>
            Create(serviceProvider: (IServiceProvider?)null,
                   trustLevel: trustLevel,
                   systemTrustedAddIns: systemTrustedAddIns,
                   userTrustedAddIns: userTrustedAddIns,
                   requestTrustPrompt: null);
    public static AddInManager Create<TServiceProvider, TSystemTrusted, TUserTrusted>([AllowNull] TServiceProvider? serviceProvider,
                                                                                      TrustLevel trustLevel,
                                                                                      [DisallowNull] TSystemTrusted systemTrustedAddIns,
                                                                                      [DisallowNull] TUserTrusted userTrustedAddIns)
        where TServiceProvider : IServiceProvider
        where TSystemTrusted : IEnumerable<Guid>
        where TUserTrusted : IEnumerable<Guid>
    {
        ArgumentNullException.ThrowIfNull(systemTrustedAddIns);
        ArgumentNullException.ThrowIfNull(userTrustedAddIns);

        AddInManager result = new(serviceProvider: serviceProvider,
                                  trustLevel: trustLevel,
                                  requestTrustPrompt: null);

        foreach (Guid id in systemTrustedAddIns)
        {
            result.m_SystemTrustedAddIns.Add(id);
        }

        foreach (Guid id in userTrustedAddIns)
        {
            result.m_UserTrustedAddIns.Add(id);
        }

        return result;
    }
    public static AddInManager Create<TServiceProvider, TSystemTrusted, TUserTrusted>([AllowNull] TServiceProvider? serviceProvider,
                                                                                      TrustLevel trustLevel,
                                                                                      [DisallowNull] TSystemTrusted systemTrustedAddIns,
                                                                                      [DisallowNull] TUserTrusted userTrustedAddIns,
                                                                                      [AllowNull] Func<AddInDefinition, Boolean>? requestTrustPrompt)
        where TServiceProvider : IServiceProvider
        where TSystemTrusted : IEnumerable<Guid>
        where TUserTrusted : IEnumerable<Guid>
    {
        ArgumentNullException.ThrowIfNull(systemTrustedAddIns);
        ArgumentNullException.ThrowIfNull(userTrustedAddIns);

        AddInManager result = new(serviceProvider: serviceProvider,
                                  trustLevel: trustLevel,
                                  requestTrustPrompt: requestTrustPrompt);

        foreach (Guid id in systemTrustedAddIns)
        {
            result.m_SystemTrustedAddIns.Add(id);
        }

        foreach (Guid id in userTrustedAddIns)
        {
            result.m_UserTrustedAddIns.Add(id);
        }

        return result;
    }
}

// Non-Public
partial class AddInManager
{
    private AddInManager(Option<IServiceProvider> serviceProvider,
                         TrustLevel trustLevel,
                         Option<Func<AddInDefinition, Boolean>> requestTrustPrompt)
    {
        m_ServiceProvider = serviceProvider;
        m_TrustLevel = trustLevel;
        m_RequestTrustPrompt = requestTrustPrompt;

        AppDomain.CurrentDomain.ProcessExit += this.OnShutdown;
    }

    private Option<AddIn> ActivateAddIn(AddInDefinition definition,
                                        Option<Type> type)
    {
        this.AddInActivating?.Invoke(sender: this,
                                     eventArgs: new(definition));

        Option<ConstructorInfo[]> constructors = type.Map(x => x.GetConstructors());
        if (constructors.Map(x => x.Length == 0))
        {
            this.AddInActivationFailed?.Invoke(sender: this,
                                               eventArgs: new(reason: ActivationFailReason.NoPublicConstructor));
            return null;
        }

        List<Exception> exceptions = new();
        constructors.TryGetValue(out ConstructorInfo[]? constructorsArray);
        m_ServiceProvider.TryGetValue(out IServiceProvider? serviceProvider);
        foreach (ConstructorInfo constructor in constructorsArray!)
        {
            Boolean skip = false;
            ParameterInfo[] constructorParameters = constructor.GetParameters();
            List<Object?> currentParameters = new();
            if (serviceProvider is not null &&
                constructorParameters.Length > 0)
            {
                foreach (ParameterInfo parameter in constructorParameters)
                {
                    Object? service = serviceProvider.GetService(parameter.ParameterType);
                    if (service is null)
                    {
                        skip = true;
                        break;
                    }
                    else
                    {
                        currentParameters.Add(service);
                    }
                }
            }

            if (skip)
            {
                continue;
            }

            try
            {
                AddIn addIn = (AddIn)constructor.Invoke(currentParameters.ToArray());
                m_Instances[definition] = addIn;
                addIn.Start();
                this.AddInActivated?.Invoke(sender: addIn,
                                            eventArgs: EventArgs.Empty);
                return addIn;
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        }

        this.AddInActivationFailed?.Invoke(sender: this,
                                           eventArgs: new(reason: ActivationFailReason.NotAllParametersServed,
                                                          exceptions: exceptions));
        return null;
    }

    private void AddNullInstance(AddInDefinition definition,
                                 AddInDefinitionCollection definitions)
    {
        if (!m_Instances.ContainsKey(definition))
        {
            m_Instances.Add(key: definition,
                            value: null);
        }

        definitions.Add(definition);

        this.AddInDiscovered?.Invoke(sender: this,
                                     eventArgs: new(definition));
    }

    private static Option<Assembly> FindAssembly(String assemblyName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
                                                             .Where(a => !a.FullName!.StartsWith("System", StringComparison.InvariantCultureIgnoreCase))
                                                             .Where(a => !a.FullName!.StartsWith("Microsoft", StringComparison.InvariantCultureIgnoreCase)))
        {
            if (!assembly.IsDynamic &&
                assembly.GetName().FullName == assemblyName)
            {
                return assembly;
            }
        }

        return null;
    }

    private void OnShutdown(Object? sender,
                            EventArgs args)
    {
        foreach (KeyValuePair<AddInDefinition, Option<AddIn>> kv in m_Instances)
        {
            if (kv.Value.Map(x => x.IsRunning))
            {
                this.AddInDeactivating?.Invoke(sender: kv.Value,
                                               eventArgs: new());
                try
                {
                    kv.Value.Interact(x => x.Stop());
                    this.AddInDeactivated?.Invoke(sender: this,
                                                  eventArgs: new(kv.Key));
                }
                catch (Exception exception)
                {
                    this.AddInDeactivationFailed?.Invoke(sender: this,
                                                         eventArgs: new(reason: ActivationFailReason.Exception,
                                                                        exception: exception));
                }
            }
        }
    }

    private readonly Dictionary<AddInDefinition, Option<AddIn>> m_Instances = new();
    private readonly HashSet<Guid> m_UserTrustedAddIns = new();
    private readonly HashSet<Guid> m_SystemTrustedAddIns = new();
    private readonly Option<IServiceProvider> m_ServiceProvider;
    private readonly Option<Func<AddInDefinition, Boolean>> m_RequestTrustPrompt;
    private readonly TrustLevel m_TrustLevel;
    private Option<ByteSerializerDeserializer<Guid[]>> m_GuidSerializer = null;
}

// Activate AddIns
partial class AddInManager
{
    public Boolean CanAddInBeActivated(AddInDefinition definition)
    {
        if (m_TrustLevel is TrustLevel.Nothing
                         or TrustLevel.OnlyNotTrusted)
        {
            return false;
        }
        else if (m_TrustLevel is TrustLevel.All)
        {
            return true;
        }

        if (m_TrustLevel.HasFlag(TrustLevel.OnlySystem))
        {
            if (m_SystemTrustedAddIns.Contains(definition.Id))
            {
                return true;
            }
        }

        if (m_TrustLevel.HasFlag(TrustLevel.OnlyUserConfirmed))
        {
            if (m_UserTrustedAddIns.Contains(definition.Id))
            {
                return true;
            }
        }

        return false;
    }

    public Boolean IsAddInActive(AddInDefinition definition) =>
        m_Instances[definition].Map(x => x.IsRunning);

    public Boolean TryActivate(AddInDefinition definition,
                               out Option<AddIn> addIn)
    {
        addIn = null;
        if (!m_Instances.ContainsKey(definition))
        {
            this.AddInActivationFailed?.Invoke(sender: this,
                                                eventArgs: new(ActivationFailReason.NotDiscovered));
            return false;
        }

        Option<Assembly> assembly = FindAssembly(definition.AssemblyName);
        Option<Type> addInType = assembly.Map(x => x.GetType(name: definition.TypeName)!);
        if (!addInType.HasValue)
        {
            this.AddInActivationFailed?.Invoke(sender: this,
                                                eventArgs: new(ActivationFailReason.AssemblyNotLoaded));
            return false;
        }

        if (!m_Instances[definition].HasValue)
        {
            addIn = this.ActivateAddIn(definition: definition,
                                       type: addInType);
            return addIn.HasValue;
        }
        
        if (m_Instances[definition].Map(x => !x.IsRunning))
        {
            m_Instances[definition].Interact(x => x.Start());
            addIn = m_Instances[definition];
            return true;
        }
        else
        {
            this.AddInActivationFailed?.Invoke(sender: this,
                                                eventArgs: new(ActivationFailReason.AlreadyRunning));
            return false;
        }
    }
    public Boolean TryActivate<TAddIn>(AddInDefinition definition,
                                       out Option<TAddIn> addIn)
        where TAddIn : AddIn
    {
        addIn = null;
        if (!m_Instances.ContainsKey(definition))
        {
            this.AddInActivationFailed?.Invoke(sender: this,
                                                eventArgs: new(ActivationFailReason.NotDiscovered));
            return false;
        }

        Option<Assembly> assembly = FindAssembly(definition.AssemblyName);
        Option<Type> addInType = assembly.Map(x => x.GetType(name: definition.TypeName)!);
        if (!addInType.HasValue)
        {
            this.AddInActivationFailed?.Invoke(sender: this,
                                                eventArgs: new(ActivationFailReason.AssemblyNotLoaded));
            return false;
        }
        if (addInType.Map(x => x.FullName != typeof(TAddIn).FullName) &&
            addInType.Map(x => !x.IsAssignableTo(typeof(TAddIn))))
        {
            this.AddInActivationFailed?.Invoke(sender: this,
                                                eventArgs: new(ActivationFailReason.InvalidCast));
            return false;
        }

        if (!m_Instances[definition].HasValue)
        {
            addIn = (TAddIn)this.ActivateAddIn(definition: definition,
                                               type: addInType)!;
            return addIn.HasValue;
        }

        if (m_Instances[definition].Map(x => !x.IsRunning))
        {
            m_Instances[definition].Interact(x => x.Start());
            addIn = (TAddIn)m_Instances[definition]!;
            return true;
        }
        else
        {
            this.AddInActivationFailed?.Invoke(sender: this,
                                                eventArgs: new(ActivationFailReason.AlreadyRunning));
            return false;
        }
    }

    public Boolean TryDeactivate(AddInDefinition definition)
    {
        if (m_Instances.ContainsKey(definition))
        {
            Option<AddIn> addIn = m_Instances[definition];
            if (addIn.Map(x => x.IsRunning))
            {
                addIn.TryGetValue(out AddIn? value);
                this.AddInDeactivating?.Invoke(sender: value,
                                               eventArgs: new());
                try
                {
                    addIn.Interact(x => x.Stop());
                }
                catch (Exception exception)
                {
                    this.AddInDeactivationFailed?.Invoke(sender: this,
                                                         eventArgs: new(reason: ActivationFailReason.Exception,
                                                                        exception: exception));
                }
                this.AddInDeactivated?.Invoke(sender: this,
                                              eventArgs: new(definition));
                return true;
            }
            else
            {
                this.AddInDeactivationFailed?.Invoke(sender: this,
                                                     eventArgs: new(ActivationFailReason.NotRunning));
                return false;
            }
        }
        else
        {
            this.AddInDeactivationFailed?.Invoke(sender: this,
                                                 eventArgs: new(ActivationFailReason.NotDiscovered));
            return false;
        }
    }

    public event EventHandler<AddInManager, AddInDefinitionEventArgs>? AddInActivating;
    public event EventHandler<AddInManager, AddInActivationFailedEventArgs>? AddInActivationFailed;
    public event EventHandler<AddIn>? AddInActivated;
    public event EventHandler<AddIn>? AddInDeactivating;
    public event EventHandler<AddInManager, AddInActivationFailedEventArgs>? AddInDeactivationFailed;
    public event EventHandler<AddInManager, AddInDefinitionEventArgs>? AddInDeactivated;
}

// AddIn List
partial class AddInManager
{
    public AddInDefinitionsEnumerator EnumerateRunningAddIns() =>
        new(source: m_Instances.GetEnumerator(),
            match: (definition, addIn) => addIn.Map(x => x.IsRunning));

    public AddInDefinitionsEnumerator EnumerateInactiveAddIns() =>
        new(source: m_Instances.GetEnumerator(),
            match: (definition, addIn) => addIn.Map(x => !x.IsRunning));

    public AddInDefinitionsEnumerator EnumerateAllAddIns() =>
        new(source: m_Instances.GetEnumerator(),
            match: (definition, addIn) => true);

    public Option<AddInDefinition?> this[Guid id]
    {
        get
        {
            foreach (AddInDefinition definition in m_Instances.Keys)
            {
                if (definition.Id == id)
                {
                    return definition;
                }
            }

            return null;
        }
    }
}

// Discoverer AddIns
partial class AddInManager
{
    [return: NotNull]
    public AddInDefinitionCollection DiscoverAddInsContainedInAssembly([DisallowNull] Assembly assembly,
                                                                       Boolean searchPrivateTypes)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        if (m_TrustLevel is TrustLevel.Nothing)
        {
            return new();
        }

        AddInDefinitionCollection result = new();
        IEnumerable<Type> types;
        if (searchPrivateTypes)
        {
            types = assembly.DefinedTypes;
        }
        else
        {
            types = assembly.ExportedTypes;
        }

        foreach (Type type in types)
        {
            if (!AttributeResolver.HasAttribute<AddInAttribute>(info: type))
            {
                continue;
            }

            if (!type.IsAssignableTo(typeof(AddIn)))
            {
                continue;
            }

            AddInAttribute attribute = AttributeResolver.FetchSingleAttribute<AddInAttribute>(type);
            AddInDefinition definition = AddInDefinition.FromAttribute(type: type,
                                                                       attribute: attribute);

            this.AddInDiscovering?.Invoke(sender: this,
                                          eventArgs: new(definition));

            if (m_TrustLevel is TrustLevel.All)
            {
                this.AddNullInstance(definition: definition,
                                     definitions: result);
                continue;
            }

            if (m_TrustLevel.HasFlag(TrustLevel.OnlyUserConfirmed) &&
                m_UserTrustedAddIns.Contains(item: definition.Id))
            {
                this.AddNullInstance(definition: definition,
                                     definitions: result);
                continue;
            }

            if (m_TrustLevel.HasFlag(TrustLevel.OnlySystem) &&
                m_SystemTrustedAddIns.Contains(item: definition.Id))
            {
                this.AddNullInstance(definition: definition,
                                     definitions: result);
                continue;
            }

            if (m_TrustLevel.HasFlag(TrustLevel.OnlyUserConfirmed))
            {

                if (m_RequestTrustPrompt.Map(x => x.Invoke(definition)))
                {
                    m_UserTrustedAddIns.Add(item: definition.Id);

                    if (!m_Instances.ContainsKey(definition))
                    {
                        m_Instances.Add(key: definition,
                                        value: null);
                    }

                    result.Add(definition);

                    this.AddInDiscovered?.Invoke(sender: this,
                                                 eventArgs: new(definition));
                }
                else
                {
                    this.AddInDiscoveryFailed?.Invoke(sender: this,
                                                      eventArgs: new(DiscoveryFailedReason.UserTrustDenied));
                }
                continue;
            }
            else if (m_TrustLevel.HasFlag(TrustLevel.OnlySystem))
            {
                this.AddInDiscoveryFailed?.Invoke(sender: this,
                                                  eventArgs: new(DiscoveryFailedReason.NotTrusted));
                continue;
            }
        }
        return result;
    }

    public Boolean IsAddInDiscovered(AddInDefinition definition) =>
        m_Instances.ContainsKey(definition);

    public event EventHandler<AddInManager, AddInDefinitionEventArgs>? AddInDiscovering;
    public event EventHandler<AddInManager, AddInDefinitionEventArgs>? AddInDiscovered;
    public event EventHandler<AddInManager, AddInDiscoveryFailedEventArgs>? AddInDiscoveryFailed;
}

// Trust Lists
partial class AddInManager
{
    public void ClearUserTrustedList() =>
        m_UserTrustedAddIns.Clear();

    public void ReadUserTrustedListFrom([DisallowNull] FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.ReadUserTrustedListFrom(filePath: file.FullName);
    }
    public void ReadUserTrustedListFrom([DisallowNull] String filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        using FileStream stream = File.OpenRead(filePath);
        this.ReadUserTrustedListFrom(stream: stream.AsReadableStream());
    }
    [Obsolete("Please use the more modern IReadableStream interface where possible.", false)]
    public void ReadUserTrustedListFrom([DisallowNull] Stream stream) =>
        ReadUserTrustedListFrom(stream.AsReadableStream());
    public void ReadUserTrustedListFrom<TStream>([DisallowNull] TStream stream)
        where TStream : IReadableStream
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!m_GuidSerializer.HasValue)
        {
            m_GuidSerializer = ByteSerializerDeserializer<Guid[]>.Create();
        }

        try
        {
            Option<Guid[]> trusted = m_GuidSerializer.Map(x => x.Deserialize(stream: stream,
                                                                             actionAfter: SerializationFinishAction.None)!);
            if (!trusted.HasValue)
            {
                trusted = Array.Empty<Guid>();
            }

            trusted.TryGetValue(out Guid[]? guids);
            m_UserTrustedAddIns.AddRange(guids!);
        }
        catch { }
    }

    public void RemoveFromUserTrustedList(in Guid guid) =>
        m_UserTrustedAddIns.Remove(guid);

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
        this.WriteUserTrustedListTo(stream: stream.AsWriteableStream());
    }
    [Obsolete("Please use the more modern IWriteableStream interface where possible.", false)]
    public void WriteUserTrustedListTo([DisallowNull] Stream stream) =>
        this.WriteUserTrustedListTo(stream.AsWriteableStream());
    public void WriteUserTrustedListTo<TStream>([DisallowNull] TStream stream)
        where TStream : IWriteableStream
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!m_GuidSerializer.HasValue)
        {
            m_GuidSerializer = ByteSerializerDeserializer<Guid[]>.Create();
        }

        m_GuidSerializer.Interact(x => x.Serialize(stream: stream,
                                                   graph: m_UserTrustedAddIns.ToArray(),
                                                   actionAfter: SerializationFinishAction.None));
    }

    public ReadOnlyCollection<Guid> UserTrustedAddIns =>
        ReadOnlyCollection<Guid>.CreateFrom(m_UserTrustedAddIns);
}