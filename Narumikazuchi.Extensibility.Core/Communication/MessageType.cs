namespace Narumikazuchi.Extensibility
{
    internal enum MessageType : System.Byte
    {
        ErrorOrInvalid = 0x00,
        InvokeMethodVoid = 0x01,
        InvokeMethodReturn = 0x02,
        InvokePropertyGet = 0x03,
        InvokePropertySet = 0x04,
        InvokeIndexerGet = 0x05,
        InvokeIndexerSet = 0x06,
        InvocationResult = 0x07,
        GetExposedMembers = 0x10,
        ExposedMemberInformation = 0x11,
        CrashNotification = 0x12,
        ShutdownCommand = 0x13
    }
}
