using Narumikazuchi.Windows.Pipes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Narumikazuchi.Extensibility
{
    internal sealed partial class __IsolatedAddInController
    {
        internal __IsolatedAddInController(AddInDefinition definition,
                                           IStoreEvents store) :
            base(definition,
                 store)
        {
            this._server = Server<__ByteMessage>.CreateServer(definition.Name, 
                                                            1, 
                                                            () => true);
        }

        private void ReceiveExposedMemberInfo(IServer<__ByteMessage> server, 
                                              DataReceivedEventArgs<__ByteMessage> args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            if (args.Data is null)
            {
                throw new InvalidOperationException();
            }
            if (args.Data.Signatures is null)
            {
                throw new InvalidOperationException();
            }
            if (args.Data.Type is not MessageType.ExposedMemberInformation)
            {
                throw new InvalidOperationException();
            }

            foreach (__ByteMessage.SignatureData signature in args.Data.Signatures)
            {
                switch (signature.SignatureType)
                {
                    case SignatureType.Method:
                        MethodSignature method = new(signature.TypeName,
                                                     signature.MemberName,
                                                     signature.ParameterList);
                        this._methods.Add(method);
                        break;
                    case SignatureType.Property:
                        PropertySignature property = new(signature.TypeName,
                                                         signature.MemberName,
                                                         signature.HasGetter,
                                                         signature.HasSetter);
                        this._properties.Add(property);
                        break;
                    case SignatureType.Indexer:
                        IndexerSignature indexer = new(signature.TypeName,
                                                       signature.MemberName,
                                                       signature.ParameterList,
                                                       signature.HasGetter,
                                                       signature.HasSetter);
                        this._indexers.Add(indexer);
                        break;
                }
            }

            this._server.DataReceived -= this.ReceiveExposedMemberInfo;
            this._exposedMembersRecevied = true;
            this._server.DataReceived += this.ReceiveValue;
        }

        private void ReceiveValue(IServer<__ByteMessage> server,
                                  DataReceivedEventArgs<__ByteMessage> args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            if (args.Data is null)
            {
                return;
            }

            if (args.Data.Type is MessageType.InvocationResult &&
                args.Data.Objects is not null &&
                args.Data.Objects.Length > 0)
            {
                this._lastReceivedValue = args.Data.Objects[0].Value;
                this._receivedValue = true;
                return;
            }
            if (args.Data.Type is MessageType.CrashNotification &&
                args.Data.Objects is not null &&
                args.Data.Objects.Length > 0)
            {
                this._store.OnAddInCrashed(this,
                                           args.Data.Objects.Select(o => (String?)o.Value));
                this.Shutdown();
                this.Dispose();
                return;
            }
            if (args.Data.Type is MessageType.ShutdownCommand)
            {
                this.Shutdown();
                this.Dispose();
                return;
            }
        }

        private readonly Server<__ByteMessage> _server;
        private Boolean _exposedMembersRecevied = false;
        private Boolean _receivedValue = false;
        private Object? _lastReceivedValue = null;
    }

    // IAddInController
    partial class __IsolatedAddInController : __AddInControllerBase
    {
        public override async Task<Boolean> ActivateAsync()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }

            if (this.IsActive)
            {
                return false;
            }

            this._store.OnActivating(this);
            this._server.Start();
            if (!this._process.Start())
            {
                return false;
            }
            Boolean @continue = await Task.Run(() =>
            {
                Int32 timeout = 0;
                while (this._server.Clients.Count < 1)
                {
                    Thread.Sleep(1);
                    timeout++;
                    if (timeout >= 15000)
                    {
                        return false;
                    }
                }
                return true;
            });
            if (!@continue)
            {
                return false;
            }
            this._server.DataReceived += this.ReceiveExposedMemberInfo;
            await this._server.BroadcastAsync(__ByteMessage.GetExposedMembers);
            @continue = await Task.Run(() =>
            {
                Int32 timeout = 0;
                while (!this._exposedMembersRecevied)
                {
                    Thread.Sleep(1);
                    timeout++;
                    if (timeout >= 15000)
                    {
                        return false;
                    }
                }
                return true;
            });
            if (@continue)
            {
                this._active = true;
                this._store.OnActivated(this);
                return true;
            }
            return false;
        }

        public override void Shutdown()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }

            this._store.OnShuttingDown(this);
            this._process.Kill();
            this._server.Stop();

            this._active = false;
            this._methods.Clear();
            this._properties.Clear();
            this._indexers.Clear();
            this._store.OnShutdown(this);
        }

        public override async Task InvokeMethodAsync(MethodSignature method,
                                                     [DisallowNull] params Object[] parameters)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }
            if (!this.IsActive)
            {
                throw new InvalidOperationException(CONTROLLER_IS_NOT_ACTIVE);
            }
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (method.Equals(default))
            {
                throw new NotSupportedException(NO_INFORMATION_HAS_BEEN_SET);
            }
            if (method.Parameters.Count != parameters.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(parameters));
            }
            for (Int32 i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].GetType()
                                  .IsAssignableTo(method.Parameters[i]))
                {
                    throw new InvalidCastException(String.Format(PARAMETER_INCOMPATIBLE,
                                                                 i,
                                                                 parameters[i].GetType().FullName,
                                                                 method.Parameters[i].FullName));
                }
            }

            __ByteMessage message = __ByteMessage.InvokeMethod(method,
                                                           parameters);
            await this._server.BroadcastAsync(message);
        }
        public override async Task<TReturn> InvokeMethodAsync<TReturn>(MethodSignature method,
                                                                       [DisallowNull] params Object[] parameters)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }
            if (!this.IsActive)
            {
                throw new InvalidOperationException(CONTROLLER_IS_NOT_ACTIVE);
            }
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (method.Equals(default))
            {
                throw new NotSupportedException(NO_INFORMATION_HAS_BEEN_SET);
            }
            if (!typeof(TReturn).IsAssignableTo(method.ReturnType))
            {
                throw new InvalidCastException(String.Format(TYPE_INCOMPATIBLE,
                                                             typeof(TReturn).FullName,
                                                             method.ReturnType.FullName));
            }
            if (method.Parameters.Count != parameters.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(parameters));
            }
            for (Int32 i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].GetType()
                                  .IsAssignableTo(method.Parameters[i]))
                {
                    throw new InvalidCastException(String.Format(PARAMETER_INCOMPATIBLE,
                                                                 i,
                                                                 parameters[i].GetType().FullName,
                                                                 method.Parameters[i].FullName));
                }
            }

            __ByteMessage message = __ByteMessage.InvokeMethod(method,
                                                           parameters);
            this._receivedValue = false;
            await this._server.BroadcastAsync(message);
            Boolean @continue = await Task.Run(() =>
            {
                Int32 timeout = 0;
                while (!this._receivedValue)
                {
                    Thread.Sleep(1);
                    timeout++;
                    if (timeout >= 15000)
                    {
                        return false;
                    }
                }
                return true;
            });
