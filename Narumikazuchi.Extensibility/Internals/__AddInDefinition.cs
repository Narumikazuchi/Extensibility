namespace Narumikazuchi.Extensibility;

#pragma warning disable CS0282
internal readonly partial struct __AddInDefinition
{
    public __AddInDefinition(in Guid guid,
                             [DisallowNull] String name,
                             in AlphanumericVersion version,
                             [DisallowNull] Type type)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(type);

        this.UniqueIdentifier = guid;
        this.Name = name;
        this.Version = version;
        m_AssemblyName = type.Assembly
                             .GetName()
                             .FullName;
        m_TypeName = type.FullName!;
    }
    public __AddInDefinition([DisallowNull] IAddIn addIn)
    {
        ArgumentNullException.ThrowIfNull(addIn);

        Type type = addIn.GetType();
        if (!AttributeResolver.HasAttribute<AddInAttribute>(info: type))
        {
            throw new NotSupportedException();
        }

        AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(info: type);

        this.UniqueIdentifier = attribute.UniqueIdentifier;
        this.Name = attribute.Name;
        this.Version = attribute.Version;
        m_AssemblyName = type.Assembly
                             .GetName()
                             .FullName;
        m_TypeName = type.FullName!;
    }
}

// Non-Public
partial struct __AddInDefinition
{
    private readonly String m_AssemblyName;
    private readonly String m_TypeName;
}

// IAddInDefinition
partial struct __AddInDefinition : IAddInDefinition
{
    public Guid UniqueIdentifier { get; }

    [NotNull]
    public String Name { get; }

    public AlphanumericVersion Version { get; }

    String IAddInDefinition.AssemblyName =>
        m_AssemblyName;

    String IAddInDefinition.TypeName =>
        m_TypeName;
}

// IEquatable<AddInDefinition>
partial struct __AddInDefinition : IEquatable<__AddInDefinition>
{
    public Boolean Equals([AllowNull] __AddInDefinition other) =>
        this.UniqueIdentifier.Equals(other.UniqueIdentifier) &&
        this.Name.Equals( other.Name) &&
        this.Version.Equals(other.Version);

    public override Boolean Equals([AllowNull] Object? obj) =>
        obj is __AddInDefinition other &&
        this.Equals(other: other);

    public override Int32 GetHashCode() =>
        this.UniqueIdentifier.GetHashCode() ^
        this.Name.GetHashCode() ^
        this.Version.GetHashCode();

#pragma warning disable
    public static Boolean operator ==(__AddInDefinition left, __AddInDefinition right)
    {
        return left.Equals(right);
    }

    public static Boolean operator !=(__AddInDefinition left, __AddInDefinition right)
    {
        return !left.Equals(right);
    }
#pragma warning restore
}