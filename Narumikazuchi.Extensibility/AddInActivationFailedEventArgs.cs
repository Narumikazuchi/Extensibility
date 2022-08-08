namespace Narumikazuchi.Extensibility;

public sealed class AddInActivationFailedEventArgs : EventArgs
{
    public AddInActivationFailedEventArgs(ActivationFailReason reason)
    {
        if (reason is ActivationFailReason.Exception
                   or ActivationFailReason.NotAllParametersServed)
        {
            // Not allowed
            throw new ArgumentException();
        }

        this.Reason = reason;
        this.Exceptions = ReadOnlyList<Exception>.Create();
    }
    public AddInActivationFailedEventArgs(ActivationFailReason reason,
                                          Exception exception)
    {
        if (reason is not ActivationFailReason.Exception)
        {
            // Not allowed
            throw new ArgumentException();
        }

        this.Reason = reason;
        this.Exceptions = ReadOnlyList<Exception>.CreateFrom(new Exception[] { exception });
    }
    public AddInActivationFailedEventArgs(ActivationFailReason reason,
                                          List<Exception> exceptions)
    {
        if (reason is not ActivationFailReason.NotAllParametersServed)
        {
            // Not allowed
            throw new ArgumentException();
        }

        this.Reason = reason;
        this.Exceptions = ReadOnlyList<Exception>.CreateFrom(exceptions);
    }

    public ReadOnlyList<Exception> Exceptions { get; }

    public ActivationFailReason Reason { get; }
}