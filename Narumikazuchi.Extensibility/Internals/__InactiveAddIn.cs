namespace Narumikazuchi.Extensibility;

internal struct __InactiveAddIn : IAddIn
{
    public void Shutdown()
    {
        this.ShutdownInitiated?.Invoke(this, EventArgs.Empty);
        this._isShuttingDown = true;
        this.SilentShutdown();
        this._isShuttingDown = false;
        this.ShutdownFinished?.Invoke(this, EventArgs.Empty);
    }

    public void SilentShutdown()
    { }

    public event EventHandler<IAddIn>? ShutdownInitiated;
    public event EventHandler<IAddIn>? ShutdownFinished;

    public Boolean IsShuttingDown => 
        this._isShuttingDown;

    private Boolean _isShuttingDown;
}