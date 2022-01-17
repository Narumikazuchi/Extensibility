namespace Narumikazuchi.Extensibility;

/// <summary>
/// Contains the <see cref="IAddInDefinition"/> that is tied to the current event.
/// </summary>
public sealed class AddInDefinitionEventArgs : EventArgs
{
    /// <summary>
    /// Creates a new instance of the <see cref="AddInDefinitionEventArgs"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public AddInDefinitionEventArgs([DisallowNull] IAddInDefinition addIn)
    {
        ExceptionHelpers.ThrowIfArgumentNull(addIn);

        this.AddIn = addIn;
    }

    /// <summary>
    /// Gets the <see cref="IAddInDefinition"/> that is tied to the event.
    /// </summary>
    [NotNull]
    [Pure]
    public IAddInDefinition AddIn { get; }
}