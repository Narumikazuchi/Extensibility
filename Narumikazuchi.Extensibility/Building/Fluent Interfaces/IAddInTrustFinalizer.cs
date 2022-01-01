namespace Narumikazuchi.Extensibility;

/// <summary>
/// Finalizes the configuration.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAddInTrustFinalizer
{
    /// <summary>
    /// Finalizes the configuration and returns an instance of the fully configured <see cref="IAddInStore"/>.
    /// </summary>
    public IAddInStore Construct();
}