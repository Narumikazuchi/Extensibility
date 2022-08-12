namespace Narumikazuchi.Extensibility;

[DebuggerDisplay("{Reason}")]
public sealed class AddInDiscoveryFailedEventArgs : EventArgs
{
    public AddInDiscoveryFailedEventArgs(DiscoveryFailedReason reason)
    {
        this.Reason = reason;
    }

    public DiscoveryFailedReason Reason { get; }
}