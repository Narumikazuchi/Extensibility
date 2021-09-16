using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Narumikazuchi.Extensibility
{
    internal sealed partial class __TrustedAddInController
    {
        internal __TrustedAddInController(AddInDefinition definition,
                                          IStoreEvents store) :
            base(definition,
                 store)
        { }

        private static Object? LoadDefault(Type type)
        {
            ConstructorInfo? ctor = type.GetConstructors()
                                        .FirstOrDefault(c => c.IsPublic &&
                                                             c.GetParameters().Length == 0);
            return ctor?.Invoke(Array.Empty<Object>());
        }

        private static Object? LoadSingleton(Type type)
        {
            Type singleton = typeof(Singleton<>).MakeGenericType(type);
            PropertyInfo? property = singleton.GetProperty("Instance",
                                                           BindingFlags.Public | BindingFlags.Static);
            return property?.GetValue(null);
        }

        private void FetchExposedMembers(Object addIn)
        {
            this._active = true;
            this._addIn = addIn;

            foreach (MethodInfo methodInfo in addIn.GetType()
                                                   .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                   .Where(m => 
                                                        AttributeResolver.HasAttribute<AddInExposedAttribute>(m)))
            {
                if (!AreParametersValid(methodInfo.GetParameters()) ||
                    !IsTypeTransferrable(methodInfo.ReturnType, 
                                         true))
                {
                    continue;
                }
                MethodSignature method = new(methodInfo);
                this._methods.Add(method);
            }

            foreach (PropertyInfo propertyInfo in addIn.GetType()
                                                       .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                       .Where(p => 
                                                            AttributeResolver.HasAttribute<AddInExposedAttribute>(p)))
            {
                if (!IsTypeTransferrable(propertyInfo.PropertyType, 
                                         false))
                {
                    continue;
                }
                if (propertyInfo.GetIndexParameters().Length > 0)
                {
                    if (!AreParametersValid(propertyInfo.GetIndexParameters()))
                    {
                        continue;
                    }
                    IndexerSignature indexer = new(propertyInfo);
                    this._indexers.Add(indexer);
                    continue;
                }
                PropertySignature property = new(propertyInfo);
                this._properties.Add(property);
            }
        }

        private async Task<TReturn> InvokeMethodInternal<TReturn>(MethodSignature method,
                                                                   Object[] parameters)
        {
            if (method.Equals(default))
            {
                throw new NotSupportedException(NO_INFORMATION_HAS_BEEN_SET);
            }
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (method.ReturnType != typeof(void) &&
                !typeof(TReturn).IsAssignableTo(method.ReturnType))
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

            if (this._addIn is null)
            {
                throw new NullReferenceException(ADDIN_NOT_REFERENCED);
            }
            MethodInfo? info = await Task.Run(() => this._addIn.GetType()
                                                               .GetMethod(method.Name,
                                                                          BindingFlags.Public | BindingFlags.Instance,
                                                                          null,
                                                                          method.Parameters.ToArray(),
                                                                          null));
            if (info is null)
            {
                throw new MissingMemberException(this._addIn.GetType().FullName,
                                                 method.Name);
            }

#pragma warning disable
            try
            {
                if (method.ReturnType != typeof(void))
                {
                    return await Task.Run(() => (TReturn?)info.Invoke(this._addIn,
                                                                      parameters));
                }
                await Task.Run(() => info.Invoke(this._addIn,
                                                 parameters));
            }
            catch (Exception e)
            {
                List<String> stackTrace = new()
                {
                    e.StackTrace
                };
                while (e.InnerException is not null)
                {
                    e = e.InnerException;
                    stackTrace.Add(e.StackTrace);
                }
                this._store.OnAddInCrashed(this,
                                           stackTrace);
            }
            return default;
#pragma warning restore
        }

        private Object? _addIn = null;

#pragma warning disable
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String ADDIN_NOT_REFERENCED = "The AddIn object has not been referenced.";
#pragma warning restore
    }

    // IAddInController
    partial class __TrustedAddInController : __AddInControllerBase
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
            if (!this._process.Start())
            {
                return false;
            }
            Type? type = await this._definition.FindAddInType();
            if (type is null)
            {
                throw new TypeAccessException();
            }

            if (type.IsSingleton())
            {
                Object? singleton = LoadSingleton(
                                        type);
                if (singleton is null)
                {
                    throw new NullReferenceException();
                }
                this.FetchExposedMembers(singleton);
                this._store.OnActivated(
                                this);
                return true;
            }

            Object? result = LoadDefault(type);
            if (result is null)
            {
                throw new NullReferenceException();
            }
            this.FetchExposedMembers(result);
            this._store.OnActivated(this);
            return true;
        }

        public override void Shutdown()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
            }

            if (this._addIn is null)
            {
                return;
            }

            this._store.OnShuttingDown(this);
            this._process.Kill();
            if (this._addIn is IDisposable disposable)
            {
                disposable.Dispose();
            }
            this._addIn = null;
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

            await this.InvokeMethodInternal<Object>(method,
                                                    parameters);
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

            return await this.InvokeMethodInternal<TReturn>(method,
                                                            parameters);
        }

        public override async Task<TProperty> InvokePropertyGetterAsync<TProperty>(PropertySignature property)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
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
            if (this._addIn is null)
            {
                throw new NullReferenceException(ADDIN_NOT_REFERENCED);
            }

            PropertyInfo? info = await Task.Run(() => this._addIn.GetType()
                                                                 .GetProperty(property.Name,
                                                                              BindingFlags.Public | BindingFlags.Instance));
            if (info is null)
            {
                throw new MissingMemberException(this._addIn.GetType().FullName, 
                                                 property.Name);
            }

