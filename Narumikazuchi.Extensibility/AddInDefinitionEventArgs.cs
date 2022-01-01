namespace Narumikazuchi.Extensibility;

/// <summary>
/// Contains the <see cref="AddInDefinition"/> that is tied to the current event.
/// </summary>
public sealed class AddInDefinitionEventArgs : EventArgs
{
    /// <summary>
    /// Creates a new instance of the <see cref="AddInDefinitionEventArgs"/> class.
    /// </summary>
    public AddInDefinitionEventArgs(in AddInDefinition addIn)
    {
        this.AddIn = addIn;
    }

    /// <summary>
    /// Gets the <see cref="AddInDefinition"/> that is tied to the event.
    /// </summary>
    public AddInDefinition AddIn { get; }
}