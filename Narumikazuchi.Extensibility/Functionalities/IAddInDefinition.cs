namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the main information of an AddIn.
/// </summary>
public interface IAddInDefinition
{
    /// <summary>
    /// Gets the GUID of this AddIn.
    /// </summary>
    [Pure]
    public Guid UniqueIdentifier { get; }
    /// <summary>
    /// Gets the name of this AddIn.
    /// </summary>
    [Pure]
    [NotNull]
    public String Name { get; }
    /// <summary>
    /// Gets the version number of this AddIn.
    /// </summary>
    [Pure]
    public AlphanumericVersion Version { get; }

    /// <summary>
    /// Gets the display name of the assembly where the type resides.
    /// </summary>
    [Pure]
    [NotNull]
    internal protected String AssemblyName { get; }

    /// <summary>
    /// Gets the Fullname of the type.
    /// </summary>
    [Pure]
    [NotNull]
    internal protected String TypeName { get; }
}