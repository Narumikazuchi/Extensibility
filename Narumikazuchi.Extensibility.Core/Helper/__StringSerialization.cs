using System;
using System.Linq;
using System.Text;

namespace Narumikazuchi.Extensibility
{
    internal static class __StringSerialization
    {
        public static Byte[] SerializeString(String s)
        {
            Byte[] data = Encoding.UTF8
                                  .GetBytes(s);
            Byte[] size = BitConverter.GetBytes(data.Length);
            return size.Concat(data)
                       .ToArray();
        }

        public static String DeserializeString(Byte[] array, 
                                               ref Int32 offset)
        {
            Int32 length = BitConverter.ToInt32(array, 
                                                offset);
            offset += 4;
            String result = Encoding.UTF8.GetString(array, 
                                                    offset, 
                                                    length);
            offset += length;
            return result;
        }
    }
}
