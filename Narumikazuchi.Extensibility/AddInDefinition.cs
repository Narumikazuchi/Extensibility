namespace Narumikazuchi.Extensibility;

/// <summary>
/// Provides the information about a specific AddIn.
/// </summary>
public readonly partial struct AddInDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddInDefinition"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public AddInDefinition(in Guid guid,
                           [DisallowNull] String name,
                           in AlphanumericVersion version,
                           [DisallowNull] Type type)
    {
        ExceptionHelpers.ThrowIfArgumentNull(name);
        ExceptionHelpers.ThrowIfArgumentNull(type);

        this._guid = guid;
        this._name = name;
        this._version = version;
        this._assemblyName = type.Assembly
                                 .FullName!;
        this._typeName = type.FullName!;
        this._assembly = new(type.Assembly
                                 .Location);
    }

    /// <summary>
    /// Gets the GUID of this AddIn.
    /// </summary>
    [Pure]
    public Guid UniqueIdentifier => 
        this._guid;
    /// <summary>
    /// Gets the name of this AddIn.
    /// </summary>
    [Pure]
    [NotNull]
    public String Name => 
        this._name;
    /// <summary>
    /// Gets the version number of this AddIn.
    /// </summary>
    [Pure]
    public AlphanumericVersion Version => 
        this._version;
}

// Non-Public
partial struct AddInDefinition
{
    internal AddInDefinition(IAddIn addIn)
    {
        ExceptionHelpers.ThrowIfArgumentNull(addIn);

        Type type = addIn.GetType();
        if (!AttributeResolver.HasAttribute<AddInAttribute>(info: type))
        {
            throw new NotSupportedException();
        }
        AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(info: type);

        this._guid = attribute.UniqueIdentifier;
        this._name = attribute.Name;
        this._version = attribute.Version;
        this._assemblyName = type.Assembly
                                 .FullName!;
        this._typeName = type.FullName!;
        this._assembly = new(type.Assembly
                                 .Location);
    }
    internal AddInDefinition(in Guid guid,
                             String name,
                             AlphanumericVersion version,
                             String typeName,
                             String assemblyName,
                             String assemblyLocation)
    {
        ExceptionHelpers.ThrowIfArgumentNull(name);
        ExceptionHelpers.ThrowIfArgumentNull(typeName);
        ExceptionHelpers.ThrowIfArgumentNull(assemblyName);
        ExceptionHelpers.ThrowIfArgumentNull(assemblyLocation);

        this._guid = guid;
        this._name = name;
        this._version = version;
        this._assemblyName = assemblyName;
        this._typeName = typeName;
        this._assembly = new(assemblyLocation);
    }

    internal String AssemblyName => 
        this._assemblyName;
    internal String TypeName => 
        this._typeName;
    internal FileInfo Assembly => 
        this._assembly;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Guid _guid;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly String _name;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly AlphanumericVersion _version;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly String _assemblyName;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly String _typeName;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly FileInfo _assembly;
}

// IDeserializable<T>
partial struct AddInDefinition : IDeserializable<AddInDefinition>
{
    static AddInDefinition IDeserializable<AddInDefinition>.ConstructFromSerializationData(ISerializationInfoGetter info)
    {
        Guid id = info.GetState<Guid>(nameof(UniqueIdentifier));
        String name = info.GetState<String>(nameof(Name))!;
        String versionRaw = info.GetState<String>(nameof(Version))!;
        String assemblyName = info.GetState<String>(nameof(AssemblyName))!;
        String typeName = info.GetState<String>(nameof(TypeName))!;
        String assemblyLocation = info.GetState<String>(nameof(Assembly))!;

        AlphanumericVersion version = AlphanumericVersion.Parse(versionRaw);

        return new(guid: id,
                   name: name,
                   version: version,
                   typeName: typeName,
                   assemblyName: assemblyName,
                   assemblyLocation: assemblyLocation);
    }
}

// IEquatable<AddInDefinition>
partial struct AddInDefinition : IEquatable<AddInDefinition>
{
    /// <inheritdoc/>
    public Boolean Equals([AllowNull] AddInDefinition other) =>
        this.UniqueIdentifier.Equals(other.UniqueIdentifier) &&
        this.Name.Equals( other.Name) &&
        this.Version.Equals(other.Version);

    /// <inheritdoc/>
    public override Boolean Equals([AllowNull] Object? obj) =>
        obj is AddInDefinition other &&
        this.Equals(other: other);

    /// <inheritdoc/>
    public override Int32 GetHashCode() =>
        this.UniqueIdentifier.GetHashCode() ^
        this.Name.GetHashCode() ^
        this.Version.GetHashCode();

#pragma warning disable
    public static Boolean operator ==(AddInDefinition left, AddInDefinition right) =>
        left.Equals(right);
    public static Boolean operator !=(AddInDefinition left, AddInDefinition right) =>
        !left.Equals(right);
#pragma warning restore
}

// ISerializable
partial struct AddInDefinition : ISerializable
{
    void ISerializable.GetSerializationData([DisallowNull] ISerializationInfoAdder info)
    {
        info.AddState(memberName: nameof(this.UniqueIdentifier),
                      memberValue: this.UniqueIdentifier);
        info.AddState(memberName: nameof(this.Name),
                      memberValue: this.Name);
        info.AddState(memberName: nameof(this.Version),
                      memberValue: this.Version
                                       .ToString());
        info.AddState(memberName: nameof(this.AssemblyName),
                      memberValue: this.AssemblyName);
        info.AddState(memberName: nameof(this.TypeName),
                      memberValue: this.TypeName);
        info.AddState(memberName: nameof(this.Assembly),
                      memberValue: this.Assembly
                                       .FullName);
    }
}