namespace Narumikazuchi.Extensibility;

public enum ActivationFailReason
{
    Unknown = 0,
    NotDiscovered,
    AssemblyNotLoaded,
    AlreadyRunning,
    NotRunning,
    InvalidCast,
    NoPublicConstructor,
    NotAllParametersServed,
    Exception
}