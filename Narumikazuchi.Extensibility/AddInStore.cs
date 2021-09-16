using Narumikazuchi.Collections.Abstract;
using Narumikazuchi.Serialization.Bytes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// This class is the core of the AddIn functionality. It caches known AddIns and generates <see cref="IAddInController"/> objects to control the AddIns.
    /// </summary>
    /// <remarks>
    /// This class is a <see cref="Singleton"/> and can therefore only be accessed through the <see cref="Singleton{TClass}.Instance"/> property.
    /// </remarks>
    public sealed partial class AddInStore<TAddIn> : Singleton
        where TAddIn : class
    {
        /// <summary>
        /// Registers all AddIns in the specified file in the default cache.
        /// </summary>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] String assemblyFile) =>
            String.IsNullOrWhiteSpace(assemblyFile)
                ? throw new ArgumentNullException(nameof(assemblyFile))
                : this.Register(this._defaultCache,
                                new FileInfo(assemblyFile),
                                AddInSecurity.Isolated);
        /// <summary>
        /// Registers all AddIns in the specified file in the default cache with the specified security level.
        /// </summary>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <param name="security">The security level for registering and loading the AddIns.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] String assemblyFile,
                                in AddInSecurity security) =>
            String.IsNullOrWhiteSpace(assemblyFile)
                    ? throw new ArgumentNullException(nameof(assemblyFile))
                    : this.Register(this._defaultCache,
                                    new FileInfo(assemblyFile),
                                    security);
        /// <summary>
        /// Registers all AddIns in the specified file in the default cache.
        /// </summary>
        /// <param name="assemblyFile">The file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] FileInfo assemblyFile) =>
            this.Register(this._defaultCache,
                          assemblyFile,
                          AddInSecurity.Isolated);
        /// <summary>
        /// Registers all AddIns in the specified file in the cache with the specified security level.
        /// </summary>
        /// <param name="assemblyFile">The file to acquire the AddIns from.</param>
        /// <param name="security">The security level for registering and loading the AddIns.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] FileInfo assemblyFile,
                                in AddInSecurity security) =>
            this.Register(this._defaultCache,
                          assemblyFile,
                          security);
        /// <summary>
        /// Registers all AddIns in the specified file in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] String cacheFile,
                                [DisallowNull] String assemblyFile)
        {
            if (String.IsNullOrWhiteSpace(cacheFile))
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (String.IsNullOrWhiteSpace(assemblyFile))
            {
                throw new ArgumentNullException(nameof(assemblyFile));
            }
            return this.Register(new FileInfo(cacheFile),
                                 new FileInfo(assemblyFile),
                                 AddInSecurity.Isolated);
        }
        /// <summary>
        /// Registers all AddIns in the specified file in the specified cache with the specified security level.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <param name="security">The security level for registering and loading the AddIns.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] String cacheFile,
                                [DisallowNull] String assemblyFile,
                                in AddInSecurity security)
        {
            if (String.IsNullOrWhiteSpace(cacheFile))
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (String.IsNullOrWhiteSpace(assemblyFile))
            {
                throw new ArgumentNullException(nameof(assemblyFile));
            }
            return this.Register(new FileInfo(cacheFile),
                                 new FileInfo(assemblyFile),
                                 security);
        }
        /// <summary>
        /// Registers all AddIns in the specified file in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] FileInfo cacheFile,
                                [DisallowNull] String assemblyFile) =>
            String.IsNullOrWhiteSpace(assemblyFile)
                ? throw new ArgumentNullException(nameof(assemblyFile))
                : this.Register(cacheFile,
                                new FileInfo(assemblyFile),
                                AddInSecurity.Isolated);
        /// <summary>
        /// Registers all AddIns in the specified file in the specified cache with the specified security level.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <param name="security">The security level for registering and loading the AddIns.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] FileInfo cacheFile,
                                [DisallowNull] String assemblyFile,
                                in AddInSecurity security) =>
            String.IsNullOrWhiteSpace(assemblyFile)
                ? throw new ArgumentNullException(nameof(assemblyFile))
                : this.Register(cacheFile,
                                new FileInfo(assemblyFile),
                                security);
        /// <summary>
        /// Registers all AddIns in the specified file in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] String cacheFile,
                                [DisallowNull] FileInfo assemblyFile) =>
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.Register(new FileInfo(cacheFile),
                                assemblyFile,
                                AddInSecurity.Isolated);
        /// <summary>
        /// Registers all AddIns in the specified file in the specified cache with the specified security level.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="assemblyFile">The file to acquire the AddIns from.</param>
        /// <param name="security">The security level for registering and loading the AddIns.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] String cacheFile,
                                [DisallowNull] FileInfo assemblyFile,
                                in AddInSecurity security) =>
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.Register(new FileInfo(cacheFile),
                                assemblyFile,
                                security);
        /// <summary>
        /// Registers all AddIns in the specified file in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="assemblyFile">The file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] FileInfo cacheFile,
                                [DisallowNull] FileInfo assemblyFile) => 
            this.Register(cacheFile,
                          assemblyFile,
                          AddInSecurity.Isolated);
        /// <summary>
        /// Registers all AddIns in the specified file in the specified cache with the specified security level.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="assemblyFile">The file to acquire the AddIns from.</param>
        /// <param name="security">The security level for registering and loading the AddIns.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] FileInfo cacheFile,
                                [DisallowNull] FileInfo assemblyFile,
                                in AddInSecurity security)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (assemblyFile is null)
            {
                throw new ArgumentNullException(nameof(assemblyFile));
            }
            if (!assemblyFile.Exists)
            {
                throw new IOException(String.Format(FILE_DOES_NOT_EXIST,
                                                    assemblyFile.FullName));
            }
            return this.RegisterInternal(cacheFile,
                                         assemblyFile,
                                         AddInSecurity.Isolated);
        }

        /// <summary>
        /// Registers the specified type in the default cache.
        /// </summary>
        /// <param name="addInType">The type to register.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] Type addInType) =>
            this.Register(this._defaultCache,
                          addInType,
                          AddInSecurity.Trusted);
        /// <summary>
        /// Registers the specified type in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="addInType">The type to register.</param>
        /// <param name="security">The security level for registering and loading the AddIn.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] String cacheFile,
                                [DisallowNull] Type addInType,
                                in AddInSecurity security) => 
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.Register(new FileInfo(cacheFile),
                                addInType,
                                security);
        /// <summary>
        /// Registers the specified type in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="addInType">The type to register.</param>
        /// <param name="security">The security level for registering and loading the AddIn.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] FileInfo cacheFile,
                                [DisallowNull] Type addInType,
                                in AddInSecurity security)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (addInType is null)
            {
                throw new ArgumentNullException(nameof(addInType));
            }
            if (!EnumValidator.IsDefined(security))
            {
                throw new ArgumentOutOfRangeException(nameof(security));
            }
            if (!addInType.IsAssignableTo(typeof(TAddIn)))
            {
                throw new InvalidCastException(String.Format(ADDIN_TYPE_DOES_NOT_INHERIT_STORE_TYPEPARAM,
                                                             addInType.FullName,
                                                             typeof(TAddIn).FullName));
            }

            AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(addInType);
            AddInDefinition definition = new(attribute.Guid,
                                             attribute.Name,
                                             attribute.Version,
                                             addInType,
                                             security);
            return this.RegisterInternal(cacheFile,
                                         definition);
        }

        /// <summary>
        /// Registers the specified AddIn in the default cache.
        /// </summary>
        /// <param name="definition">The AddIn to register.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] AddInDefinition definition) =>
            this.Register(this._defaultCache,
                          definition);
        /// <summary>
        /// Registers the specified AddIn in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="definition">The AddIn to register.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] String cacheFile,
                                [DisallowNull] AddInDefinition definition) => 
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.RegisterInternal(new FileInfo(cacheFile),
                                        definition);
        /// <summary>
        /// Registers the specified AddIn in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="definition">The AddIn to register.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidDataException"/>
        public Boolean Register([DisallowNull] FileInfo cacheFile,
                                [DisallowNull] AddInDefinition definition)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            return this.RegisterInternal(cacheFile,
                                         definition);
        }

        /// <summary>
        /// Unregisters all AddIns in the specified file from the default cache.
        /// </summary>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] String assemblyFile) =>
            String.IsNullOrWhiteSpace(assemblyFile)
                ? throw new ArgumentNullException(nameof(assemblyFile))
                : this.Unregister(this._defaultCache,
                                  new FileInfo(assemblyFile));
        /// <summary>
        /// Unregisters all AddIns in the specified file from the default cache.
        /// </summary>
        /// <param name="assemblyFile">The file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] FileInfo assemblyFile) =>
            this.Unregister(this._defaultCache,
                            assemblyFile);
        /// <summary>
        /// Unregisters all AddIns in the specified file from the specified cache.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] String cacheFile,
                                  [DisallowNull] String assemblyFile)
        {
            if (String.IsNullOrWhiteSpace(cacheFile))
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (String.IsNullOrWhiteSpace(assemblyFile))
            {
                throw new ArgumentNullException(nameof(assemblyFile));
            }
            return this.Unregister(new FileInfo(cacheFile),
                                   new FileInfo(assemblyFile));
        }
        /// <summary>
        /// Unregisters all AddIns in the specified file from the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="assemblyFile">The path to the file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] FileInfo cacheFile,
                                  [DisallowNull] String assemblyFile) =>
            String.IsNullOrWhiteSpace(assemblyFile)
                ? throw new ArgumentNullException(nameof(assemblyFile))
                : this.Unregister(cacheFile,
                                  new FileInfo(assemblyFile));
        /// <summary>
        /// Unregisters all AddIns in the specified file from the specified cache.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="assemblyFile">The file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] String cacheFile,
                                  [DisallowNull] FileInfo assemblyFile) =>
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.Unregister(new FileInfo(cacheFile),
                                  assemblyFile);
        /// <summary>
        /// Unregisters all AddIns in the specified file from the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="assemblyFile">The file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] FileInfo cacheFile,
                                  [DisallowNull] FileInfo assemblyFile)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (assemblyFile is null)
            {
                throw new ArgumentNullException(nameof(assemblyFile));
            }
            if (!assemblyFile.Exists)
            {
                throw new IOException(String.Format(FILE_DOES_NOT_EXIST,
                                                    assemblyFile.FullName));
            }
            return this.UnregisterInternal(cacheFile,
                                           assemblyFile);
        }

        /// <summary>
        /// Unregisters the specified type in the specified cache.
        /// </summary>
        /// <param name="addInType">The type to unregister.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] Type addInType) =>
            this.Unregister(this._defaultCache,
                            addInType);
        /// <summary>
        /// Unregisters the specified type in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="addInType">The type to unregister.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] String cacheFile,
                                  [DisallowNull] Type addInType) => 
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.Unregister(new FileInfo(cacheFile),
                                  addInType);
        /// <summary>
        /// Unregisters the specified type in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="addInType">The path to the file to acquire the AddIns from.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] FileInfo cacheFile,
                                  [DisallowNull] Type addInType)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (addInType is null)
            {
                throw new ArgumentNullException(nameof(addInType));
            }
            if (!addInType.IsAssignableTo(typeof(TAddIn)))
            {
                throw new InvalidCastException(String.Format(ADDIN_TYPE_DOES_NOT_INHERIT_STORE_TYPEPARAM,
                                                             addInType.FullName,
                                                             typeof(TAddIn).FullName));
            }

            AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(addInType);
            AddInDefinition definition = new(attribute.Guid,
                                             attribute.Name,
                                             attribute.Version,
                                             addInType);
            return this.UnregisterInternal(cacheFile,
                                           definition);
        }

        /// <summary>
        /// Unregisters the specified AddIn in the specified cache.
        /// </summary>
        /// <param name="definition">The AddIn to unregister.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] AddInDefinition definition) =>
            this.Unregister(this._defaultCache,
                            definition);
        /// <summary>
        /// Unregisters the specified AddIn in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to store the information into.</param>
        /// <param name="definition">The AddIn to unregister.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] String cacheFile,
                                  [DisallowNull] AddInDefinition definition) => 
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.Unregister(new FileInfo(cacheFile),
                                  definition);
        /// <summary>
        /// Unregisters the specified AddIn in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to store the information into.</param>
        /// <param name="definition">The AddIn to unregister.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean Unregister([DisallowNull] FileInfo cacheFile,
                                  [DisallowNull] AddInDefinition definition)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            return this.UnregisterInternal(cacheFile,
                                           definition);
        }

        /// <summary>
        /// Gets whether the specified AddIn is currently registered in any cache.
        /// </summary>
        /// <param name="definition">The AddIn to search for.</param>
        /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
        public Boolean IsAddInRegistered([DisallowNull] AddInDefinition definition) =>
            definition is null
                ? throw new ArgumentNullException(nameof(definition))
                : this._register.ContainsKey(definition.Guid);
        /// <summary>
        /// Gets whether the specified AddIn is currently registered in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to search through.</param>
        /// <param name="definition">The AddIn to search for.</param>
        /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
        public Boolean IsAddInRegistered([DisallowNull] FileInfo cacheFile,
                                         [DisallowNull] AddInDefinition definition)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            if (!this._cache.ContainsKey(cacheFile.FullName))
            {
                throw new KeyNotFoundException(String.Format(CACHE_HAS_NOT_BEEN_LOADED,
                                                             cacheFile.FullName));
            }
            return this._cache[cacheFile.FullName].Contains(definition);
        }
        /// <summary>
        /// Gets whether the AddIn with the specified GUID is currently registered in any cache.
        /// </summary>
        /// <param name="guid">The GUID of the AddIn to search for.</param>
        /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
        public Boolean IsAddInRegistered(in Guid guid) =>
            this._register.ContainsKey(guid);
        /// <summary>
        /// Gets whether the AddIn with the specified GUID is currently registered in the specified cache.
        /// </summary>
        /// <param name="cacheFile">The cache file to search through.</param>
        /// <param name="guid">The GUID of the AddIn to search for.</param>
        /// <returns><see langword="true"/> if the AddIn is registered; otherwise, <see langword="false"/></returns>
        public Boolean IsAddInRegistered([DisallowNull] FileInfo cacheFile,
                                         Guid guid)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (!this._cache.ContainsKey(cacheFile.FullName))
            {
                throw new KeyNotFoundException(String.Format(CACHE_HAS_NOT_BEEN_LOADED,
                                                             cacheFile.FullName));
            }
            return this._cache[cacheFile.FullName].Any(d => d.Guid == guid);
        }

        /// <summary>
        /// Sets up the isolation settings for the store for AddIns that run in the <see cref="AddInSecurity.Isolated"/> security mode.
        /// </summary>
        /// <param name="settings">The settings for the <see cref="AddInSecurity.Isolated"/> security mode. If <see langword="null"/> the defualt will be used.</param>
        /// <exception cref="InvalidOperationException"/>
        public void SetupIsolation(IsolationSettings? settings)
        {
            if (this.ActiveAddIns.Any())
            {
                throw new InvalidOperationException(CANNOT_CHANGE_ISOLATION_SETTINGS_WHILE_ADDINS_ARE_ACTIVE);
            }
            if (settings is null)
            {
                this._isolationSettings = IsolationSettings.Default;
                return;
            }
            this._isolationSettings = settings;
        }

        /// <summary>
        /// Clears the default cache in memory.
        /// </summary>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        public Boolean ClearCache() =>
            this.ClearCache(this._defaultCache);
        /// <summary>
        /// Clears the specified cache in memory.
        /// </summary>
        /// <param name="cacheFile">The filepath to clear.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean ClearCache([DisallowNull] String cacheFile) =>
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.ClearCache(new FileInfo(cacheFile));
        /// <summary>
        /// Clears the specified cache in memory.
        /// </summary>
        /// <param name="cacheFile">The file to clear.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean ClearCache([DisallowNull] FileInfo cacheFile)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }

            if (!this._cache.ContainsKey(cacheFile.FullName))
            {
                return false;
            }

            this._cache[cacheFile.FullName].Clear();
            return true;
        }

        /// <summary>
        /// Loads the default cache into memory.
        /// </summary>
        /// <returns>All known AddIn from the cache</returns>
        public IEnumerable<AddInDefinition> LoadCache() =>
            this.LoadCache(this._defaultCache);
        /// <summary>
        /// Loads the specified cache into memory.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to load.</param>
        /// <returns>All known AddIn from the cache</returns>
        /// <exception cref="ArgumentNullException"/>
        public IEnumerable<AddInDefinition> LoadCache([DisallowNull] String cacheFile) =>
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.LoadCache(new FileInfo(cacheFile));
        /// <summary>
        /// Loads the specified cache into memory.
        /// </summary>
        /// <param name="cacheFile">The cache file to load.</param>
        /// <returns>All known AddIn from the cache</returns>
        /// <exception cref="ArgumentNullException"/>
        public IEnumerable<AddInDefinition> LoadCache([DisallowNull] FileInfo cacheFile)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }

            if (this._cache.ContainsKey(cacheFile.FullName))
            {
                return this.UpdateCache(cacheFile);
            }

            List<AddInDefinition> list = AddInStore<TAddIn>.ReadCacheFile(cacheFile);
            List<AddInDefinition> result = new();
            foreach (AddInDefinition definition in list)
            {
                if (this._register.ContainsKey(definition.Guid))
                {
                    continue;
                }
                if (definition.Security is AddInSecurity.Trusted)
                {
                    this._register.Add(definition.Guid,
                                       new __TrustedAddInController(definition,
                                                                  this));
                }
                else
                {
                    this._register.Add(definition.Guid,
                                       new __IsolatedAddInController(definition,
                                                                   this));
                }
                result.Add(definition);
                this.CachedAddIns.Add(definition);
            }

            this._cache.Add(cacheFile.FullName,
                            result);
            return result;
        }

        /// <summary>
        /// Reloads the default cache from the disk.
        /// </summary>
        /// <returns>All known AddIn from the cache</returns>
        public IEnumerable<AddInDefinition> UpdateCache() =>
            this.UpdateCache(this._defaultCache);
        /// <summary>
        /// Reloads the specified cache from the disk.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file to load.</param>
        /// <returns>All known AddIn from the cache</returns>
        /// <exception cref="ArgumentNullException"/>
        public IEnumerable<AddInDefinition> UpdateCache([DisallowNull] String cacheFile) =>
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.UpdateCache(new FileInfo(cacheFile));
        /// <summary>
        /// Reloads the specified cache from the disk.
        /// </summary>
        /// <param name="cacheFile">The cache file to load.</param>
        /// <returns>All known AddIn from the cache</returns>
        /// <exception cref="ArgumentNullException"/>
        public IEnumerable<AddInDefinition> UpdateCache([DisallowNull] FileInfo cacheFile)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }
            if (!this._cache.ContainsKey(cacheFile.FullName))
            {
                return this.LoadCache(cacheFile);
            }

            List<AddInDefinition> list = AddInStore<TAddIn>.ReadCacheFile(cacheFile);

            IEnumerable<AddInDefinition> identical = list.Where(d => this._cache[cacheFile.FullName].Contains(d));
            IEnumerable<AddInDefinition> old = this._cache[cacheFile.FullName].Except(identical);

            foreach (AddInDefinition definition in old)
            {
                if (!this._register.ContainsKey(definition.Guid))
                {
                    continue;
                }
                if (this._register[definition.Guid].IsActive)
                {
                    this._register[definition.Guid].Dispose();
                }
                this._register.Remove(definition.Guid);
                this.CachedAddIns.Remove(definition);
            }

            IEnumerable<AddInDefinition> updated = list.Except(identical);

            foreach (AddInDefinition definition in updated)
            {
                if (definition.Security is AddInSecurity.Trusted)
                {
                    this._register.Add(definition.Guid,
                                       new __TrustedAddInController(definition,
                                                                  this));
                }
                else
                {
                    this._register.Add(definition.Guid,
                                       new __IsolatedAddInController(definition,
                                                                   this));
                }
                this.CachedAddIns.Add(definition);
            }

            return updated;
        }

        /// <summary>
        /// Writes the default cache to disk.
        /// </summary>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        public Boolean RebuildCache() =>
            this.RebuildCache(this._defaultCache);
        /// <summary>
        /// Writes the specified cache to disk.
        /// </summary>
        /// <param name="cacheFile">The path to write to.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean RebuildCache([DisallowNull] String cacheFile) =>
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.RebuildCache(new FileInfo(cacheFile));
        /// <summary>
        /// Writes the specified cache to disk.
        /// </summary>
        /// <param name="cacheFile">The file to write to.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean RebuildCache([DisallowNull] FileInfo cacheFile)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }

            if (!this._cache.ContainsKey(cacheFile.FullName))
            {
                return false;
            }

            AddInStore<TAddIn>.WriteCacheFile(cacheFile,
                                              this._cache[cacheFile.FullName]);
            return true;
        }

        /// <summary>
        /// Unloads the default cache from the memory.
        /// </summary>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        public Boolean UnloadCache() =>
            this.UnloadCache(this._defaultCache);
        /// <summary>
        /// Unloads the specified cache from the memory.
        /// </summary>
        /// <param name="cacheFile">The path to the file to unload.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean UnloadCache([DisallowNull] String cacheFile) =>
            String.IsNullOrWhiteSpace(cacheFile)
                ? throw new ArgumentNullException(nameof(cacheFile))
                : this.UnloadCache(new FileInfo(cacheFile));
        /// <summary>
        /// Unloads the specified cache from the memory.
        /// </summary>
        /// <param name="cacheFile">The file to unload.</param>
        /// <returns><see langword="true"/> if the action succeeded; otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public Boolean UnloadCache([DisallowNull] FileInfo cacheFile)
        {
            if (cacheFile is null)
            {
                throw new ArgumentNullException(nameof(cacheFile));
            }

            if (!this._cache.ContainsKey(cacheFile.FullName))
            {
                return false;
            }

            foreach (AddInDefinition definition in this._cache[cacheFile.FullName])
            {
                if (this._register.ContainsKey(definition.Guid))
                {
                    this._register[definition.Guid].Dispose();
                    this._register.Remove(definition.Guid);
                    this.CachedAddIns.Remove(definition);
                }
            }
            this._cache.Remove(cacheFile.FullName);
            return true;
        }

        /// <summary>
        /// Discovers all AddIns that are available in the dll files in the specified directory.
        /// </summary>
        /// <param name="directoryPath">The path to the directoy to search.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
        /// <exception cref="ArgumentNullException"/>
        public IEnumerable<AddInDefinition> Discover([DisallowNull] String directoryPath) =>
            String.IsNullOrWhiteSpace(directoryPath)
                ? throw new ArgumentNullException(nameof(directoryPath))
                : this.Discover(new DirectoryInfo(directoryPath),
                                AddInSecurity.Isolated);
        /// <summary>
        /// Discovers all AddIns that are available in the dll files in the specified directory.
        /// </summary>
        /// <param name="directoryPath">The path to the directoy to search.</param>
        /// <param name="security">The security level of this operation.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
        /// <exception cref="ArgumentNullException"/>
        public IEnumerable<AddInDefinition> Discover([DisallowNull] String directoryPath,
                                                     in AddInSecurity security) =>
            String.IsNullOrWhiteSpace(directoryPath)
                ? throw new ArgumentNullException(nameof(directoryPath))
                : this.Discover(new DirectoryInfo(directoryPath),
                                security);
        /// <summary>
        /// Discovers all AddIns that are available in the dll files in the specified directory.
        /// </summary>
        /// <param name="directory">The directoy to search.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
        /// <exception cref="ArgumentNullException"/>
        public IEnumerable<AddInDefinition> Discover([DisallowNull] DirectoryInfo directory) =>
            directory is null
                ? throw new ArgumentNullException(nameof(directory))
                : this.Discover(directory,
                                AddInSecurity.Isolated);
        /// <summary>
        /// Discovers all AddIns that are available in the dll files in the specified directory.
        /// </summary>
        /// <param name="directory">The directoy to search.</param>
        /// <param name="security">The security level of this operation.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all AddIns that could be identified</returns>
        /// <exception cref="ArgumentNullException"/>
        public IEnumerable<AddInDefinition> Discover([DisallowNull] DirectoryInfo directory,
                                                     in AddInSecurity security)
        {
            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }
            if (!directory.Exists)
            {
                throw new IOException(String.Format(DIRECTORY_DOES_NOT_EXIST,
                                                    directory.FullName));
            }
            if (!EnumValidator.IsDefined(security))
            {
                throw new ArgumentOutOfRangeException(nameof(security));
            }
            return security is AddInSecurity.Trusted
                        ? AddInStore<TAddIn>.DiscoverTrustedInternal(directory)
                        : AddInStore<TAddIn>.DiscoverIsolatedInternal(directory);
        }

        /// <summary>
        /// Gets the <see cref="IAddInController"/> for the specified AddIn.
        /// </summary>
        /// <param name="definition">The AddIn whose controller to get.</param>
        /// <returns>The <see cref="IAddInController"/> for the specified AddIn</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="KeyNotFoundException"/>
        public IAddInController GetController([DisallowNull] AddInDefinition definition)
        {
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            if (!this._register.ContainsKey(definition.Guid))
            {
                throw new KeyNotFoundException(String.Format(ADDIN_HAS_NOT_YET_BEEN_REGISTERED,
                                                             definition.Guid.ToString()));
            }
            return this._register[definition.Guid];
        }
        /// <summary>
        /// Gets the <see cref="IAddInController"/> for the AddIn with the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID of the AddIn whose controller to get.</param>
        /// <returns>The <see cref="IAddInController"/> for the specified AddIn</returns>
        /// <exception cref="KeyNotFoundException"/>
        public IAddInController GetController(Guid guid) =>
            !this._register.ContainsKey(guid)
                ? throw new KeyNotFoundException(String.Format(ADDIN_HAS_NOT_YET_BEEN_REGISTERED,
                                                               guid.ToString()))
                : this._register[guid];


        /// <summary>
        /// Gets a collection of all registered AddIns.
        /// </summary>
        [Pure]
        public AddInDefinitionCollection CachedAddIns { get; } = new();
        /// <summary>
        /// Gets a collection of all active AddIns.
        /// </summary>
        [Pure]
        public IEnumerable<IAddInController> ActiveAddIns => 
            this._register.Where(kv => kv.Value.IsActive)
                          .Select(kv => kv.Value);
        /// <summary>
        /// Gets a collection of all AddIns.
        /// </summary>
        [Pure]
        public IEnumerable<IAddInController> AllAddIns =>
            this._register.Select(kv => kv.Value);

        /// <summary>
        /// Occurs when an AddIn activation has begun.
        /// </summary>
        public event EventHandler<IAddInController>? AddInActivating;
        /// <summary>
        /// Occurs when an AddIn activation has finished.
        /// </summary>
        public event EventHandler<IAddInController>? AddInActivated;
        /// <summary>
        /// Occurs when an AddIn shutdown has begun.
        /// </summary>
        public event EventHandler<IAddInController>? AddInShuttingDown;
        /// <summary>
        /// Occurs when an AddIn shutdown has finished.
        /// </summary>
        public event EventHandler<IAddInController>? AddInShutDown;
        /// <summary>
        /// Occurs when an AddIn has crashed with a critical error.
        /// </summary>
        public event EventHandler<IAddInController, CrashEventArgs>? AddInCrashed;
    }

    // Non-Public
    partial class AddInStore<TAddIn>
    {
        private AddInStore()
        {
            this._register = new();
            this._cache = new();
            this._isolationSettings = IsolationSettings.Default;
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                foreach (KeyValuePair<Guid, IAddInController> kv in this._register)
                {
                    try
                    {
                        kv.Value.Process.Kill();
                        kv.Value.Dispose();
                    }
                    catch (ObjectDisposedException) { }
                    catch (InvalidOperationException) { }
                }
            };
        }

        private Boolean RegisterInternal(FileInfo cacheFile,
                                         FileInfo assemblyFile,
                                         in AddInSecurity security)
        {
            if (!EnumValidator.IsDefined(security))
            {
                throw new ArgumentOutOfRangeException(nameof(security));
            }

            if (security is AddInSecurity.Trusted)
            {
                return this.RegisterInternalTrusted(cacheFile,
                                                    assemblyFile);
            }
            return this.RegisterInternalIsolated(cacheFile,
                                                 assemblyFile);
        }
        private Boolean RegisterInternalIsolated(FileInfo cacheFile,
                                                 FileInfo assemblyFile)
        {
            if (!this._isolationSettings.IsolationDirectory.Exists)
            {
                this._isolationSettings.IsolationDirectory.Create();
            }
            String proc = Path.Combine(this._isolationSettings.IsolationDirectory.FullName,
                                       "Register.exe");
            File.WriteAllBytes(proc,
                               Properties.Resources.ProcessWrapper);

            ProcessStartInfo psi = new()
            {
                Arguments = "-d \"" + assemblyFile.FullName + "\"",
                CreateNoWindow = true,
                FileName = proc,
                RedirectStandardOutput = true
            };
            Process? process = Process.Start(psi);
            if (process is null)
            {
                return false;
            }
            Boolean result = false;
            while (!process.StandardOutput.EndOfStream)
            {
                String? line = process.StandardOutput.ReadLine();
                if (String.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                String[] tokens = line.Split('|');
                if (tokens.Length < 6)
                {
                    continue;
                }
                AddInDefinition definition = new(Guid.Parse(tokens[0]),
                                                 tokens[1],
                                                 Version.Parse(tokens[2]),
                                                 tokens[3],
                                                 tokens[4],
                                                 tokens[5],
                                                 AddInSecurity.Isolated);
                result |= this.RegisterInternal(cacheFile,
                                                definition);
            }
            process.WaitForExit();
            return result;
        }
        private Boolean RegisterInternalTrusted(FileInfo cacheFile,
                                                FileInfo assemblyFile)
        {
            Assembly? assembly = __AssemblyHelper.GetAssembly(assemblyFile);
            if (assembly is null)
            {
                return false;
            }

            Boolean result = false;
            foreach (AddInDefinition definition in AddInStore<TAddIn>.EnumerateAddIns(assembly))
            {
                result |= this.RegisterInternal(cacheFile,
                                                definition);
            }
            return result;
        }
        private Boolean RegisterInternal(FileInfo cacheFile,
                                         AddInDefinition definition)
        {
            if (!this._cache.ContainsKey(cacheFile.FullName) ||
                this._cache[cacheFile.FullName].Contains(definition))
            {
                return false;
            }

            if (!this._register.ContainsKey(definition.Guid))
            {
                this._cache[cacheFile.FullName].Add(definition);
                this._register[definition.Guid] = definition.Security is AddInSecurity.Trusted
                                                        ? new __TrustedAddInController(definition,
                                                                                     this)
                                                        : new __IsolatedAddInController(definition,
                                                                                      this);
                this.CachedAddIns.Add(definition);
                return true;
            }

            AddInDefinition? outdated = this._cache[cacheFile.FullName].FirstOrDefault(d => d.Guid == definition.Guid);
            if (outdated is null)
            {
                AddInDefinition? other = this._cache.SelectMany(kv => kv.Value)
                                                    .FirstOrDefault(d => d.Guid == definition.Guid);
                if (other is not null &&
                    definition.Version > other.Version)
                {
                    if (this._register[definition.Guid].IsActive)
                    {
                        this._register[definition.Guid].Shutdown();
                    }
                    String file = this._cache.FirstOrDefault(kv => kv.Value.Contains(other))
                                             .Key;
                    Int32 index = this._cache[file].IndexOf(other);
                    this._cache[file].RemoveAt(index);
                    this._cache[cacheFile.FullName].Add(definition);
                    this._register[definition.Guid] = definition.Security is AddInSecurity.Trusted
                                                            ? new __TrustedAddInController(definition,
                                                                                         this)
                                                            : new __IsolatedAddInController(definition,
                                                                                          this);
                    this.CachedAddIns.Add(definition);
                    return true;
                }
                return false;
            }
            if (definition.Version > outdated.Version)
            {
                Int32 index = this._cache[cacheFile.FullName].IndexOf(outdated);
                if (this._register.ContainsKey(outdated.Guid) &&
                    this._register[outdated.Guid].IsActive)
                {
                    this._register[outdated.Guid].Shutdown();
                }
                this._cache[cacheFile.FullName].RemoveAt(index);
                this._cache[cacheFile.FullName].Insert(index,
                                              definition);
                this._register[definition.Guid] = definition.Security is AddInSecurity.Trusted
                                                        ? new __TrustedAddInController(definition,
                                                                                     this)
                                                        : new __IsolatedAddInController(definition,
                                                                                      this);
                this.CachedAddIns.Add(definition);
                return true;
            }
            return false;
        }

        private Boolean UnregisterInternal(FileInfo cacheFile,
                                           FileInfo assemblyFile)
        {
            Assembly? assembly = __AssemblyHelper.GetAssembly(assemblyFile);
            if (assembly is null)
            {
                return false;
            }

            Boolean result = false;
            foreach (AddInDefinition definition in AddInStore<TAddIn>.EnumerateAddIns(assembly))
            {
                result |= this.UnregisterInternal(cacheFile,
                                                  definition);
            }
            return result;
        }
        private Boolean UnregisterInternal(FileInfo cacheFile,
                                           AddInDefinition definition)
        {
            if (!this._cache.ContainsKey(cacheFile.FullName) ||
                !this._cache[cacheFile.FullName].Contains(definition))
            {
                return false;
            }

            Int32 index = this._cache[cacheFile.FullName].IndexOf(definition);
            if (this._register.ContainsKey(definition.Guid) &&
                this._register[definition.Guid].IsActive)
            {
                this._register[definition.Guid].Dispose();
            }
            this._cache[cacheFile.FullName].RemoveAt(index);
            this._register.Remove(definition.Guid);
            this.CachedAddIns.Remove(definition);
            return true;
        }

        private static List<AddInDefinition> ReadCacheFile(FileInfo file)
        {
            if (!file.Exists)
            {
                file.Create()
                    .Dispose();
                return new();
            }

            using FileStream stream = file.OpenRead();
            ByteSerializer<AddInDefinitionCollection> serializer = new();
            return !serializer.TryDeserialize(stream,
                                              out AddInDefinitionCollection? collection)
                        ? new()
                        : collection.ToList();
        }

        private static void WriteCacheFile(FileInfo file,
                                           IEnumerable<AddInDefinition> definitions)
        {
            using FileStream stream = file.Create();
            ByteSerializer<AddInDefinitionCollection> serializer = new();
            AddInDefinitionCollection collection = new(definitions);
            serializer.TrySerialize(stream,
                                    collection);
        }

        private static IEnumerable<AddInDefinition> DiscoverTrustedInternal(DirectoryInfo directory)
        {
            foreach (FileInfo dll in directory.GetFiles()
                                              .Where(f => f.Extension == ".dll"))
            {
                Assembly? assembly = __AssemblyHelper.GetAssembly(dll);
                if (assembly is null)
                {
                    continue;
                }
                foreach (AddInDefinition definition in AddInStore<TAddIn>.EnumerateAddIns(assembly))
                {
                    yield return definition;
                }
            }
            yield break;
        }

        private static IEnumerable<AddInDefinition> DiscoverIsolatedInternal(DirectoryInfo directory)
        {
            String appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(appData))
            {
                Directory.CreateDirectory(appData);
            }
            DirectoryInfo temp = new(Path.Combine(appData,
                                                  "Narumikazuchi.Extensibility.Process",
                                                  Environment.ProcessId.ToString()));
            if (!temp.Exists)
            {
                temp.Create();
            }
            String proc = Path.Combine(temp.FullName,
                                       "Discovery.exe");
            File.WriteAllBytes(proc,
                               Properties.Resources.ProcessWrapper);

            foreach (FileInfo dll in directory.GetFiles()
                                              .Where(f => f.Extension == ".dll"))
            {
                ProcessStartInfo psi = new()
                {
                    Arguments = "-d \"" + dll.FullName + "\"",
                    CreateNoWindow = true,
                    FileName = proc,
                    RedirectStandardOutput = true
                };
                Process? process = Process.Start(psi);
                if (process is null)
                {
                    continue;
                }
                while (!process.StandardOutput.EndOfStream)
                {
                    String? line = process.StandardOutput.ReadLine();
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    String[] tokens = line.Split('|');
                    if (tokens.Length < 6)
                    {
                        continue;
                    }
                    yield return new(Guid.Parse(tokens[0]),
                                     tokens[1],
                                     Version.Parse(tokens[2]),
                                     tokens[3],
                                     tokens[4],
                                     tokens[5],
                                     AddInSecurity.Isolated);
                }
                process.WaitForExit();
            }
            yield break;
        }

        private static IEnumerable<AddInDefinition> EnumerateAddIns(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes()
                                          .Where(t => AttributeResolver.HasAttribute<AddInAttribute>(t)))
            {
                AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(type);
                yield return new(attribute.Guid,
                                 attribute.Name,
                                 attribute.Version,
                                 type);
            }
        }

        private readonly Dictionary<Guid, IAddInController> _register;
        private readonly Dictionary<String, List<AddInDefinition>> _cache;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Object _mutex = new();
