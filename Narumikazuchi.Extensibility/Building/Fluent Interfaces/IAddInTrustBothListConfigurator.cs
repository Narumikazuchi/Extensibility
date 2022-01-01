namespace Narumikazuchi.Extensibility;

/// <summary>
/// Configures which AddIns should be trusted.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAddInTrustBothListConfigurator
{
    /// <summary>
    /// Provides the store with a list of system-trusted AddIns.
    /// </summary>
    /// <param name="systemTrusted">The list of AddIns that the store will trust.</param>
    public IAddInTrustBothListConfiguratorOrFinalizer ProvidingSystemTrustedAddIns(IEnumerable<Guid> systemTrusted);

    /// <summary>
    /// Provides the store with a list of user-trusted AddIns.
    /// </summary>
    /// <param name="userTrusted">The list of AddIns that the store will perceive as user-trusted.</param>
    public IAddInTrustBothListConfiguratorOrFinalizer ProvidingUserTrustedAddIns(IEnumerable<Guid> userTrusted);
}