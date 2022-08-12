namespace Narumikazuchi.Extensibility;

[DebuggerDisplay("{m_AddInDefinition}")]
public abstract partial class AddIn
{
    public virtual void Start() =>
        m_IsRunning = true;

    public virtual void Stop() =>
        m_IsRunning = false;

    public AddInDefinition GetDefinition() =>
        m_AddInDefinition;

    public Boolean IsRunning =>
        m_IsRunning;
}

partial class AddIn
{
    protected AddIn()
    {
        Type type = this.GetType();
        if (!AttributeResolver.HasAttribute<AddInAttribute>(info: type))
        {
            throw new NotSupportedException();
        }

        AddInAttribute attribute = AttributeResolver.FetchSingleAttribute<AddInAttribute>(info: type);
        m_AddInDefinition = AddInDefinition.FromAttribute(type: type,
                                                          attribute: attribute);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly AddInDefinition m_AddInDefinition;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Boolean m_IsRunning;
}