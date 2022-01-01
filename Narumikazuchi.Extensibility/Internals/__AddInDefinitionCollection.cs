namespace Narumikazuchi.Extensibility;

internal sealed partial class __AddInDefinitionCollection
{
    public __AddInDefinitionCollection()
    {
        this._definitions = new();
        this._userTrusted = new();
    }
    public __AddInDefinitionCollection([DisallowNull] IEnumerable<AddInDefinition> definitions)
    {
        ExceptionHelpers.ThrowIfArgumentNull(definitions);

        this._definitions = new(collection: definitions);
        this._userTrusted = new();
    }
    public __AddInDefinitionCollection([DisallowNull] IEnumerable<AddInDefinition> definitions,
                                       [DisallowNull] IEnumerable<Guid> userTrusted)
    {
        ExceptionHelpers.ThrowIfArgumentNull(definitions);
        ExceptionHelpers.ThrowIfArgumentNull(userTrusted);

        this._definitions = new(collection: definitions);
        this._userTrusted = new(collection: userTrusted);
    }
}

// Non-Public
partial class __AddInDefinitionCollection
{
    private readonly List<AddInDefinition> _definitions;
    private readonly List<Guid> _userTrusted;
}

// IEnumerable
partial class __AddInDefinitionCollection : IEnumerable
{
    IEnumerator IEnumerable.GetEnumerator() =>
        this._definitions
            .GetEnumerator();
}

// IEnumerable<T> - AddInDefinition
partial class __AddInDefinitionCollection : IEnumerable<AddInDefinition>
{
    IEnumerator<AddInDefinition> IEnumerable<AddInDefinition>.GetEnumerator() =>
        this._definitions
            .GetEnumerator();
}

// IEnumerable<T> - Guid
partial class __AddInDefinitionCollection : IEnumerable<Guid>
{
    IEnumerator<Guid> IEnumerable<Guid>.GetEnumerator() =>
        this._userTrusted
            .GetEnumerator();
}

// IDeserializable<T>
partial class __AddInDefinitionCollection : IDeserializable<__AddInDefinitionCollection>
{
    [return: NotNull]
    public static __AddInDefinitionCollection ConstructFromSerializationData([DisallowNull] ISerializationInfoGetter info) =>
        new(definitions: info.GetState<AddInDefinition[]>("AddIns")!,
            userTrusted: info.GetState<Guid[]>("Trusted")!);
}

// ISerializable
partial class __AddInDefinitionCollection : ISerializable
{
    public void GetSerializationData([DisallowNull] ISerializationInfoAdder info)
    {
        info.AddState(memberName: "AddIns",
                      memberValue: this._definitions
                                       .ToArray());
        info.AddState(memberName: "Trusted",
                      memberValue: this._userTrusted
                                       .ToArray());
    }
}