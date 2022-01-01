﻿namespace Narumikazuchi.Extensibility;

/// <summary>
/// Marks a class as an AddIn.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed partial class AddInAttribute :  Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddInAttribute"/> class.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    public AddInAttribute([DisallowNull] String name, 
                          [DisallowNull] String guid, 
                          [DisallowNull] String version)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }
        if (guid is null)
        {
            throw new ArgumentNullException(nameof(guid));
        }
        if (!Guid.TryParse(input: guid, 
                            result: out Guid id))
        {
            throw new ArgumentException(message: GUID_PARSE_ERROR, 
                                        paramName: nameof(guid));
        }
        if (!AlphanumericVersion.TryParse(source: version, 
                                            result: out AlphanumericVersion ver))
        {
            throw new ArgumentException(message: VERSION_PARSE_ERROR,
                                        paramName: nameof(version));
        }

        this.Name = name;
        this.UniqueIdentifier = id;
        this.Version = ver;
    }

    /// <summary>
    /// Gets the internal name of the AddIn.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    [Pure]
    [NotNull]
    public String Name { get; }
    /// <summary>
    /// Gets the unique GUID of the AddIn.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    [Pure]
    public Guid UniqueIdentifier { get; }
    /// <summary>
    /// Gets the release version of the AddIn.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    [Pure]
    public AlphanumericVersion Version { get; }
}

partial class AddInAttribute
{
#pragma warning disable
    private const String GUID_PARSE_ERROR = "Could not parse the specified string to a Guid.";
    private const String VERSION_PARSE_ERROR = "Could not parse the specified string to a Version.";
#pragma warning restore
}