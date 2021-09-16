using Narumikazuchi.Serialization;
using Narumikazuchi.Serialization.Bytes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Provides the information about a specific AddIn.
    /// </summary>
    public sealed partial class AddInDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddInDefinition"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public AddInDefinition(Guid guid,
                               [DisallowNull] String name,
                               [DisallowNull] Version version,
                               [DisallowNull] Type type) :
            this(guid,
                 name,
                 version,
                 type,
                 AddInSecurity.Isolated)
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="AddInDefinition"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
#pragma warning disable
        public AddInDefinition(in Guid guid,
                               [DisallowNull] String name,
                               [DisallowNull] Version version,
                               [DisallowNull] Type type,
                               in AddInSecurity security) :
            this(guid,
                 name,
                 version,
                 type.FullName,
                 type.Assembly.FullName,
                 type.Assembly.Location,
                 security)
        { }
#pragma warning restore

        /// <summary>
        /// Gets the GUID of this AddIn.
        /// </summary>
        [Pure]
        public Guid Guid => this._guid;
        /// <summary>
        /// Gets the name of this AddIn.
        /// </summary>
        [Pure]
        [NotNull]
        public String Name => this._name;
        /// <summary>
        /// Gets the version number of this AddIn.
        /// </summary>
        [Pure]
        [NotNull]
        public Version Version => this._version;
        /// <summary>
        /// Gets or sets the security level for this AddIn.
        /// </summary>
        public AddInSecurity Security
        {
            get => this._security;
            set => this._security = value;
        }
    }

    // Non-Public
    partial class AddInDefinition
    {
        internal AddInDefinition(in Guid guid,
                                 String name,
                                 Version version,
                                 String typeName,
                                 String assemblyName,
                                 String assemblyLocation,
                                 in AddInSecurity security)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (version is null)
            {
                throw new ArgumentNullException(nameof(version));
            }
            if (typeName is null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }
            if (assemblyName is null)
            {
                throw new ArgumentNullException(nameof(assemblyName));
            }
            if (assemblyLocation is null)
            {
                throw new ArgumentNullException(nameof(assemblyLocation));
            }
            if (!EnumValidator.IsDefined(security))
            {
                throw new ArgumentOutOfRangeException(nameof(security));
            }

            this.Security = security;
            this._guid = guid;
            this._name = name;
            this._version = version;
            this._assemblyName = assemblyName;
            this._typeName = typeName;
            this._assembly = new(assemblyLocation);
        }

        internal async Task<Type?> FindAddInType()
        {
            Assembly assembly = await Task.Run(() => this.LoadAddInAssembly());
            return assembly.GetType(this.TypeName);
        }

        private Assembly LoadAddInAssembly()
        {
            Assembly? exisiting = AppDomain.CurrentDomain.GetAssemblies()
                                                         .FirstOrDefault(a => a.FullName == this.AssemblyName);
            if (exisiting is null)
            {
                Assembly? loaded = __AssemblyHelper.GetAssembly(this.Assembly);
                return loaded is null 
                            ? throw new BadImageFormatException() 
                            : loaded;
            }
            return exisiting;
        }

        internal String AssemblyName => this._assemblyName;
        internal String TypeName => this._typeName;
        internal FileInfo Assembly => this._assembly;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Guid _guid;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly String _name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Version _version;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly String _assemblyName;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly String _typeName;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly FileInfo _assembly;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private AddInSecurity _security = AddInSecurity.Isolated;
    }

    // IEquatable<AddInDefinition>
    partial class AddInDefinition : IEquatable<AddInDefinition>
    {
        /// <inheritdoc/>
        public Boolean Equals([AllowNull] AddInDefinition? other) =>
            other is not null &&
            this.Guid.Equals(other.Guid) &&
            this.Name.Equals( other.Name) &&
            this.Version.Equals(other.Version);

        /// <inheritdoc/>
        public override Boolean Equals([AllowNull] Object? obj) =>
            obj is AddInDefinition other &&
            this.Equals(other);

        /// <inheritdoc/>
        public override Int32 GetHashCode() =>
            this.Guid.GetHashCode() ^
            this.Name.GetHashCode() ^
            this.Version.GetHashCode();

#pragma warning disable
        public static Boolean operator ==([AllowNull] AddInDefinition? left, [AllowNull] AddInDefinition? right) =>
            left is null
                ? right is null
                : left.Equals(right);
        public static Boolean operator !=([AllowNull] AddInDefinition? left, [AllowNull] AddInDefinition? right) =>
            left is null
                ? right is not null
                : !left.Equals(right);
#pragma warning restore
    }

    // IByteSerializable
    partial class AddInDefinition : IByteSerializable
    {
        UInt32 IByteSerializable.SetState(Byte[] bytes) =>
            throw new NotImplementedException();

        UInt32 IByteSerializable.InitializeUninitializedState(Byte[] bytes)
        {
            Int32 result = 0;

            Byte[] data = bytes.Take(16)
                               .ToArray();
            result += 16;
            FieldInfo? field = typeof(AddInDefinition).GetField(nameof(this._guid),
                                                                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null)
            {
                throw new MissingMemberException();
            }
            field.SetValue(this, 
                              new Guid(data));

            Int32 length = BitConverter.ToInt32(bytes, 
                                                result);
            result += 4;
            String name = Encoding.UTF8.GetString(bytes, 
                                                  result, 
                                                  length);
            result += length;
            field = typeof(AddInDefinition).GetField(nameof(this._name),
                                                     BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null)
            {
                throw new MissingMemberException();
            }
            field.SetValue(this, 
                              name);

            Int32 major = BitConverter.ToInt32(bytes, 
                                               result);
            result += 4;
            Int32 minor = BitConverter.ToInt32(bytes, 
                                               result);
            result += 4;
            Int32 build = BitConverter.ToInt32(bytes, 
                                               result);
            result += 4;
            Int32 revision = BitConverter.ToInt32(bytes, 
                                                  result);
            result += 4;
            field = typeof(AddInDefinition).GetField(nameof(this._version),
                                                     BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null)
            {
                throw new MissingMemberException();
            }
            Version version;
            if (build == -1)
            {
                version = new(major, 
                              minor);
            }
            else if (build > -1 &&
                     revision == -1)
            {
                version = new(major, 
                              minor, 
                              build);
            }
            else
            {
                version = new(major, 
                              minor, 
                              build, 
                              revision);
            }
            field.SetValue(this, 
                              version);

            length = BitConverter.ToInt32(bytes, 
                                          result);
            result += 4;
            name = Encoding.UTF8.GetString(bytes, 
                                           result, 
                                           length);
            result += length;
            field = typeof(AddInDefinition).GetField(nameof(this._assemblyName),
                                                     BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null)
            {
                throw new MissingMemberException();
            }
            field.SetValue(this, 
                              name);

            length = BitConverter.ToInt32(bytes, 
                                          result);
            result += 4;
            name = Encoding.UTF8.GetString(bytes,   
                                           result, 
                                           length);
            result += length;
            field = typeof(AddInDefinition).GetField(nameof(this._typeName),
                                                     BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null)
            {
                throw new MissingMemberException();
            }
            field.SetValue(this, 
                              name);

            length = BitConverter.ToInt32(bytes, 
                                          result);
            result += 4;
            String filepath = Encoding.UTF8.GetString(bytes, 
                                                      result, 
                                                      length);
            result += length;
            field = typeof(AddInDefinition).GetField(nameof(this._assembly),
                                                     BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null)
            {
                throw new MissingMemberException();
            }
            field.SetValue(this, 
                              new FileInfo(filepath));

            return (UInt32)result;
        }

        Byte[] ISerializable.ToBytes()
        {
            List<Byte> result = new();

            result.AddRange(this.Guid.ToByteArray());

            Byte[] data = Encoding.UTF8.GetBytes(this.Name);
            Byte[] size = BitConverter.GetBytes(data.Length);
            result.AddRange(size);
            result.AddRange(data);

            result.AddRange(BitConverter.GetBytes(this.Version.Major));
            result.AddRange(BitConverter.GetBytes(this.Version.Minor));
            result.AddRange(BitConverter.GetBytes(this.Version.Build));
            result.AddRange(BitConverter.GetBytes(this.Version.Revision));

            data = Encoding.UTF8.GetBytes(this.AssemblyName);
            size = BitConverter.GetBytes(data.Length);
            result.AddRange(size);
            result.AddRange(data);

            data = Encoding.UTF8.GetBytes(this.TypeName);
            size = BitConverter.GetBytes(data.Length);
            result.AddRange(size);
            result.AddRange(data);

            data = Encoding.UTF8.GetBytes(this.Assembly.FullName);
            size = BitConverter.GetBytes(data.Length);
            result.AddRange(size);
            result.AddRange(data);

            return result.ToArray();
        }
    }
}
