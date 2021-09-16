using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Narumikazuchi.Extensibility
{
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
            if (!Guid.TryParse(guid, 
                               out Guid id))
            {
                throw new ArgumentException(GUID_PARSE_ERROR, 
                                            nameof(guid));
            }
            if (!Version.TryParse(
                    version, 
                    out Version? ver))
            {
                throw new ArgumentException(VERSION_PARSE_ERROR,
                                            nameof(version));
            }

            this.Name = name;
            this.Guid = id;
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
        public Guid Guid { get; }
        /// <summary>
        /// Gets the release version of the AddIn.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        [Pure]
        [NotNull]
        public Version Version { get; }
    }

    partial class AddInAttribute
    {
#pragma warning disable
        private const String GUID_PARSE_ERROR = "Could not parse the specified string to a Guid.";
        private const String VERSION_PARSE_ERROR = "Could not parse the specified string to a Version.";
#pragma warning restore
    }
}