#pragma warning disable
            return !@continue
                        ? throw new TimeoutException()
                        : (TReturn?)this._lastReceivedValue;
#pragma warning restore
        }

        public override async Task<TProperty> InvokePropertyGetterAsync<TProperty>(PropertySignature property)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }
            if (!this.IsActive)
            {
                throw new InvalidOperationException(CONTROLLER_IS_NOT_ACTIVE);
            }
            if (property.Equals(default))
            {
                throw new NotSupportedException(NO_INFORMATION_HAS_BEEN_SET);
            }
            if (!property.HasGetter)
            {
                throw new NotSupportedException(PROPERTY_HAS_NO_GETTER);
            }
            if (!typeof(TProperty).IsAssignableTo(property.Type))
            {
                throw new InvalidCastException(String.Format(TYPE_INCOMPATIBLE,
                                                             typeof(TProperty).FullName,
                                                             property.Type.FullName));
            }

            __ByteMessage message = __ByteMessage.InvokePropertyGetter(property);
            this._receivedValue = false;
            await this._server.BroadcastAsync(message);
            Boolean @continue = await Task.Run(() =>
            {
                Int32 timeout = 0;
                while (!this._receivedValue)
                {
                    Thread.Sleep(1);
                    timeout++;
                    if (timeout >= 15000)
                    {
                        return false;
                    }
                }
                return true;
            });
#pragma warning disable
            return !@continue
                        ? throw new TimeoutException()
                        : (TProperty?)this._lastReceivedValue;
