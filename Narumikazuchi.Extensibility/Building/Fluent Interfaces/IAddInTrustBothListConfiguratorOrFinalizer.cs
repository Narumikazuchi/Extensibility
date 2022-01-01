namespace Narumikazuchi.Extensibility;

/// <summary>
/// Configures which AddIns should be trusted or finalizes the configuration.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAddInTrustBothListConfiguratorOrFinalizer :
    IAddInTrustFinalizer,
    IAddInTrustBothListConfigurator
{ }