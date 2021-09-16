using System;
using System.Diagnostics;
using System.IO;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Represents the process in which an AddIn is running.
    /// </summary>
    public sealed partial class AddInProcess
    {
        /// <summary>
        /// Gets whether this process is the same as the host application.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        public Boolean IsHostProcess => 
            this._disposed
                ? throw new ObjectDisposedException(nameof(AddInProcess))
                : this._hostProcess;
        /// <summary>
        /// Gets the id of this process.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        public Int32 ProcessId =>
            this._disposed
                ? throw new ObjectDisposedException(nameof(AddInProcess))
                : this._processId;
        /// <summary>
        /// Gets the handle to this process.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        public IntPtr Handle =>
            this._disposed
                ? throw new ObjectDisposedException(nameof(AddInProcess))
                : this._processHandle;
        /// <summary>
        /// Gets the lifetime of this process.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        public TimeSpan Lifetime =>
            this._disposed
                ? throw new ObjectDisposedException(nameof(AddInProcess))
                : DateTime.Now - this._startTime;
    }

    // Non-Public
    partial class AddInProcess
    {
        internal AddInProcess(AddInDefinition definition,
                              IStoreIsolation store)
        {
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            this._definition = definition;
            this._store = store;
        }

        internal Boolean Start()
        {
            if (this._disposed)
            {
                return false;
            }

            if (this._definition.Security is AddInSecurity.Trusted)
            {
                this._hostProcess = true;
                this._processId = Environment.ProcessId;
                this._processHandle = Process.GetCurrentProcess().Handle;
                this._startTime = _hostStartTime;
                return true;
            }
            return this.StartIsolatedInternal();
        }

        private Boolean StartIsolatedInternal()
        {
            this._process = this._store.CreateIsolatedProcess(this._definition);
            if (!this._process.Start())
            {
                return false;
            }

            this._startTime = DateTime.Now;
            this._hostProcess = false;
            this._processId = this._process.Id;
            this._processHandle = this._process.Handle;
            return true;
        }

        internal void Kill()
        {
            if (this._disposed)
            {
                return;
            }

            if (this._definition.Security is AddInSecurity.Trusted)
            {
                return;
            }
            this.KillIsolatedInternal();
        }

        private void KillIsolatedInternal()
        {
            if (this._process is null ||
                this._process.HasExited)
            {
                this._process = null;
                this.ClearTempDirectory();
                return;
            }

            this._process.Kill();
            this._process.WaitForExit();
            this._process = null;
            this.ClearTempDirectory();
        }

        private void ClearTempDirectory()
        {
            if (!this._store.IsolationSettings.DirectoryIsTemporary ||
                !this._store.IsolationSettings.IsolationDirectory.Exists)
            {
                return;
            }
            String proc = Path.Combine(this._store.IsolationSettings.IsolationDirectory.FullName,
                                       this._definition.Name + ".exe");
            if (File.Exists(proc))
            {
                File.Delete(proc);
            }
            FileInfo[] files = this._store.IsolationSettings.IsolationDirectory.GetFiles();
            if (files.Length == 0)
            {
                this._store.IsolationSettings.IsolationDirectory.Delete();
                return;
            }
        }

        private static readonly DateTime _hostStartTime = DateTime.Now;
        private readonly AddInDefinition _definition;
        private readonly IStoreIsolation _store;
        private DateTime _startTime = DateTime.MinValue;
        private Process? _process = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Boolean _hostProcess = false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Int32 _processId = 0;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IntPtr _processHandle = IntPtr.Zero;
    }

    // IDisposable
    partial class AddInProcess : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.Kill();
            if (this._definition.Security is AddInSecurity.Isolated &&
                this._process is not null)
            {
                this._process.Dispose();
                this._process = null;
            }
            this._disposed = true;
        }

        private Boolean _disposed = false;
    }
}
