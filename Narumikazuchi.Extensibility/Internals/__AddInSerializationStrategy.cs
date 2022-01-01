namespace Narumikazuchi.Extensibility;

internal sealed partial class __AddInDefinitionSerializationStrategy
{ }

// Non-Public
partial class __AddInDefinitionSerializationStrategy : 
    Singleton, 
    ISerializationDeserializationStrategy<Byte[], AddInDefinition[]>
{ 
    private __AddInDefinitionSerializationStrategy()
    { }
}

// IDeserializationStrategy<T>
partial class __AddInDefinitionSerializationStrategy : IDeserializationStrategy<Byte[]>
{
    Object? IDeserializationStrategy<Byte[]>.Deserialize(Byte[] input) =>
        this.Deserialize(input: input);
}

// IDeserializationStrategy<T, U>
partial class __AddInDefinitionSerializationStrategy : IDeserializationStrategy<Byte[], AddInDefinition[]>
{
    public AddInDefinition[] Deserialize(Byte[] input)
    {
        Int32 length = BitConverter.ToInt32(value: input,
                                            startIndex: 0);
        AddInDefinition[] result = new AddInDefinition[length];

        IByteDeserializer<AddInDefinition> deserializer = CreateByteSerializer
                                                         .ForDeserialization()
                                                         .ConfigureForOwnedType<AddInDefinition>()
                                                         .UseDefaultStrategies()
                                                         .Construct();

        Int32 offset = 4;
        for (Int32 i = 0;
             i < length;
             i++)
        {
            Int32 size = BitConverter.ToInt32(value: input,
                                              startIndex: offset);
            offset += 4;
            using MemoryStream stream = new(input.Skip(offset)
                                                 .Take(size)
                                                 .ToArray());
            offset += size;
            stream.Position = 0;

            AddInDefinition current = deserializer.Deserialize(stream: stream);
            result[i] = current;
        }

        return result;
    }
}

// ISerializationStrategy<T>
partial class __AddInDefinitionSerializationStrategy : ISerializationStrategy<Byte[]>
{
    Byte[] ISerializationStrategy<Byte[]>.Serialize(Object? input) =>
        this.Serialize((AddInDefinition[])input!);
}

// ISerializationStrategy<T, U>
partial class __AddInDefinitionSerializationStrategy : ISerializationStrategy<Byte[], AddInDefinition[]>
{
    public Byte[] Serialize(AddInDefinition[] input)
    {
        List<Byte> result = new();

        IByteSerializer<AddInDefinition> serializer = CreateByteSerializer
                                                     .ForSerialization()
                                                     .ConfigureForOwnedType<AddInDefinition>()
                                                     .UseDefaultStrategies()
                                                     .Construct();

        result.AddRange(BitConverter.GetBytes(input.Length));
        for (Int32 i = 0;
             i < input.Length;
             i++)
        {
            using MemoryStream stream = new();
            serializer.Serialize(stream: stream,
                                 graph: input[i]);
            stream.Position = 0;
            result.AddRange(BitConverter.GetBytes((Int32)stream.Length));
            result.AddRange(stream.ToArray());
        }

        return result.ToArray();
    }
}