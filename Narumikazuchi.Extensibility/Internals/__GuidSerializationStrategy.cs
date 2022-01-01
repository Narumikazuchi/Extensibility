namespace Narumikazuchi.Extensibility;

internal sealed partial class __GuidSerializationStrategy
{ }

// Non-Public
partial class __GuidSerializationStrategy :
    Singleton,
    ISerializationDeserializationStrategy<Byte[], Guid[]>
{
    private __GuidSerializationStrategy()
    { }
}

// IDeserializationStrategy<T>
partial class __GuidSerializationStrategy : IDeserializationStrategy<Byte[]>
{
    Object? IDeserializationStrategy<Byte[]>.Deserialize(Byte[] input) =>
        this.Deserialize(input: input);
}

// IDeserializationStrategy<T, U>
partial class __GuidSerializationStrategy : IDeserializationStrategy<Byte[], Guid[]>
{
    public Guid[] Deserialize(Byte[] input)
    {
        Int32 length = BitConverter.ToInt32(value: input,
                                            startIndex: 0);
        Guid[] result = new Guid[length];

        Int32 offset = 4;
        for (Int32 i = 0;
             i < length;
             i++)
        {
            Guid current = new(b: input.Skip(offset)
                                       .Take(16)
                                       .ToArray());
            offset += 16;
            result[i] = current;
        }

        return result;
    }
}

// ISerializationStrategy<T>
partial class __GuidSerializationStrategy : ISerializationStrategy<Byte[]>
{
    Byte[] ISerializationStrategy<Byte[]>.Serialize(Object? input) =>
        this.Serialize((Guid[])input!);
}

// ISerializationStrategy<T, U>
partial class __GuidSerializationStrategy : ISerializationStrategy<Byte[], Guid[]>
{
    public Byte[] Serialize(Guid[] input)
    {
        List<Byte> result = new();

        result.AddRange(BitConverter.GetBytes(input.Length));
        for (Int32 i = 0;
             i < input.Length;
             i++)
        {
            result.AddRange(input[i].ToByteArray());
        }

        return result.ToArray();
    }
}