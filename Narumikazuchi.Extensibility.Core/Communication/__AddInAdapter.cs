using Narumikazuchi.Serialization.Bytes;
using Narumikazuchi.Windows.Pipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Narumikazuchi.Extensibility
{
    // Non-Generic class for ProcessWrapper
    internal abstract class __AddInAdapter
    {
        public abstract Boolean Process(__ByteMessage input,
                                        IClient<__ByteMessage> client);
    }

    // Abstract class implementation
    internal sealed partial class __AddInAdapter<TAddIn> : __AddInAdapter
    {
        public __AddInAdapter()
        {
            if (typeof(TAddIn).IsSingleton())
            {
                Type singleton = typeof(Singleton<>).MakeGenericType(typeof(TAddIn));
                PropertyInfo? property = singleton.GetProperty("Instance");
                TAddIn? addIn = (TAddIn?)property?.GetValue(null);
                if (addIn is null)
                {
                    throw new NullReferenceException();
                }
                this._addIn = addIn;
                return;
            }
            ConstructorInfo? ctor = typeof(TAddIn).GetConstructor(Array.Empty<Type>());
            if (ctor is null)
            {
                throw new NullReferenceException();
            }
            this._addIn = (TAddIn)ctor.Invoke(Array.Empty<Object>());
        }

        public override Boolean Process(__ByteMessage input,
                                        IClient<__ByteMessage> client)
        {
            if (!EnumValidator.IsDefined(input.Type) ||
                input.Type is MessageType.ErrorOrInvalid)
            {
                return true;
            }

            if (input.Type is MessageType.ShutdownCommand)
            {
                return false;
            }

            try
            {
                switch (input.Type)
                {
                    case MessageType.InvokeMethodVoid:
                        if (input.Signatures is null ||
                            input.Signatures.Length < 1 ||
                            input.Objects is null)
                        {
                            return true;
                        }
                        this.InvokeMethod(input.Signatures[0],
                                          input.Objects);
                        break;
                    case MessageType.InvokeMethodReturn:
                        if (input.Signatures is null ||
                            input.Signatures.Length < 1 ||
                            input.Objects is null)
                        {
                            return true;
                        }
                        this.InvokeMethodWithReturn(input.Signatures[0],
                                                    input.Objects,
                                                    client);
                        break;
                    case MessageType.InvokePropertyGet:
                        if (input.Signatures is null ||
                            input.Signatures.Length < 1)
                        {
                            return true;
                        }
                        this.InvokePropertyGet(input.Signatures[0],
                                               client);
                        break;
                    case MessageType.InvokePropertySet:
                        if (input.Signatures is null ||
                            input.Signatures.Length < 1 ||
                            input.Objects is null)
                        {
                            return true;
                        }
                        this.InvokePropertySet(input.Signatures[0],
                                               input.Objects);
                        break;
                    case MessageType.InvokeIndexerGet:
                        if (input.Signatures is null ||
                            input.Signatures.Length < 1 ||
                            input.Objects is null)
                        {
                            return true;
                        }
                        this.InvokeIndexerGet(input.Signatures[0],
                                              input.Objects,
                                              client);
                        break;
                    case MessageType.InvokeIndexerSet:
                        if (input.Signatures is null ||
                            input.Signatures.Length < 1 ||
                            input.Objects is null)
                        {
                            return true;
                        }
                        this.InvokeIndexerSet(input.Signatures[0],
                                              input.Objects);
                        break;
                    case MessageType.GetExposedMembers:
                        __AddInAdapter<TAddIn>.DetermineExposedMembers(client);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                client.SendAsync(__ByteMessage.Crash(e));
                return false;
            }
            return true;
        }
    }

    // Non-Public
    partial class __AddInAdapter<TAddIn>
    {
        private void InvokeMethod(__ByteMessage.SignatureData signature,
                                  __ByteMessage.ObjectData[] objects) =>
            this.InvokeMethodInternal(signature,
                                      objects);

        private void InvokeMethodWithReturn(__ByteMessage.SignatureData signature,
                                            __ByteMessage.ObjectData[] objects,
                                            IClient<__ByteMessage> client)
        {
            Object? result = this.InvokeMethodInternal(signature,
                                                       objects);
            client.SendAsync(__ByteMessage.InvocationResult(result));
        }

        private void InvokePropertyGet(__ByteMessage.SignatureData signature,
                                       IClient<__ByteMessage> client)
        {
            PropertyInfo property = __AddInAdapter<TAddIn>.FindProperty(signature.MemberName);
            if (!property.CanRead)
            {
                throw new NotSupportedException();
            }
            Object? result = property.GetValue(this._addIn);
            client.SendAsync(__ByteMessage.InvocationResult(result));
        }

        private void InvokePropertySet(__ByteMessage.SignatureData signature,
                                       __ByteMessage.ObjectData[] objects)
        {
            if (objects.Length != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(objects));
            }
            PropertyInfo property = __AddInAdapter<TAddIn>.FindProperty(signature.MemberName);
            if (!property.CanWrite)
            {
                throw new NotSupportedException();
            }
            property.SetValue(this._addIn, 
                              objects[0].Value);
        }

        private void InvokeIndexerGet(__ByteMessage.SignatureData signature,
                                      __ByteMessage.ObjectData[] objects,
                                      IClient<__ByteMessage> client)
        {
            PropertyInfo property = __AddInAdapter<TAddIn>.FindIndexer(signature.MemberName,
                                                                     signature.TypeName,
                                                                     signature.ParameterList);
            if (!property.CanRead)
            {
                throw new NotSupportedException();
            }
            Object? result = property.GetValue(this._addIn,
                                               objects.Select(o => o.Value)
                                                      .ToArray());
            client.SendAsync(__ByteMessage.InvocationResult(result));
        }

        private void InvokeIndexerSet(__ByteMessage.SignatureData signature,
                                      __ByteMessage.ObjectData[] objects)
        {
            if (objects.Length < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(objects));
            }
            PropertyInfo property = __AddInAdapter<TAddIn>.FindIndexer(signature.MemberName,
                                                                     signature.TypeName,
                                                                     signature.ParameterList);
            if (!property.CanWrite)
            {
                throw new NotSupportedException();
            }
            property.SetValue(this._addIn, 
                              objects[0].Value,
                              objects.Skip(1)
                                     .Select(o => o.Value)
                                     .ToArray());
        }

        private Object? InvokeMethodInternal(__ByteMessage.SignatureData signature,
                                             __ByteMessage.ObjectData[] objects)
        {
            Type[] types;
            IEnumerable<Type?> temp = signature.ParameterList
                                               .Select(p => Type.GetType(p));
            if (temp.Any(t => t is null))
            {
                throw new TypeAccessException();
            }
#pragma warning disable
            types = temp.ToArray();
#pragma warning restore

            foreach (MethodInfo method in typeof(TAddIn).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                        .Where(m => m.Name == signature.MemberName))
            {
                if (types.SequenceEqual(method.GetParameters()
                                              .Select(p => p.ParameterType)))
                {
                    return method.Invoke(this._addIn,
                                         objects.Select(o => o.Value)
                                                .ToArray());
                }
            }
            throw new MissingMethodException();
        }

        private static PropertyInfo FindProperty(String name)
        {
            PropertyInfo? property = typeof(TAddIn).GetProperty(name,
                                                                BindingFlags.Public | BindingFlags.Instance);
            return property is null 
                        ? throw new MissingMemberException() 
                        : property;
        }

        private static PropertyInfo FindIndexer(String name, 
                                                String typeName,
                                                IReadOnlyList<String> parameters)
        {
            Type? returnType = Type.GetType(typeName);
            if (returnType is null)
            {
                throw new TypeAccessException();
            }
            List<Type> types = new();
            for (Int32 i = 0; i < parameters.Count; i++)
            {
                Type? type = Type.GetType(parameters[i]);
                if (type is null)
                {
                    throw new TypeAccessException();
                }
                types.Add(type);
            }
            PropertyInfo? property = typeof(TAddIn).GetProperty(name,
                                                                BindingFlags.Public | BindingFlags.Instance,
                                                                null,
                                                                returnType,
                                                                types.ToArray(),
                                                                null);
            return property is null
                        ? throw new MissingMemberException()
                        : property;
        }

        private static void DetermineExposedMembers(IClient<__ByteMessage> client)
        {
            List<MethodSignature> methods = new();
            foreach (MethodInfo methodInfo in typeof(TAddIn).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                            .Where(m => AttributeResolver.HasAttribute<AddInExposedAttribute>(m)))
            {
                if (!AreParametersValid(methodInfo.GetParameters()) ||
                    !IsTypeTransferrable(methodInfo.ReturnType,
                                         true))
                {
                    continue;
                }
                MethodSignature method = new(methodInfo);
                methods.Add(method);
            }

            List<PropertySignature> properties = new();
            List<IndexerSignature> indexers = new();
            foreach (PropertyInfo propertyInfo in typeof(TAddIn).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                                .Where(p => AttributeResolver.HasAttribute<AddInExposedAttribute>(p)))
            {
                if (!IsTypeTransferrable(propertyInfo.PropertyType,
                                         false))
                {
                    continue;
                }
                if (propertyInfo.GetIndexParameters()
                                .Length > 0)
                {
                    if (!AreParametersValid(propertyInfo.GetIndexParameters()))
                    {
                        continue;
                    }
                    IndexerSignature indexer = new(propertyInfo);
                    indexers.Add(indexer);
                    continue;
                }
                PropertySignature property = new(propertyInfo);
                properties.Add(property);
            }

            client.SendAsync(__ByteMessage.MemberInformation(methods.ToArray(),
                                                           properties.ToArray(),
                                                           indexers.ToArray()));
        }

        private static Boolean AreParametersValid(IEnumerable<ParameterInfo> parameters)
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

        private static Boolean IsTypeTransferrable(Type type,
                                                   Boolean method) =>
            type.IsPrimitive ||
            type == typeof(String) ||
            type.IsAssignableTo(typeof(IByteSerializable)) ||
            (method &&
            type == typeof(void));

        private readonly TAddIn _addIn;
    }
}
