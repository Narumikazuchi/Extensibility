using Narumikazuchi.Serialization.Bytes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Narumikazuchi.Extensibility
{
    internal abstract partial class __AddInControllerBase
    {
        protected __AddInControllerBase(AddInDefinition definition,
                                        IStoreEvents store)
        {
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            this._definition = definition;
            this._store = store;
            this._process = new(definition,
                                store);
        }

        protected static Boolean AreParametersValid(IEnumerable<ParameterInfo> parameters)
        {
            foreach (ParameterInfo parameter in parameters)
            {
                if (!IsTypeTransferrable(parameter.ParameterType,
                                         false))
                {
                    return false;
                }
            }
            return true;
        }

        protected static Boolean IsTypeTransferrable(Type type,
                                                     Boolean method) =>
            type.IsPrimitive ||
            type == typeof(String) ||
            type.IsAssignableTo(typeof(IByteSerializable)) ||
            (method &&
            type == typeof(void));

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected readonly AddInDefinition _definition;
        protected readonly IStoreEvents _store;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected readonly AddInProcess _process;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected readonly List<MethodSignature> _methods = new();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected readonly List<PropertySignature> _properties = new();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected readonly List<IndexerSignature> _indexers = new();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected Boolean _active = false;
        protected Boolean _disposed = false;

#pragma warning disable
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String CONTROLLER_IS_NOT_ACTIVE = "The controller has not activated the AddIn yet.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String METHOD_SIGNATURE_NOT_FOUND = "A method signature with the name '{0}' and the parameters '{1}' could not be found.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String PROPERTY_SIGNATURE_NOT_FOUND = "A property signature with the name '{0}' could not be found.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String INDEXER_SIGNATURE_NOT_FOUND = "A indexer signature with the parameters '{0}' could not be found.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String PARAMETER_INCOMPATIBLE = "The parameter at position '{0}' of type '{1}' was incompatible to the expected type '{1}'.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String TYPE_INCOMPATIBLE = "The member type '{0}' was incompatible to the expected type '{1}'.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String PROPERTY_HAS_NO_GETTER = "The property has no accessible getter.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String PROPERTY_HAS_NO_SETTER = "The property has no accessible setter.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String INDEXER_HAS_NO_GETTER = "The indexer has no accessible getter.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String INDEXER_HAS_NO_SETTER = "The indexer has no accessible setter.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected const String NO_INFORMATION_HAS_BEEN_SET = "The signature does not contain any information.";
#pragma warning restore
    }

    // IAddInController
    partial class __AddInControllerBase : IAddInController
    {
        public abstract Task<Boolean> ActivateAsync();

        public abstract void Shutdown();

        public abstract Task InvokeMethodAsync(MethodSignature method,
                                               [DisallowNull] params Object[] parameters);
        public abstract Task<TReturn> InvokeMethodAsync<TReturn>(MethodSignature method,
                                                                 [DisallowNull] params Object[] parameters);

        public abstract Task<TProperty> InvokePropertyGetterAsync<TProperty>(PropertySignature property);

        public abstract Task InvokePropertySetterAsync<TProperty>(PropertySignature property,
                                                                  [AllowNull] TProperty value);

        public abstract Task<TIndex> InvokeIndexerGetterAsync<TIndex>(IndexerSignature indexer,
                                                                      [DisallowNull] params Object[] indecies);

        public abstract Task InvokeIndexerSetterAsync<TIndex>(IndexerSignature indexer,
                                                              [AllowNull] TIndex value,
                                                              [DisallowNull] params Object[] indecies);

        public virtual MethodSignature GetMethod([DisallowNull] String name,
                                                 [DisallowNull] params Type[] parameters)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            MethodSignature method = this._methods.FirstOrDefault(m => m.Name == name &&
                                                                         m.Parameters.SequenceEqual(parameters));

            return method.Equals(default)
                    ? throw new KeyNotFoundException(String.Format(METHOD_SIGNATURE_NOT_FOUND,
                                                                   name,
                                                                   String.Join(',', parameters.Select(p => p.FullName))))
                    : method;
        }

        public virtual PropertySignature GetProperty([DisallowNull] String name)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            PropertySignature property = this._properties.FirstOrDefault(p => p.Name == name);

            return property.Equals(default)
                    ? throw new KeyNotFoundException(String.Format(PROPERTY_SIGNATURE_NOT_FOUND,
                                                                   name))
                    : property;
        }

        public virtual IndexerSignature GetIndexer([DisallowNull] params Type[] indecies)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }
            if (indecies is null)
            {
                throw new ArgumentNullException(nameof(indecies));
            }

            IndexerSignature indexer = this._indexers.FirstOrDefault(i => i.Indecies.SequenceEqual(indecies));
            return indexer.Equals(default)
                    ? throw new KeyNotFoundException(String.Format(INDEXER_SIGNATURE_NOT_FOUND,
                                                                   String.Join(',', indecies.Select(p => p.FullName))))
                    : indexer;
        }

        public IReadOnlyList<MethodSignature> Methods =>
            this._methods;
        public IReadOnlyList<PropertySignature> Properties =>
            this._properties;
        public IReadOnlyList<IndexerSignature> Indexers =>
            this._indexers;
        public Boolean IsActive =>
            this._disposed
                ? throw new ObjectDisposedException(nameof(__TrustedAddInController))
                : this._active;
        public AddInDefinition Definition =>
            this._disposed
                ? throw new ObjectDisposedException(nameof(__TrustedAddInController))
                : this._definition;
        public AddInProcess Process
        {
            get
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(nameof(__TrustedAddInController));
                }
                if (!this._active ||
                    this._process is null)
                {
                    throw new InvalidOperationException(CONTROLLER_IS_NOT_ACTIVE);
                }
                return this._process;
            }
        }
    }

    // IDisposable
    partial class __AddInControllerBase : IDisposable
    {
        public virtual void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            if (this.IsActive)
            {
                this.Shutdown();
            }
            this._process.Dispose();
            this._disposed = true;
        }
    }
}
