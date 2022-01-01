namespace Narumikazuchi.Extensibility;

/// <summary>
/// Configures which AddIns should be trusted.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAddInSystemTrustOnlyListConfigurator
{
    /// <summary>
    /// Provides the store with a list of system-trusted AddIns.
    /// </summary>
    /// <param name="systemTrusted">The list of AddIns that the store will trust.</param>
    public IAddInSystemTrustOnlyListConfiguratorOrFinalizer ProvidingSystemTrustedAddIns(IEnumerable<Guid> systemTrusted);
}