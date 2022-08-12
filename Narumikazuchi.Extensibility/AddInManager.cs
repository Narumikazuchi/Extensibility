using System;
using System.Reflection.Metadata;

namespace Narumikazuchi.Extensibility;

public sealed partial class AddInManager
{
    public static AddInManager Create(TrustLevel trustLevel) =>
        Create(parameterProvider: null,
               trustLevel: trustLevel,
               systemTrustedAddIns: Array.Empty<Guid>(),
               userBlockedAddIns: Array.Empty<Guid>(),
               userTrustedAddIns: Array.Empty<Guid>(),
               requestTrustPrompt: null);
    public static AddInManager Create<TSystemTrusted, TUserBlocked, TUserTrusted>(TrustLevel trustLevel,
                                                                                  [DisallowNull] TSystemTrusted systemTrustedAddIns,
                                                                                  [DisallowNull] TUserBlocked userBlockedAddIns,
                                                                                  [DisallowNull] TUserTrusted userTrustedAddIns)
        where TSystemTrusted : IEnumerable<Guid>
        where TUserBlocked : IEnumerable<Guid>
        where TUserTrusted : IEnumerable<Guid> =>
            Create(parameterProvider: null,
                   trustLevel: trustLevel,
                   systemTrustedAddIns: systemTrustedAddIns,
                   userBlockedAddIns: userBlockedAddIns,
                   userTrustedAddIns: userTrustedAddIns,
                   requestTrustPrompt: null);
    public static AddInManager Create<TSystemTrusted, TUserBlocked, TUserTrusted>([AllowNull] Option<AddInParameterProvider> parameterProvider,
                                                                                  TrustLevel trustLevel,
                                                                                  [DisallowNull] TSystemTrusted systemTrustedAddIns,
                                                                                  [DisallowNull] TUserBlocked userBlockedAddIns,
                                                                                  [DisallowNull] TUserTrusted userTrustedAddIns)
        where TSystemTrusted : IEnumerable<Guid>
        where TUserBlocked : IEnumerable<Guid>
        where TUserTrusted : IEnumerable<Guid>
    {
        ArgumentNullException.ThrowIfNull(systemTrustedAddIns);
        ArgumentNullException.ThrowIfNull(userTrustedAddIns);

        AddInManager result = new(parameterProvider: parameterProvider,
                                  trustLevel: trustLevel,
                                  requestTrustPrompt: null);

        foreach (Guid id in systemTrustedAddIns)
        {
            result.m_SystemTrustedAddIns.Add(id);
        }

        foreach (Guid id in userBlockedAddIns)
        {
            result.m_UserBlockedAddIns.Add(id);
        }

        foreach (Guid id in userTrustedAddIns)
        {
            result.m_UserTrustedAddIns.Add(id);
        }

        return result;
    }
    public static AddInManager Create<TSystemTrusted, TUserBlocked, TUserTrusted>([AllowNull] Option<AddInParameterProvider> parameterProvider,
                                                                                  TrustLevel trustLevel,
                                                                                  [DisallowNull] TSystemTrusted systemTrustedAddIns,
                                                                                  [DisallowNull] TUserBlocked userBlockedAddIns,
                                                                                  [DisallowNull] TUserTrusted userTrustedAddIns,
                                                                                  [AllowNull] Func<AddInDefinition, Boolean>? requestTrustPrompt)
        where TSystemTrusted : IEnumerable<Guid>
        where TUserBlocked : IEnumerable<Guid>
        where TUserTrusted : IEnumerable<Guid>
    {
        ArgumentNullException.ThrowIfNull(systemTrustedAddIns);
        ArgumentNullException.ThrowIfNull(userTrustedAddIns);

        AddInManager result = new(parameterProvider: parameterProvider,
                                  trustLevel: trustLevel,
                                  requestTrustPrompt: requestTrustPrompt);

        foreach (Guid id in systemTrustedAddIns)
        {
            result.m_SystemTrustedAddIns.Add(id);
        }

        foreach (Guid id in userBlockedAddIns)
        {
            result.m_UserBlockedAddIns.Add(id);
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
    private AddInManager(Option<AddInParameterProvider> parameterProvider,
                         TrustLevel trustLevel,
                         Option<Func<AddInDefinition, Boolean>> requestTrustPrompt)
    {
        if (parameterProvider.HasValue)
        {
            m_ParameterProvider = parameterProvider!;
        }
        else
        {
            m_ParameterProvider = new GenericParameterProvider();
        }

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
        foreach (ConstructorInfo constructor in constructorsArray!)
        {
            if (!this.TryAssembleConstructor(constructor: constructor,
                                             currentParameters: out List<Object?> currentParameters))
            {
                continue;
            }

            if (TryInstantiateAddIn(constructor: constructor,
                                    currentParameters: currentParameters,
                                    exceptions: exceptions,
                                    addIn: out Option<AddIn> addIn))
            {
                if (definition.InjectAsDependency)
                {
                    addIn.TryGetValue(out AddIn? addInInstance);
                    m_ParameterProvider.AddParameter(parameterType: type!,
                                                     parameter: addInInstance!);
                }

                m_Instances[definition] = addIn;
                addIn.Interact(x => x.Start());
                this.AddInActivated?.Invoke(sender: addIn,
                                            eventArgs: EventArgs.Empty);
                return addIn;
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

    private Boolean TryAssembleConstructor(ConstructorInfo constructor,
                                           out List<Object?> currentParameters)
    {
        ParameterInfo[] constructorParameters = constructor.GetParameters();
        currentParameters = new();
        if (constructorParameters.Length == 0)
        {
            return true;
        }
        else
        {
            foreach (ParameterInfo parameter in constructorParameters)
            {
                if (!this.TryResolveParameter(parameter: parameter,
                                              currentParameters: currentParameters))
                {
                    return false;
                }
            }

            return true;
        }
    }

    private Boolean TryResolveParameter(ParameterInfo parameter,
                                        List<Object?> currentParameters)
    {
        Type parameterType = parameter.ParameterType;
        Boolean isOption = false;
        if (parameter.ParameterType.IsGenericType &&
            parameter.ParameterType.GetGenericTypeDefinition() == typeof(Option<>))
        {
            parameterType = parameter.ParameterType.GetGenericArguments()[0];
            isOption = true;
        }

        Option<Object> param = m_ParameterProvider.GetParameter(parameterType);
        if (param.HasValue)
        {
            if (isOption)
            {
                param.TryGetValue(out Object? paramValue);
                paramValue = parameter.ParameterType.GetMethod(name: "op_Implicit",
                                                               types: new Type[] { parameterType })!
                                                    .Invoke(obj: null,
                                                            parameters: new Object?[] { paramValue });
                currentParameters.Add(paramValue!);
                return true;
            }
            else
            {
                param.TryGetValue(out Object? paramValue);
                currentParameters.Add(paramValue!);
                return true;
            }
        }
        else if (isOption ||
                 AttributeResolver.HasAttribute<OptionalAttribute>(parameter))
        {
            currentParameters.Add(null);
            return true;
        }
        else
        {
            return false;
        }
    }

    private static Boolean TryInstantiateAddIn(ConstructorInfo constructor,
                                               List<Object?> currentParameters,
                                               List<Exception> exceptions,
                                               out Option<AddIn> addIn)
    {
        try
        {
            addIn = (AddIn)constructor.Invoke(currentParameters.ToArray());
            return true;
        }
        catch (Exception exception)
        {
            addIn = null;
            exceptions.Add(exception);
            return false;
        }
    }

    private readonly Dictionary<AddInDefinition, Option<AddIn>> m_Instances = new();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly HashSet<Guid> m_UserBlockedAddIns = new();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly HashSet<Guid> m_UserTrustedAddIns = new();
    private readonly HashSet<Guid> m_SystemTrustedAddIns = new();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly AddInParameterProvider m_ParameterProvider;
    private readonly TrustLevel m_TrustLevel;
    private Option<ByteSerializerDeserializer<Guid[]>> m_GuidSerializer = null;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Option<Func<AddInDefinition, Boolean>> m_RequestTrustPrompt;
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

    public AddInParameterProvider ParameterProvider =>
        m_ParameterProvider;
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
            match: null);

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
                if (m_UserBlockedAddIns.Contains(item: definition.Id))
                {
                    this.AddInDiscoveryFailed?.Invoke(sender: this,
                                                      eventArgs: new(DiscoveryFailedReason.UserTrustDenied));
                }
                else if (m_RequestTrustPrompt.Map(x => x.Invoke(definition)))
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
                    m_UserBlockedAddIns.Add(definition.Id);
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

    public Option<Func<AddInDefinition, Boolean>> UserPromptOnNewAddIn
    {
        get => m_RequestTrustPrompt;
        set => m_RequestTrustPrompt = value;
    }
}

// Trust Lists
partial class AddInManager
{
    public void ClearUserBlockedList() =>
        m_UserBlockedAddIns.Clear();

    public void ClearUserTrustedList() =>
        m_UserTrustedAddIns.Clear();

    public void ReadUserBlockedListFrom([DisallowNull] FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.ReadUserBlockedListFrom(filePath: file.FullName);
    }
    public void ReadUserBlockedListFrom([DisallowNull] String filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        using FileStream stream = File.OpenRead(filePath);
        this.ReadUserBlockedListFrom(stream: stream.AsReadableStream());
    }
    [Obsolete("Please use the more modern IReadableStream interface where possible.", false)]
    public void ReadUserBlockedListFrom([DisallowNull] Stream stream) =>
        this.ReadUserBlockedListFrom(stream.AsReadableStream());
    public void ReadUserBlockedListFrom<TStream>([DisallowNull] TStream stream)
        where TStream : IReadableStream => 
            m_UserBlockedAddIns.AddRange(this.ReadGuidListFrom(stream));

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
        this.ReadUserTrustedListFrom(stream.AsReadableStream());
    public void ReadUserTrustedListFrom<TStream>([DisallowNull] TStream stream)
        where TStream : IReadableStream =>
            m_UserTrustedAddIns.AddRange(this.ReadGuidListFrom(stream));

    public void RemoveFromUserBlockedList(in Guid guid) =>
        m_UserBlockedAddIns.Remove(guid);

    public void RemoveFromUserTrustedList(in Guid guid) =>
        m_UserTrustedAddIns.Remove(guid);

    public void WriteUserBlockedListTo([DisallowNull] FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.WriteUserBlockedListTo(filePath: file.FullName);
    }
    public void WriteUserBlockedListTo([DisallowNull] String filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using FileStream stream = File.Create(filePath);
        this.WriteUserBlockedListTo(stream: stream.AsWriteableStream());
    }
    [Obsolete("Please use the more modern IWriteableStream interface where possible.", false)]
    public void WriteUserBlockedListTo([DisallowNull] Stream stream) =>
        this.WriteUserBlockedListTo(stream.AsWriteableStream());
    public void WriteUserBlockedListTo<TStream>([DisallowNull] TStream stream)
        where TStream : IWriteableStream =>
            this.WriteGuidListTo(stream: stream,
                                 guids: m_UserBlockedAddIns);

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
        where TStream : IWriteableStream =>
            this.WriteGuidListTo(stream: stream,
                                 guids: m_UserTrustedAddIns);

    private Guid[] ReadGuidListFrom<TStream>(TStream stream)
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
            return guids!;
        }
        catch
        {
            return Array.Empty<Guid>();
        }
    }

    private void WriteGuidListTo<TStream>(TStream stream,
                                          HashSet<Guid> guids)
        where TStream : IWriteableStream
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!m_GuidSerializer.HasValue)
        {
            m_GuidSerializer = ByteSerializerDeserializer<Guid[]>.Create();
        }

        m_GuidSerializer.Interact(x => x.Serialize(stream: stream,
                                                   graph: guids.ToArray(),
                                                   actionAfter: SerializationFinishAction.None));
    }

    public ReadOnlyCollection<Guid> UserBlockedAddIns =>
        ReadOnlyCollection<Guid>.CreateFrom(m_UserBlockedAddIns);

    public ReadOnlyCollection<Guid> UserTrustedAddIns =>
        ReadOnlyCollection<Guid>.CreateFrom(m_UserTrustedAddIns);
}