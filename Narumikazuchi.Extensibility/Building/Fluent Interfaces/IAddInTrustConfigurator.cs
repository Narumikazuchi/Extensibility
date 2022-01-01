namespace Narumikazuchi.Extensibility;

/// <summary>
/// Configures the trust level of the store.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAddInTrustConfigurator
{
    /// <summary>
    /// The store will only trust system-trusted AddIns, which are a fixed set of programmatically provided AddIns.
    /// </summary>
    /// <remarks>
    /// While there are certainly ways around it, this trust level is designed to not be dynamically
    /// influenceable by an end user. You might want to implement a way for administrators to easily alter the list,
    /// but it is highly discouraged to allow an end user such freedom.
    /// </remarks>
    public IAddInNotSystemTrustedConfigurator TrustProvidedAddInsOnly();

    /// <summary>
    /// The store will only trust user-trusted AddIns, which are a dynamic set of AddIns that are either dynamically loaded
    /// or manually approved by an end user.
    /// </summary>
    public IAddInNotUserTrustedConfigurator TrustUserApprovedAddInsOnly();

    /// <summary>
    /// The store will trust user-trusted AddIns as well as system-trusted AddIns.<para/>
    /// System-trusted AddIns are are a fixed set of programmatically provided AddIns.<para/>
    /// User-trusted AddIns are a dynamic set of AddIns that are either dynamically loaded
    /// or manually approved by an end user.
    /// </summary>
    public IAddInNotBothTrustedConfigurator TrustBothProvidedAndUserApprovedAddIns();

    /// <summary>
    /// The store will trust any AddIn, regardless of it's origin.
    /// </summary>
    /// <remarks>
    /// This option is great if you are building an application where you are certain that only a controlled set of AddIns will be loaded.
    /// </remarks>
    public IAddInTrustFinalizer TrustAllAddIns();
}