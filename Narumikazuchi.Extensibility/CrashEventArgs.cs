using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Contains information about the crash of an AddIn.
    /// </summary>
    public sealed class CrashEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrashEventArgs"/> class.
        /// </summary>
        public CrashEventArgs([DisallowNull] IEnumerable<String?> exceptions)
        {
            List<String> temp = new();
            foreach (String? e in exceptions)
            {
                if (e is null)
                {
                    continue;
                }
                temp.Add(e);
            }
            this.Exceptions = temp;
        }

        /// <summary>
        /// Gets the list of <see cref="Exception"/> object, that have been thrown.
        /// </summary>
        public IReadOnlyList<String> Exceptions { get; }
    }
}
