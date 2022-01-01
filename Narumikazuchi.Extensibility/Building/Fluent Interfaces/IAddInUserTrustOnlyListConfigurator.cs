namespace Narumikazuchi.Extensibility;

/// <summary>
/// Configures which AddIns should be trusted.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAddInUserTrustOnlyListConfigurator
{
    /// <summary>
    /// Provides the store with a list of user-trusted AddIns.
    /// </summary>
    /// <param name="userTrusted">The list of AddIns that the store will perceive as user-trusted.</param>
    public IAddInUserTrustOnlyListConfiguratorOrFinalizer ProvidingUserTrustedAddIns(IEnumerable<Guid> userTrusted);
}