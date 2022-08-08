namespace Narumikazuchi.Extensibility;

public sealed class AddInDiscoveryFailedEventArgs : EventArgs
{
    public AddInDiscoveryFailedEventArgs(DiscoveryFailedReason reason)
    {
        this.Reason = reason;
    }

    public DiscoveryFailedReason Reason { get; }
}