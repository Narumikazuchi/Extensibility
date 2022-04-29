namespace Narumikazuchi.Extensibility;

[Singleton]
internal sealed partial class __GuidSerializationStrategy :
    ISerializationDeserializationStrategy<Byte[], Guid[]>
{ }

// IDeserializationStrategy<T, U>
partial class __GuidSerializationStrategy : IDeserializationStrategy<Byte[], Guid[]>
{
    public Guid[] Deserialize(Byte[] input)
    {
        Int32 length = BitConverter.ToInt32(value: input,
                                            startIndex: 0);
        Guid[] result = new Guid[length];

        IDeserializationStrategy<Byte[], Guid> guid = IntegratedSerializationStrategies.Guid.Reference;

        Int32 offset = 4;
        for (Int32 i = 0;
             i < length;
             i++)
        {
            
            Guid current = guid.Deserialize(input.Skip(offset)
                                                 .Take(16)
                                                 .ToArray());
            offset += 16;
            result[i] = current;
        }

        return result;
    }
}

// ISerializationStrategy<T, U>
partial class __GuidSerializationStrategy : ISerializationStrategy<Byte[], Guid[]>
{
    public Byte[] Serialize(Guid[] input)
    {
        List<Byte> result = new();

        result.AddRange(BitConverter.GetBytes(input.Length));

        ISerializationStrategy<Byte[], Guid> guid = IntegratedSerializationStrategies.Guid.Reference;

        for (Int32 i = 0;
             i < input.Length;
             i++)
        {
            result.AddRange(guid.Serialize(input[i]));
        }

        return result.ToArray();
    }
}