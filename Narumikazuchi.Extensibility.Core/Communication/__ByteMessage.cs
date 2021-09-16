using Narumikazuchi.Serialization;
using Narumikazuchi.Serialization.Bytes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Narumikazuchi.Extensibility
{
    internal sealed partial class __ByteMessage
    {
        public static __ByteMessage Crash(Exception exception) =>
            new(MessageType.CrashNotification,
                null,
                ObjectData.FromException(exception));
        public static __ByteMessage InvokeMethod(MethodSignature method,
                                               params Object[] parameters) => 
            new(method.ReturnType == typeof(void)
                    ? MessageType.InvokeMethodVoid
                    : MessageType.InvokeMethodReturn,
                new SignatureData[] { new(method) },
                ObjectData.FromParameters(parameters));
        public static __ByteMessage InvokePropertyGetter(PropertySignature property) =>
            new(MessageType.InvokePropertyGet,
                new SignatureData[] { new(property) },
                null);
        public static __ByteMessage InvokePropertySetter(PropertySignature property,
                                                       Object? newValue) =>
            new(MessageType.InvokePropertySet,
                new SignatureData[] { new(property) },
                ObjectData.FromParameters(new Object?[] { newValue }));
        public static __ByteMessage InvokeIndexGetter(IndexerSignature indexer,
                                                    params Object[] parameters) =>
            new(MessageType.InvokeIndexerGet,
                new SignatureData[] { new(indexer) },
                ObjectData.FromParameters(parameters));
        public static __ByteMessage InvokeIndexSetter(IndexerSignature indexer,
                                                    Object? newValue,
                                                    params Object[] parameters) =>
            new(MessageType.InvokeIndexerSet,
                new SignatureData[] { new(indexer) },
                ObjectData.FromParameters(parameters.Prepend(newValue)
                                                    .ToArray()));
        public static __ByteMessage InvocationResult(Object? result) =>
            new(MessageType.InvocationResult,
                null,
                ObjectData.FromParameters(new Object?[] { result }));
        public static __ByteMessage MemberInformation(MethodSignature[]? methods,
                                                    PropertySignature[]? properties,
                                                    IndexerSignature[]? indexers)
        {
            List<SignatureData> signatures = new();
            if (methods is not null)
            {
                signatures.AddRange(methods.Select(m => new SignatureData(m)));
            }
            if (properties is not null)
            {
                signatures.AddRange(properties.Select(p => new SignatureData(p)));
            }
            if (indexers is not null)
            {
                signatures.AddRange(indexers.Select(i => new SignatureData(i)));
            }
            return new(MessageType.ExposedMemberInformation,
                       signatures.ToArray(),
                       null);
        }

        public static __ByteMessage Shutdown { get; } = new(MessageType.ShutdownCommand,
                                                          null,
                                                          null);
        public static __ByteMessage GetExposedMembers { get; } = new(MessageType.GetExposedMembers,
                                                                   null,
                                                                   null);

        public SignatureData[]? Signatures { get; init; }
        public MessageType Type { get; init; }
        public ObjectData[]? Objects { get; init; }
    }

    // Non-Public
    partial class __ByteMessage
    {
        private __ByteMessage(MessageType type,
                            SignatureData[]? signatures,
                            ObjectData[]? objects)
        {
            this.Type = type;
            this.Signatures = signatures;
            this.Objects = objects;
        }
        private __ByteMessage(MessageType type,
                            __ByteMessage source) :
            this(type,
                 source.Signatures,
                 source.Objects)
        { }
    }

    // IByteSerializable
    /*
     * Byte Structure:
     *      1 2 3..n n..m
     *  1 - MessageType
     *  2 - Data Flags 
     *          00 - no signature and no objects
     *          01 - signature set but no objects
     *          10 - no signature but objects set
     *          11 - signature and objects set
     *  3..n - Serialized signature, if present; else serialized objects, if present; otherwise not present
     *  n..m - Serialized objects, if signature is set and objects is set; else not present
     */
    partial class __ByteMessage : IByteSerializable
    {
#nullable disable
        UInt32 IByteSerializable.SetState(Byte[] bytes) => 
            throw new NotImplementedException();

        UInt32 IByteSerializable.InitializeUninitializedState(Byte[] bytes)
        {
            MessageType type = (MessageType)bytes[0];

            PropertyInfo propInfo = typeof(__ByteMessage).GetProperty(nameof(this.Type));
            propInfo.SetValue(this, 
                              type);


            Byte flags = bytes[1];
            Int32 offset = 2;
            if ((flags & 0b01) == 0b01)
            {
                propInfo = typeof(__ByteMessage).GetProperty(nameof(this.Signatures));

                Int32 count = BitConverter.ToInt32(bytes, 
                                                   offset);
                offset += 4;

                List<SignatureData> temp = new();

                ByteSerializer<SignatureData> signatureSerializer = new();
                for (Int32 i = 0; i < count; i++)
                {
                    temp.Add(signatureSerializer.Deserialize(bytes,
                                                             offset,
                                                             out UInt32 read));
                    offset += (Int32)read;
                }
                propInfo.SetValue(this,
                                  temp.ToArray());
            }
            
            if ((flags & 0b10) == 0b10)
            {
                propInfo = typeof(__ByteMessage).GetProperty(nameof(this.Objects));

                Int32 count = BitConverter.ToInt32(bytes, 
                                                   offset);
                offset += 4;

                List<ObjectData> temp = new();
                ByteSerializer<ObjectData> objectSerializer = new();
                for (Int32 i = 0; i < count; i++)
                {
                    temp.Add(objectSerializer.Deserialize(bytes,
                                                          offset, 
                                                          out UInt32 read));
                    offset += (Int32)read;
                }
                propInfo.SetValue(this,
                                  temp.ToArray());
            }

            return (UInt32)offset;
        }

        Byte[] ISerializable.ToBytes()
        {
            List<Byte> result = new()
            {
                (Byte)this.Type
            };
            Byte flags = 0;
            if (this.Signatures is not null)
            {
                flags |= 0b01;
            }
            if (this.Objects is not null)
            {
                flags |= 0b10;
            }
            result.Add(flags);

            if (this.Signatures is not null)
            {
                Byte[] size = BitConverter.GetBytes(this.Signatures.Length);
                result.AddRange(size);

                ByteSerializer<SignatureData> serializer = new();
                for (Int32 i = 0; i < this.Signatures.Length; i++)
                {
                    result.AddRange(serializer.Serialize(this.Signatures[i]));
                }
            }

            if (this.Objects is not null)
            {
                Byte[] size = BitConverter.GetBytes(this.Objects.Length);
                result.AddRange(size);

                ByteSerializer<ObjectData> serializer = new();
                for (Int32 i = 0; i < this.Objects.Length; i++)
                {
                    result.AddRange(serializer.Serialize(this.Objects[i]));
                }
            }

            return result.ToArray();
        }
#nullable enable
    }

    // SignatureData
    partial class __ByteMessage
    {
        public partial class SignatureData
        {
#pragma warning disable
            internal SignatureData()
            {
                this.TypeName = String.Empty;
                this.MemberName = String.Empty;
                this.ParameterList = new List<String>(0);
                this.HasGetter = null;
                this.HasSetter = null;
            }
            public SignatureData(MethodSignature method) :
                this()
            {
                this.SignatureType = SignatureType.Method;
                this.TypeName = method.ReturnType
                                      .AssemblyQualifiedName;
                this.MemberName = method.Name;
                this.ParameterList = method.Parameters
                                           .Select(t => t.AssemblyQualifiedName)
                                           .ToList();
            }
            public SignatureData(PropertySignature property) :
                this()
            {
                this.SignatureType = SignatureType.Property;
                this.TypeName = property.Type
                                        .AssemblyQualifiedName;
                this.MemberName = property.Name;
                this.HasGetter = property.HasGetter;
                this.HasSetter = property.HasSetter;
            }
            public SignatureData(IndexerSignature indexer)
            {
                this.SignatureType = SignatureType.Indexer;
                this.TypeName = indexer.Type
                                       .AssemblyQualifiedName;
                this.MemberName = indexer.Name;
                this.ParameterList = indexer.Indecies
                                            .Select(t => t.AssemblyQualifiedName)
                                            .ToList();
                this.HasGetter = indexer.HasGetter;
                this.HasSetter = indexer.HasSetter;
            }
#pragma warning restore

            public SignatureType SignatureType { get; set; }
            public String TypeName { get; set; }
            public String MemberName { get; set; }
            public IReadOnlyList<String> ParameterList { get; set; }
            public Boolean? HasGetter { get; set; }
            public Boolean? HasSetter { get; set; }
        }
    }

    // SignatureData : IByteSerializable
    partial class __ByteMessage
    {
        partial class SignatureData : IByteSerializable
        {
#nullable disable
            UInt32 IByteSerializable.InitializeUninitializedState(Byte[] bytes) => 
                throw new NotImplementedException();

            UInt32 IByteSerializable.SetState(Byte[] bytes)
            {
                this.SignatureType = (SignatureType)bytes[0];

                Int32 offset = 1;

                this.TypeName = __StringSerialization.DeserializeString(bytes,
                                                                        ref offset);
                this.MemberName = __StringSerialization.DeserializeString(bytes,
                                                                          ref offset);

                Byte getset = bytes[offset];
                offset++;
                if (getset > 0)
                {
                    this.HasGetter = (getset & 0b01) == 0b01;
                    this.HasSetter = (getset & 0b10) == 0b10;
                }

                Int32 count = BitConverter.ToInt32(bytes,
                                                   offset);
                offset += 4;

                List<String> parameters = new();
                for (Int32 i = 0; i < count; i++)
                {
                    parameters.Add(__StringSerialization.DeserializeString(bytes,
                                                                           ref offset));
                }
                this.ParameterList = parameters;

                return (UInt32)offset;
            }

            Byte[] ISerializable.ToBytes()
            {
                List<Byte> result = new()
                {
                    (Byte)this.SignatureType
                };

                result.AddRange(__StringSerialization.SerializeString(this.TypeName));
                result.AddRange(__StringSerialization.SerializeString(this.MemberName));

                if (!this.HasGetter
                         .HasValue ||
                    !this.HasSetter
                         .HasValue)
                {
                    result.Add(0);
                }
                else
                {
                    Byte getset = 0;
                    if (this.HasGetter.Value)
                    {
                        getset |= 0b01;
                    }
                    if (this.HasSetter.Value)
                    {
                        getset |= 0b10;
                    }
                    result.Add(getset);
                }

                Byte[] size = BitConverter.GetBytes(this.ParameterList
                                                        .Count);
                result.AddRange(size);

                for (Int32 i = 0;
                     i < this.ParameterList
                             .Count;
                    i++)
                {
                    result.AddRange(__StringSerialization.SerializeString(this.ParameterList[i]));
                }

                return result.ToArray();
            }
#nullable enable
        }
    }

    // ObjectData
    partial class __ByteMessage
    {
        [DebuggerDisplay("{Value}")]
        public partial class ObjectData
        {
            private ObjectData(Object? value)
            {
                if (value is null)
                {
                    this._type = ParameterType.Primitive_Null;
                    this._value = value;
                    return;
                }
                if (value.GetType()
                         .IsPrimitive)
                {
                    if(value.GetType() == typeof(Boolean))
                    {
                        this._type = ParameterType.Primitive_Boolean;
                    }
                    else if (value.GetType() == typeof(Byte))
                    {
                        this._type = ParameterType.Primitive_Byte;
                    }
                    else if (value.GetType() == typeof(Char))
                    {
                        this._type = ParameterType.Primitive_Char;
                    }
                    else if (value.GetType() == typeof(Double))
                    {
                        this._type = ParameterType.Primitive_Double;
                    }
                    else if (value.GetType() == typeof(Int16))
                    {
                        this._type = ParameterType.Primitive_Int16;
                    }
                    else if (value.GetType() == typeof(Int32))
                    {
                        this._type = ParameterType.Primitive_Int32;
                    }
                    else if (value.GetType() == typeof(Int64))
                    {
                        this._type = ParameterType.Primitive_Int64;
                    }
                    else if (value.GetType() == typeof(IntPtr))
                    {
                        this._type = ParameterType.Primitive_IntPtr;
                    }
                    else if (value.GetType() == typeof(SByte))
                    {
                        this._type = ParameterType.Primitive_SByte;
                    }
                    else if (value.GetType() == typeof(Single))
                    {
                        this._type = ParameterType.Primitive_Single;
                    }
                    else if (value.GetType() == typeof(UInt16))
                    {
                        this._type = ParameterType.Primitive_UInt16;
                    }
                    else if (value.GetType() == typeof(UInt32))
                    {
                        this._type = ParameterType.Primitive_UInt32;
                    }
                    else if (value.GetType() == typeof(UInt64))
                    {
                        this._type = ParameterType.Primitive_UInt64;
                    }
                    else if (value.GetType() == typeof(UIntPtr))
                    {
                        this._type = ParameterType.Primitive_UIntPtr;
                    }
                    this._value = value;
                    return;
                }
                if (value.GetType() == typeof(String))
                {
                    this._type = ParameterType.Primitive_String;
                    this._value = value;
                    return;
                }
                if (value.GetType()
                         .IsAssignableTo(typeof(IByteSerializable)))
                {
                    this._type = ParameterType.Serializable;
                    this._value = value;
                    return;
                }
                throw new NotSupportedException();
            }

            public static ObjectData[]? FromParameters(Object?[] parameters)
            {
                List<ObjectData> result = new();

                for (Int32 i = 0; i < parameters.Length; i++)
                {
                    result.Add(new(parameters[i]));
                }

                return result.ToArray();
            }

            public static ObjectData[]? FromException(Exception exception)
            {
                List<ObjectData> result = new();

                result.Add(new(exception.StackTrace));

                while (exception.InnerException is not null)
                {
                    exception = exception.InnerException;
                    result.Add(new(exception.StackTrace));
                }

                return result.ToArray();
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public ParameterType Type => this._type;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public Object? Value => this._value;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly ParameterType _type;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly Object? _value;
        }
    }

    // ObjectData : IByteSerializable
    partial class __ByteMessage
    {
        partial class ObjectData : IByteSerializable
        {
#nullable disable
            UInt32 IByteSerializable.SetState(Byte[] bytes) => 
                throw new NotImplementedException();

            UInt32 IByteSerializable.InitializeUninitializedState(Byte[] bytes)
            {
                ParameterType type = (ParameterType)bytes[0];
                if (!EnumValidator.IsDefined(type))
                {
                    throw new DataMisalignedException();
                }

                FieldInfo fieldInfo = typeof(ObjectData).GetField(nameof(this._type),
                                                                  BindingFlags.Instance | BindingFlags.NonPublic);
                fieldInfo.SetValue(this, 
                                  type);

                if (type is ParameterType.NotSupported)
                {
                    return 1;
                }

                fieldInfo = typeof(ObjectData).GetField(nameof(this._value),
                                                        BindingFlags.Instance | BindingFlags.NonPublic);

                if (type is ParameterType.Primitive_Null)
                {
                    fieldInfo.SetValue(this, 
                                       null);
                    return 1;
                }

                Int32 offset = 1;
                if (type is ParameterType.Serializable)
                {
                    String typename = __StringSerialization.DeserializeString(bytes, 
                                                                              ref offset);
                    Type objectType;
                    try
                    {
                        objectType = System.Type.GetType(typename);
                    }
                    catch
                    {
                        return (UInt32)offset;
                    }
                    Type serializerType = typeof(ByteSerializer<>).MakeGenericType(objectType);
                    ConstructorInfo ctor = serializerType.GetConstructor(Array.Empty<Type>());
                    Object serializer = ctor.Invoke(Array.Empty<Object>());
                    MethodInfo method = serializerType.GetMethod("Deserialize",
                                                                 BindingFlags.Public | BindingFlags.Instance,
                                                                 null,
                                                                 new Type[] { typeof(Byte[]), typeof(UInt32).MakeByRefType() },
                                                                 null);
                    Object[] parameters = new Object[] { bytes, null };
                    fieldInfo.SetValue(this, 
                                       method.Invoke(serializer, 
                                                     parameters));
                    offset += (Int32)parameters[1];
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_Boolean)
                {
                    Boolean value = BitConverter.ToBoolean(bytes, 
                                                           offset);
                    offset += sizeof(Boolean);
                    fieldInfo.SetValue(this, 
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_Byte)
                {
                    Byte value = bytes[offset];
                    offset += sizeof(Byte);
                    fieldInfo.SetValue(this, 
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_Char)
                {
                    Char value = BitConverter.ToChar(bytes, 
                                                     offset);
                    offset += sizeof(Char);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_Double)
                {
                    Double value = BitConverter.ToDouble(bytes,
                                                         offset);
                    offset += sizeof(Double);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_Int16)
                {
                    Int16 value = BitConverter.ToInt16(bytes, 
                                                       offset);
                    offset += sizeof(Int16);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_Int32)
                {
                    Int32 value = BitConverter.ToInt32(bytes, 
                                                       offset);
                    offset += sizeof(Int32);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_Int64)
                {
                    Int64 value = BitConverter.ToInt64(bytes, 
                                                       offset);
                    offset += sizeof(Int64);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_IntPtr)
                {
                    if (IntPtr.Size == sizeof(Int32))
                    {
                        Int32 value = BitConverter.ToInt32(bytes, 
                                                           offset);
                        offset += sizeof(Int32);
                        fieldInfo.SetValue(this, 
                                           (IntPtr)value);
                        return (UInt32)offset;
                    }
                    else
                    {
                        Int64 value = BitConverter.ToInt64(bytes, 
                                                           offset);
                        offset += sizeof(Int64);
                        fieldInfo.SetValue(this, 
                                           (IntPtr)value);
                        return (UInt32)offset;
                    }
                }
                if (type is ParameterType.Primitive_SByte)
                {
                    SByte value = (SByte)bytes[offset];
                    offset += sizeof(SByte);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_Single)
                {
                    Single value = BitConverter.ToSingle(bytes, 
                                                         offset);
                    offset += sizeof(Single);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_String)
                {
                    String value = __StringSerialization.DeserializeString(bytes,
                                                                           ref offset);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_UInt16)
                {
                    UInt16 value = BitConverter.ToUInt16(bytes, 
                                                         offset);
                    offset += sizeof(UInt16);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_UInt32)
                {
                    UInt32 value = BitConverter.ToUInt32(bytes, 
                                                         offset);
                    offset += sizeof(UInt32);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_UInt64)
                {
                    UInt64 value = BitConverter.ToUInt64(bytes, 
                                                         offset);
                    offset += sizeof(UInt64);
                    fieldInfo.SetValue(this,
                                       value);
                    return (UInt32)offset;
                }
                if (type is ParameterType.Primitive_UIntPtr)
                {
                    if (UIntPtr.Size == sizeof(UInt32))
                    {
                        UInt32 value = BitConverter.ToUInt32(bytes, 
                                                             offset);
                        offset += sizeof(UInt32);
                        fieldInfo.SetValue(this, 
                                           (UIntPtr)value);
                        return (UInt32)offset;
                    }
                    else
                    {
                        UInt64 value = BitConverter.ToUInt64(bytes, 
                                                             offset);
                        offset += sizeof(UInt64);
                        fieldInfo.SetValue(this, 
                                           (UIntPtr)value);
                        return (UInt32)offset;
                    }
                }
                return 1;
            }

            Byte[] ISerializable.ToBytes()
            {
                if (this.Type is ParameterType.Primitive_Null)
                {
                    return new Byte[] { (Byte)this.Type };
                }

                List<Byte> result = new()
                {
                    (Byte)this.Type
                };

                if (this.Type is ParameterType.Serializable)
                {
                    result.AddRange(__StringSerialization.SerializeString(this.Value
                                                                              .GetType()
                                                                              .AssemblyQualifiedName));

                    Type type = typeof(ByteSerializer<>).MakeGenericType(this.Value
                                                                             .GetType());
                    ConstructorInfo ctor = type.GetConstructor(Array.Empty<Type>());
                    Object serializer = ctor.Invoke(Array.Empty<Object>());
                    MethodInfo method = type.GetMethod("Serialize", 
                                                        BindingFlags.Public | BindingFlags.Instance, 
                                                        null, 
                                                        new Type[] { this.Value.GetType() }, 
                                                        null);
                    Byte[] data = (Byte[])method.Invoke(serializer, new Object[] { this.Value });
                    result.AddRange(data);
                }
                else if (this.Type is not ParameterType.NotSupported)
                {
                    Byte[] data;
                    switch (this.Type)
                    {
                        case ParameterType.Primitive_Boolean:
                            data = BitConverter.GetBytes((Boolean)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_Byte:
                            result.Add((Byte)this.Value);
                            break;
                        case ParameterType.Primitive_Char:
                            data = BitConverter.GetBytes((Char)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_Double:
                            data = BitConverter.GetBytes((Double)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_Int16:
                            data = BitConverter.GetBytes((Int16)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_Int32:
                            data = BitConverter.GetBytes((Int32)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_Int64:
                            data = BitConverter.GetBytes((Int64)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_IntPtr:
                            if (IntPtr.Size == sizeof(Int32))
                            {
                                data = BitConverter.GetBytes((Int32)(IntPtr)this.Value);
                                result.AddRange(data);
                                break;
                            }
                            data = BitConverter.GetBytes((Int64)(IntPtr)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_SByte:
                            result.Add((Byte)(SByte)this.Value);
                            break;
                        case ParameterType.Primitive_Single:
                            data = BitConverter.GetBytes((Single)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_String:
                            data = __StringSerialization.SerializeString((String)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_UInt16:
                            data = BitConverter.GetBytes((UInt16)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_UInt32:
                            data = BitConverter.GetBytes((UInt32)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_UInt64:
                            data = BitConverter.GetBytes((UInt64)this.Value);
                            result.AddRange(data);
                            break;
                        case ParameterType.Primitive_UIntPtr:
                            if (UIntPtr.Size == sizeof(UInt32))
                            {
                                data = BitConverter.GetBytes((UInt32)(UIntPtr)this.Value);
                                result.AddRange(data);
                                break;
                            }
                            data = BitConverter.GetBytes((UInt64)(UIntPtr)this.Value);
                            result.AddRange(data);
                            break;
                    }
                }

                return result.ToArray();
            }
#nullable enable
        }
    }
}
