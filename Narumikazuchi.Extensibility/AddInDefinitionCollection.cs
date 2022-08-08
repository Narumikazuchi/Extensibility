namespace Narumikazuchi.Extensibility;

public sealed partial class AddInDefinitionCollection
{ }

partial class AddInDefinitionCollection
{
    internal AddInDefinitionCollection()
    { }

    internal void Add(AddInDefinition definition) =>
        m_Items.Add(definition);
    internal void Add(AddInDefinitionCollection other)
    {
        foreach (AddInDefinition item in other.m_Items)
        {
            m_Items.Add(item);
        }
    }

    private readonly HashSet<AddInDefinition> m_Items = new(comparer: AddInEqualityComparer.Instance);
}

partial class AddInDefinitionCollection : IEnumerable
{
    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();
}

partial class AddInDefinitionCollection : IEnumerable<AddInDefinition>
{
    IEnumerator<AddInDefinition> IEnumerable<AddInDefinition>.GetEnumerator() =>
        this.GetEnumerator();
}

partial class AddInDefinitionCollection : IStrongEnumerable<AddInDefinition, CommonHashSetEnumerator<AddInDefinition>>
{
    /// <inheritdoc/>
    public CommonHashSetEnumerator<AddInDefinition> GetEnumerator() =>
        new(m_Items);
}