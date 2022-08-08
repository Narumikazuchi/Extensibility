namespace Narumikazuchi.Extensibility;

[Singleton]
public sealed partial class AddInEqualityComparer
{ }

// IEqualityComparer<T>
partial class AddInEqualityComparer : IEqualityComparer<AddInDefinition>
{
    /// <inheritdoc/>
    public Boolean Equals(AddInDefinition x,
                          AddInDefinition y) =>
        x.Name == y.Name &&
        x.Version == y.Version &&
        x.Id == y.Id;

    /// <inheritdoc/>
    public Int32 GetHashCode(AddInDefinition obj) =>
        obj.GetHashCode();
}