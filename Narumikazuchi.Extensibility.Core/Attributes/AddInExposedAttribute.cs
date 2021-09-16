using System;

namespace Narumikazuchi.Extensibility
{
    /// <summary>
    /// Marks this member as exposed. An exposed member will be available through the <see cref="IAddInController"/> interface.
    /// </summary>
    /// <remarks>
    /// All types that are used in the signature of the member are required to be either a primitive (<see cref="Boolean"/>, <see cref="Byte"/>, <see cref="Char"/>, <see cref="Double"/>, 
    /// <see cref="Int16"/>, <see cref="Int32"/>, <see cref="Int64"/>, <see cref="IntPtr"/>, <see cref="SByte"/>, <see cref="Single"/>, <see cref="UInt16"/>, <see cref="UInt32"/>, 
    /// <see cref="UInt64"/>, <see cref="UIntPtr"/>), a <see cref="String"/> or a <see cref="Serialization.Bytes.IByteSerializable"/>.
    /// Method parameters cannot be <see langword="ref"/>, <see langword="in"/> or <see langword="out"/>.
    /// Members that do not fulfill these requirements can be marked with this attribute, but will be ingored when exposing them to the <see cref="IAddInController"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class AddInExposedAttribute : Attribute
    { }
}
