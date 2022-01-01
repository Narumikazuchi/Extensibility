namespace Narumikazuchi.Extensibility;

internal struct __InactiveAddIn : IAddIn
{
    public void Shutdown()
    {
        this.ShutdownInitiated?.Invoke(this, EventArgs.Empty);
        this.SilentShutdown();
        this.ShutdownFinished?.Invoke(this, EventArgs.Empty);
    }

    public void SilentShutdown()
    { }

    public event EventHandler<IAddIn>? ShutdownInitiated;
    public event EventHandler<IAddIn>? ShutdownFinished;
}