using Narumikazuchi.Collections.Abstract;
using Narumikazuchi.Serialization;
using Narumikazuchi.Serialization.Bytes;
using System;
using System.Collections.Generic;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Represents a collection of <see cref="AddInDefinition"/> objects.
    /// </summary>
    public sealed partial class AddInDefinitionCollection :  ReadOnlyCollectionBase<AddInDefinition>
    {
        internal AddInDefinitionCollection() : 
            base()
        { }
        internal AddInDefinitionCollection(IEnumerable<AddInDefinition> definitions) :
            base(definitions)
        { }

        internal void Add(AddInDefinition definition)
        {
            this.EnsureCapacity(this._size + 1);
            lock (this._syncRoot)
            {
                this._items[this._size++] = definition;
                this._version++;
            }
        }

        internal void Remove(AddInDefinition definition)
        {
            lock (this._syncRoot)
            {
                for (Int32 i = 0; i < this._size; i++)
                {
                    if (definition.Equals(this._items[i]))
                    {
                        Array.Copy(this._items, 
                                   i + 1, 
                                   this._items, 
                                   i, 
                                   this._size - i);
                        this._size--;
                        this._items[this._size] = null;
                        return;
                    }
                }
            }
        }
    }

    // IByteSerializable
    partial class AddInDefinitionCollection : IByteSerializable
    {
        UInt32 IByteSerializable.InitializeUninitializedState(Byte[] bytes) =>
            throw new NotImplementedException();

        UInt32 IByteSerializable.SetState(Byte[] bytes)
        {
            Int32 result = 0;

            Int32 size = BitConverter.ToInt32(bytes, 
                                              result);
            result += 4;

            ByteSerializer<AddInDefinition> serializer = new();
            for (Int32 i = 0; i < size; i++)
            {
                AddInDefinition definition = serializer.Deserialize(bytes, 
                                                                    result, 
                                                                    out UInt32 read);
                result += (Int32)read;
                this.Add(definition);
            }

            return (UInt32)result;
        }

        Byte[] ISerializable.ToBytes()
        {
            List<Byte> result = new();
            lock (this._syncRoot)
            {
                Byte[] data = BitConverter.GetBytes(this._size);
                result.AddRange(data);

                ByteSerializer<AddInDefinition> serializer = new();
                for (Int32 i = 0; i < this._size; i++)
                {
#pragma warning disable
                    result.AddRange(serializer.Serialize(this._items[i]));
#pragma warning restore
                }
            }
            return result.ToArray();
        }
    }
}
