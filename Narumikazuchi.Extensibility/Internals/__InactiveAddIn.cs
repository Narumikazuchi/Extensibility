namespace Narumikazuchi.Extensibility;

internal struct __InactiveAddIn : IAddIn
{
    public void Shutdown()
    {
        this.ShutdownInitiated?.Invoke(this, EventArgs.Empty);
        m_IsShuttingDown = true;
        this.SilentShutdown();
        m_IsShuttingDown = false;
        this.ShutdownFinished?.Invoke(this, EventArgs.Empty);
    }

    public void SilentShutdown()
    { }

    public event EventHandler<IAddIn>? ShutdownInitiated;
    public event EventHandler<IAddIn>? ShutdownFinished;

    public Boolean IsShuttingDown => 
        m_IsShuttingDown;

    private Boolean m_IsShuttingDown;
}