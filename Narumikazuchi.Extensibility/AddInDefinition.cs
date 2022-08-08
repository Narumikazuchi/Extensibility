namespace Narumikazuchi.Extensibility;

#pragma warning disable CS0282
public readonly partial struct AddInDefinition
{
    public static AddInDefinition FromAttribute<T>(AddInAttribute attribute) =>
        FromAttribute(type: typeof(T),
                      attribute: attribute);
    public static AddInDefinition FromAttribute(Type type,
                                                AddInAttribute attribute)
    {
        AddInDefinition result = new(id: attribute.Id,
                                     name: attribute.Name,
                                     version: attribute.Version,
                                     assembly: type.Assembly.GetName().FullName,
                                     type: type.FullName!);

        s_KnownDefinitions.TryAdd(key: type,
                                  value: result);

        return result;
    }

    public Guid Id { get; }

    public Option<String> Name { get; }

    public AlphanumericVersion Version { get; }
}

// Non-Public
partial struct AddInDefinition
{
    internal AddInDefinition(Guid id,
                             String name,
                             AlphanumericVersion version,
                             String assembly,
                             String type)
    {
        name.ThrowIfNullOrEmpty(asArgumentException: true);
        assembly.ThrowIfNullOrEmpty(asArgumentException: true);
        type.ThrowIfNullOrEmpty(asArgumentException: true);

        this.Id = id;
        this.Name = name;
        this.Version = version;
        m_AssemblyName = assembly;
        m_TypeName = type;
    }

    internal String AssemblyName =>
        m_AssemblyName;

    internal String TypeName =>
        m_TypeName;

    internal static readonly Dictionary<Type, AddInDefinition> s_KnownDefinitions = new();
    private readonly String m_AssemblyName;
    private readonly String m_TypeName;
}