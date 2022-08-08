namespace Narumikazuchi.Extensibility;

public partial struct AddInDefinitionsEnumerator
{
}

// Non-Public
partial struct AddInDefinitionsEnumerator
{
    internal AddInDefinitionsEnumerator(Dictionary<AddInDefinition, Option<AddIn>>.Enumerator source,
                                        Func<AddInDefinition, Option<AddIn>, Boolean> match)
    {
        m_Match = match;
        m_Source = source;
        m_State = null;
        m_Current = null;
    }

    private readonly Func<AddInDefinition, Option<AddIn>, Boolean> m_Match;
    private Dictionary<AddInDefinition, Option<AddIn>>.Enumerator m_Source;
    private Option<Boolean?> m_State;
    private Option<AddInDefinition?> m_Current;
}

// IStrongEnumerable<T, V>
partial struct AddInDefinitionsEnumerator : IStrongEnumerable<AddInDefinition, AddInDefinitionsEnumerator>
{
    /// <inheritdoc/>
    public AddInDefinitionsEnumerator GetEnumerator()
    {
        if (m_State.HasValue)
        {
            return new(source: m_Source,
                       match: m_Match);
        }
        else
        {
            return this;
        }
    }
}

// IStrongEnumerator<T>
partial struct AddInDefinitionsEnumerator : IStrongEnumerator<AddInDefinition>
{
    /// <inheritdoc/>
    public Boolean MoveNext()
    {
        if (m_State.HasValue &&
            (Boolean?)m_State == false)
        {
            return false;
        }

        if (!m_State.HasValue)
        {
            m_State = true;
        }

        while (m_Source.MoveNext())
        {
            if (m_Match.Invoke(m_Source.Current.Key, m_Source.Current.Value))
            {
                m_Current = m_Source.Current.Key;
                return true;
            }
        }

        m_State = false;
        return false;
    }

    /// <inheritdoc/>
    public AddInDefinition Current
    {
        get
        {
            if (m_State.HasValue &&
                (Boolean?)m_State == true)
            {
                return (AddInDefinition)m_Current!;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}