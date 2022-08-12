namespace Narumikazuchi.Extensibility;

#pragma warning disable CS0282
[DebuggerDisplay("{Name} v{Version} ({Id})")]
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
                                     inject: attribute.InjectAsDependency,
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
                             Boolean inject,
                             String assembly,
                             String type)
    {
        name.ThrowIfNullOrEmpty(asArgumentException: true);
        assembly.ThrowIfNullOrEmpty(asArgumentException: true);
        type.ThrowIfNullOrEmpty(asArgumentException: true);

        this.Id = id;
        this.Name = name;
        this.Version = version;
        this.InjectAsDependency = inject;
        this.AssemblyName = assembly;
        this.TypeName = type;
    }

    internal String AssemblyName { get; }

    internal String TypeName { get; }

    internal Boolean InjectAsDependency { get; }

    internal static readonly Dictionary<Type, AddInDefinition> s_KnownDefinitions = new();
}