#pragma warning restore
        }

        public override async Task InvokePropertySetterAsync<TProperty>(PropertySignature property,
                                                                        [AllowNull] TProperty value)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }
            if (!this.IsActive)
            {
                throw new InvalidOperationException(CONTROLLER_IS_NOT_ACTIVE);
            }
            if (property.Equals(default))
            {
                throw new NotSupportedException(NO_INFORMATION_HAS_BEEN_SET);
            }
            if (!property.HasSetter)
            {
                throw new NotSupportedException(PROPERTY_HAS_NO_SETTER);
            }
            if (!typeof(TProperty).IsAssignableTo(property.Type))
            {
                throw new InvalidCastException(String.Format(TYPE_INCOMPATIBLE,
                                                             typeof(TProperty).FullName,
                                                             property.Type.FullName));
            }

            __ByteMessage message = __ByteMessage.InvokePropertySetter(property,
                                                                   value);
            await this._server.BroadcastAsync(message);
        }

        public override async Task<TIndex> InvokeIndexerGetterAsync<TIndex>(IndexerSignature indexer,
                                                                            [DisallowNull] params Object[] indecies)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }
            if (!this.IsActive)
            {
                throw new InvalidOperationException(CONTROLLER_IS_NOT_ACTIVE);
            }
            if (indexer.Equals(default))
            {
                throw new NotSupportedException(NO_INFORMATION_HAS_BEEN_SET);
            }
            if (indecies is null)
            {
                throw new ArgumentNullException(nameof(indecies));
            }
            if (!indexer.HasGetter)
            {
                throw new NotSupportedException(INDEXER_HAS_NO_GETTER);
            }
            if (!typeof(TIndex).IsAssignableTo(indexer.Type))
            {
                throw new InvalidCastException(String.Format(TYPE_INCOMPATIBLE,
                                                             typeof(TIndex).FullName,
                                                             indexer.Type.FullName));
            }
            if (indexer.Indecies.Count != indecies.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(indecies));
            }
            for (Int32 i = 0; i < indecies.Length; i++)
            {
                if (!indecies[i].GetType()
                                .IsAssignableTo(indexer.Indecies[i]))
                {
                    throw new InvalidCastException(String.Format(PARAMETER_INCOMPATIBLE,
                                                                 i,
                                                                 indecies[i].GetType().FullName,
                                                                 indexer.Indecies[i].FullName));
                }
            }

            __ByteMessage message = __ByteMessage.InvokeIndexGetter(indexer,
                                                                indecies);
            this._receivedValue = false;
            await this._server.BroadcastAsync(message);
            Boolean @continue = await Task.Run(() =>
            {
                Int32 timeout = 0;
                while (!this._receivedValue)
                {
                    Thread.Sleep(1);
                    timeout++;
                    if (timeout >= 15000)
                    {
                        return false;
                    }
                }
                return true;
            });
#pragma warning disable
            return !@continue
                        ? throw new TimeoutException()
                        : (TIndex?)this._lastReceivedValue;
#pragma warning restore
        }

        public override async Task InvokeIndexerSetterAsync<TIndex>(IndexerSignature indexer,
                                                                    [AllowNull] TIndex value,
                                                                    [DisallowNull] params Object[] indecies)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }
            if (!this.IsActive)
            {
                throw new InvalidOperationException(CONTROLLER_IS_NOT_ACTIVE);
            }
            if (indexer.Equals(default))
            {
                throw new NotSupportedException(NO_INFORMATION_HAS_BEEN_SET);
            }
            if (indecies is null)
            {
                throw new ArgumentNullException(nameof(indecies));
            }
            if (!indexer.HasSetter)
            {
                throw new NotSupportedException(INDEXER_HAS_NO_SETTER);
            }
            if (!typeof(TIndex).IsAssignableTo(indexer.Type))
            {
                throw new InvalidCastException(String.Format(TYPE_INCOMPATIBLE,
                                                             typeof(TIndex).FullName,
                                                             indexer.Type.FullName));
            }
            if (indexer.Indecies.Count != indecies.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(indecies));
            }
            for (Int32 i = 0; i < indecies.Length; i++)
            {
                if (!indecies[i].GetType()
                                .IsAssignableTo(indexer.Indecies[i]))
                {
                    throw new InvalidCastException(String.Format(PARAMETER_INCOMPATIBLE,
                                                                 i,
                                                                 indecies[i].GetType().FullName,
                                                                 indexer.Indecies[i].FullName));
                }
            }

            __ByteMessage message = __ByteMessage.InvokeIndexSetter(indexer,
                                                                value,
                                                                indecies);
            await this._server.BroadcastAsync(message);
        }
    }

    // IDisposable
    partial class __IsolatedAddInController : IDisposable
    {
        public override void Dispose()
        {
            base.Dispose();
            this._server.Dispose();
        }
    }
}
