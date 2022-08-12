namespace Narumikazuchi.Extensibility;

public sealed partial class GenericParameterProvider : AddInParameterProvider
{
    public override void AddParameter([DisallowNull] Type parameterType,
                                      [DisallowNull] Object parameter)
    {
        ArgumentNullException.ThrowIfNull(parameterType);
        ArgumentNullException.ThrowIfNull(parameter);

        this.AddTypeWithObject(type: parameterType,
                               @object: parameter);
    }
    public override void AddParameter([DisallowNull] Type parameterType,
                                      [DisallowNull] Func<AddInParameterProvider, Object> factory)
    {
        ArgumentNullException.ThrowIfNull(parameterType);
        ArgumentNullException.ThrowIfNull(factory);

        this.AddTypeWithFactory(type: parameterType,
                                factory: factory);
    }

    public override void RemoveParameter([DisallowNull] Type parameterType,
                                         [DisallowNull] Object parameter)
    {
        ArgumentNullException.ThrowIfNull(parameterType);
        ArgumentNullException.ThrowIfNull(parameter);

        this.RemoveTypeWithObject(type: parameterType,
                                  @object: parameter);
    }

    public override void RemoveParameters([DisallowNull] Type parameterType)
    {
        ArgumentNullException.ThrowIfNull(parameterType);

        if (m_Factories.ContainsKey(parameterType))
        {
            m_Factories.Remove(parameterType);
        }

        if (m_Parameters.ContainsKey(parameterType))
        {
            m_Parameters.Remove(parameterType);
        }
    }

    public override Option<Object> GetParameter([DisallowNull] Type parameterType)
    {
        if (parameterType.IsGenericType &&
            parameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return this.GetEnumerableParameter(parameterType);
        }
        else if (parameterType == typeof(IEnumerable))
        {
            return m_Parameters.SelectMany(x => x.Value)
                               .ToArray();
        }
        else
        {
            return this.GetNonEnumerableParameter(parameterType);
        }
    }
}

// Non-Public
partial class GenericParameterProvider
{
    private void AddTypeWithFactory(Type type,
                                    Func<AddInParameterProvider, Object> factory)
    {
        if (m_Factories.ContainsKey(type))
        {
            m_Factories[type] = factory;
        }
        else
        {
            m_Factories.Add(key: type,
                            value: factory);
        }
    }

    private void AddTypeWithObject(Type type,
                                   Object @object)
    {
        if (m_Parameters.ContainsKey(type))
        {
            m_Parameters[type].Add(@object);
        }
        else
        {
            m_Parameters.Add(key: type,
                             value: new()
                             {
                                 @object
                             });
        }
    }

    private void RemoveTypeWithObject(Type type,
                                      Object @object)
    {
        if (m_Parameters.ContainsKey(type))
        {
            m_Parameters[type].Remove(@object);
            if (m_Parameters[type].Count == 0)
            {
                m_Parameters.Remove(type);
            }
        }
    }

    private Option<Object> GetEnumerableParameter(Type parameterType)
    {
        Type searchType = parameterType.GetGenericArguments()[0];
        if (m_Parameters.ContainsKey(searchType))
        {
            return m_Parameters[searchType];
        }
        else if (m_Factories.ContainsKey(searchType))
        {
            return new Object[] { m_Factories[searchType].Invoke(this) };
        }
        else
        {
            return null;
        }
    }

    private Option<Object> GetNonEnumerableParameter(Type parameterType)
    {
        if (m_Factories.ContainsKey(parameterType))
        {
            return m_Factories[parameterType].Invoke(this);
        }
        if (m_Parameters.ContainsKey(parameterType))
        {
            return m_Parameters[parameterType].LastOrDefault();
        }
        else
        {
            return null;
        }
    }

    private readonly Dictionary<Type, Func<AddInParameterProvider, Object>> m_Factories = new();
    private readonly Dictionary<Type, HashSet<Object>> m_Parameters = new();
}