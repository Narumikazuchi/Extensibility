using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Represents the settings for running AddIns in the <see cref="AddInSecurity.Isolated"/> security mode.
    /// </summary>
    public sealed class IsolationSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsolationSettings"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public IsolationSettings([DisallowNull] String directoryPath,
                                 Boolean isTemporary) :
            this(new DirectoryInfo(directoryPath),
                 isTemporary)
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="IsolationSettings"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public IsolationSettings([DisallowNull] DirectoryInfo directory,
                                 Boolean isTemporary)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            this.DirectoryIsTemporary = isTemporary;
            this.IsolationDirectory = directory;
        }

        /// <summary>
        /// Gets the default settings.
        /// </summary>
        public static IsolationSettings Default { get; } = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                                            "Narumikazuchi.Extensibility.Process" + Environment.ProcessId.ToString()),
                                                               true);

        /// <summary>
        /// Gets the directory where the tempopary executable binaries will be placed.
        /// </summary>
        public DirectoryInfo IsolationDirectory { get; init; }
        /// <summary>
        /// Gets whether the <see cref="IsolationDirectory"/> will be deleted after the application has exited.
        /// </summary>
        public Boolean DirectoryIsTemporary { get; init; }
    }
}
