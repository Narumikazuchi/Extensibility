namespace Narumikazuchi.Extensibility;

internal sealed partial class __AddInEqualityComparer
{ }

// Non-Public
partial class __AddInEqualityComparer : Singleton
{
    private __AddInEqualityComparer()
    { }
}

// IEqualityComparer<T>
partial class __AddInEqualityComparer : IEqualityComparer<IAddInDefinition>
{
    public Boolean Equals([AllowNull] IAddInDefinition? x, 
                          [AllowNull] IAddInDefinition? y)
    {
        if (x is null)
        {
            return y is null;
        }
        return y is not null &&
               x.Name == y.Name &&
               x.Version == y.Version &&
               x.UniqueIdentifier == y.UniqueIdentifier;
    }

    public Int32 GetHashCode([DisallowNull] IAddInDefinition obj) => 
        obj.GetHashCode();
}