#pragma warning disable
            try
            {
                return await Task.Run(() => (TProperty?)info.GetValue(this._addIn));
            }
            catch (Exception e)
            {
                List<String> stackTrace = new()
                {
                    e.StackTrace
                };
                while (e.InnerException is not null)
                {
                    e = e.InnerException;
                    stackTrace.Add(e.StackTrace);
                }
                this._store.OnAddInCrashed(this,
                                           stackTrace);
            }
            return default;
#pragma warning restore
        }

        public override async Task InvokePropertySetterAsync<TProperty>(PropertySignature property,
                                                                        [AllowNull] TProperty value)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(__TrustedAddInController));
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
            if (this._addIn is null)
            {
                throw new NullReferenceException(ADDIN_NOT_REFERENCED);
            }

            PropertyInfo? info = await Task.Run(() => this._addIn.GetType()
                                                                 .GetProperty(property.Name,
                                                                              BindingFlags.Public | BindingFlags.Instance));
            if (info is null)
            {
                throw new MissingMemberException(this._addIn.GetType().FullName,
                                                 property.Name);
            }

#pragma warning disable
            try
            {
                info.SetValue(this._addIn,
                              value);
            }
            catch (Exception e)
            {
                List<String> stackTrace = new()
                {
                    e.StackTrace
                };
                while (e.InnerException is not null)
                {
                    e = e.InnerException;
                    stackTrace.Add(e.StackTrace);
                }
                this._store.OnAddInCrashed(this,
                                           stackTrace);
            }
#pragma warning restore
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
            if (this._addIn is null)
            {
                throw new NullReferenceException(ADDIN_NOT_REFERENCED);
            }

            PropertyInfo? info = await Task.Run(() => this._addIn.GetType()
                                                                 .GetProperty(indexer.Name,
                                                                              BindingFlags.Public | BindingFlags.Instance,
                                                                              null,
                                                                              indexer.Type,
                                                                              indexer.Indecies.ToArray(),
                                                                              null));
            if (info is null)
            {
                throw new MissingMemberException(this._addIn.GetType().FullName,
                                                 indexer.Name);
            }
#pragma warning disable
            try
            {
                return await Task.Run(() => (TIndex?)info.GetValue(this._addIn,
                                                                   indecies));
            }
            catch (Exception e)
            {
                List<String> stackTrace = new()
                {
                    e.StackTrace
                };
                while (e.InnerException is not null)
                {
                    e = e.InnerException;
                    stackTrace.Add(e.StackTrace);
                }
                this._store.OnAddInCrashed(this,
                                           stackTrace);
            }
            return default;
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
            if (this._addIn is null)
            {
                throw new NullReferenceException(ADDIN_NOT_REFERENCED);
            }

            PropertyInfo? info = await Task.Run(() => this._addIn.GetType()
                                                                 .GetProperty(indexer.Name,
                                                                              BindingFlags.Public | BindingFlags.Instance));
            if (info is null)
            {
                throw new MissingMemberException(this._addIn.GetType().FullName,
                                                 indexer.Name);
            }

#pragma warning disable
            try
            {
                info.SetValue(this._addIn,
                              value,
                              indecies);
            }
            catch (Exception e)
            {
                List<String> stackTrace = new()
                {
                    e.StackTrace
                };
                while (e.InnerException is not null)
                {
                    e = e.InnerException;
                    stackTrace.Add(e.StackTrace);
                }
                this._store.OnAddInCrashed(this,
                                           stackTrace);
            }
#pragma warning restore
        }
    }
}
