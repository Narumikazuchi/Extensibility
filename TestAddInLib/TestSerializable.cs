using Narumikazuchi.Serialization;
using Narumikazuchi.Serialization.Bytes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestAddInLib
{
    public class TestSerializable : IByteSerializable
    {
        public TestSerializable()
        { }
        public TestSerializable(IEnumerable<Byte> content) =>
            this._content = content.ToArray();

        UInt32 IByteSerializable.InitializeUninitializedState(Byte[] bytes) => 
            throw new NotImplementedException();
        UInt32 IByteSerializable.SetState(Byte[] bytes)
        {
            this._content = new Byte[bytes.Length];
            Array.Copy(bytes, this._content, bytes.Length);
            return (UInt32)bytes.Length;
        }
        Byte[] ISerializable.ToBytes() => this._content;

        private Byte[] _content = new Byte[1] { 0xFF };
    }
}
