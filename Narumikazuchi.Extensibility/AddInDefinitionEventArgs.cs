namespace Narumikazuchi.Extensibility;

/// <summary>
/// Contains the <see cref="AddInDefinition"/> that is tied to the current event.
/// </summary>
public sealed class AddInDefinitionEventArgs : EventArgs
{
    /// <summary>
    /// Creates a new instance of the <see cref="AddInDefinitionEventArgs"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public AddInDefinitionEventArgs(AddInDefinition addIn)
    {
        this.Definition = addIn;
    }

    /// <summary>
    /// Gets the <see cref="AddInDefinition"/> that is tied to the event.
    /// </summary>
    [NotNull]
    [Pure]
    public AddInDefinition Definition { get; }
}