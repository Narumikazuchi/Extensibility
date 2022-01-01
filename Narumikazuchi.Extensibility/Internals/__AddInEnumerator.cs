namespace Narumikazuchi.Extensibility;

internal sealed partial class __AddInEnumerator
{
    public  __AddInEnumerator([DisallowNull] Assembly assembly)
    {
        ExceptionHelpers.ThrowIfArgumentNull(assembly);
        this._assembly = assembly;
    }
}

// Non-Public
partial class __AddInEnumerator
{
    private readonly Assembly _assembly;
}

// IEnumerable
partial class __AddInEnumerator : IEnumerable
{
    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();
}

// IEnumerable<T>
partial class __AddInEnumerator : IEnumerable<AddInDefinition>
{
    public IEnumerator<AddInDefinition> GetEnumerator()
    {
        foreach (Type type in this._assembly.GetTypes())
        {
            if (!AttributeResolver.HasAttribute<AddInAttribute>(info: type))
            {
                continue;
            }
            Boolean cancel = true;
            foreach (Type @interface in type.GetInterfaces())
            {
                if (@interface.GetGenericTypeDefinition() == typeof(IAddIn<>))
                {
                    cancel = false;
                    break;
                }
                if (@interface.GetGenericTypeDefinition() == typeof(IAddIn<,>))
                {
                    cancel = false;
                    break;
                }
            }
            if (cancel)
            {
                continue;
            }

            AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(type);
            yield return new(guid: attribute.UniqueIdentifier,
                             name: attribute.Name,
                             version: attribute.Version,
                             type: type);
        }
        yield break;
    }
}