#pragma warning disable
        private readonly String _defaultCache =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                         "addin." + typeof(TAddIn).Name + ".cache");
        private IsolationSettings _isolationSettings;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String FILE_DOES_NOT_EXIST = "The specified file does not exist at this location. Full path: \"{0}\"";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String DIRECTORY_DOES_NOT_EXIST = "The specified directory does not exist at this location. Full path: \"{0}\"";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String CACHE_HAS_NOT_BEEN_LOADED = "The specified cache file has not been loaded into the store. Full path: \"{0}\"";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String ADDIN_HAS_NOT_YET_BEEN_REGISTERED = "The AddIn with the specified Guid '{0}' has not been registered in the store yet.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String ADDIN_TYPE_DOES_NOT_INHERIT_STORE_TYPEPARAM = "The specified type '{0}' can not be used in this store since it does not inherit from it's type parameter '{1}'.";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String CANNOT_CHANGE_ISOLATION_SETTINGS_WHILE_ADDINS_ARE_ACTIVE = "Changing of isolation settings while AddIns are actively running is not supported.";
#pragma warning restore
    }

    // IIndexGetter<Guid, AddInDefinition>
    partial class AddInStore<TAddIn> : IIndexGetter<Guid, AddInDefinition>
    {
        /// <inheritdoc/>
        public IEnumerator<AddInDefinition> GetEnumerator() =>
            this.CachedAddIns.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            this.GetEnumerator();

        /// <summary>
        /// Gets the corresponding <see cref="AddInDefinition"/> for the specified GUID.
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        public AddInDefinition this[Guid guid]
        {
            get
            {
                lock (this._mutex)
                {
                    return !this._register.ContainsKey(guid)
                                ? throw new KeyNotFoundException(String.Format(ADDIN_HAS_NOT_YET_BEEN_REGISTERED,
                                                                               guid.ToString()))
                                : this.CachedAddIns.First(d => d.Guid == guid);
                }
            }
        }
    }

    // IStoreEvents
    partial class AddInStore<TAddIn> : IStoreEvents
    {
        void IStoreEvents.OnActivating(IAddInController controller) =>
            this.AddInActivating?.Invoke(controller,
                                         EventArgs.Empty);
        void IStoreEvents.OnActivated(IAddInController controller) =>
            this.AddInActivated?.Invoke(controller,
                                        EventArgs.Empty);
        void IStoreEvents.OnShuttingDown(IAddInController controller) =>
            this.AddInShuttingDown?.Invoke(controller,
                                           EventArgs.Empty);
        void IStoreEvents.OnShutdown(IAddInController controller) =>
            this.AddInShutDown?.Invoke(controller,
                                       EventArgs.Empty);

        void IStoreEvents.OnAddInCrashed(IAddInController controller,
                                         IEnumerable<String?> exceptions) =>
            this.AddInCrashed?.Invoke(controller,
                                      new(exceptions));
    }

    // IStoreIsolation
    partial class AddInStore<TAddIn> : IStoreIsolation
    {
        Process IStoreIsolation.CreateIsolatedProcess(AddInDefinition definition)
        {
            if (!this._isolationSettings.IsolationDirectory.Exists)
            {
                this._isolationSettings.IsolationDirectory.Create();
            }
            String proc = Path.Combine(this._isolationSettings.IsolationDirectory.FullName,
                                       definition.Name + ".exe");
            File.WriteAllBytes(proc,
                               Properties.Resources.ProcessWrapper);

            ProcessStartInfo psi = new()
            {
                Arguments = $"-e \"{definition.Assembly}\" \"{definition.TypeName}\"",
                CreateNoWindow = true,
                FileName = proc,
            };
            return new()
            {
                StartInfo = psi
            };
        }

        IsolationSettings IStoreIsolation.IsolationSettings => this._isolationSettings;
    }
}