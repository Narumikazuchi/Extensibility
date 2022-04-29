namespace Narumikazuchi.Extensibility;

/// <summary>
/// A builder class to build an <see cref="IAddInStore"/> for your application.
/// </summary>
public static class CreateAddInStore
{
    /// <summary>
    /// The store will only trust system-trusted AddIns, which are a fixed set of programmatically provided AddIns.
    /// </summary>
    /// <remarks>
    /// While there are certainly ways around it, this trust level is designed to not be dynamically
    /// influenceable by an end user. You might want to implement a way for administrators to easily alter the list,
    /// but it is highly discouraged to allow an end user such freedom.
    /// </remarks>
    public static IAddInNotSystemTrustedConfigurator TrustProvidedAddInsOnly() =>
        new __ConfigurationInfo(__TrustLevel.TRUSTED_ONLY);

    /// <summary>
    /// The store will only trust user-trusted AddIns, which are a dynamic set of AddIns that are either dynamically loaded
    /// or manually approved by an end user.
    /// </summary>
    public static IAddInNotUserTrustedConfigurator TrustUserApprovedAddInsOnly() =>
        new __ConfigurationInfo(__TrustLevel.USER_CONFIRMED_ONLY);

    /// <summary>
    /// The store will trust user-trusted AddIns as well as system-trusted AddIns.<para/>
    /// System-trusted AddIns are are a fixed set of programmatically provided AddIns.<para/>
    /// User-trusted AddIns are a dynamic set of AddIns that are either dynamically loaded
    /// or manually approved by an end user.
    /// </summary>
    public static IAddInNotBothTrustedConfigurator TrustBothProvidedAndUserApprovedAddIns() =>
        new __ConfigurationInfo(__TrustLevel.TRUSTED_AND_USER_CONFIRMED);

    /// <summary>
    /// The store will trust any AddIn, regardless of it's origin.
    /// </summary>
    /// <remarks>
    /// This option is great if you are building an application where you are certain that only a controlled set of AddIns will be loaded.
    /// </remarks>
    public static IAddInTrustFinalizer TrustAllAddIns() =>
        new __ConfigurationInfo(__TrustLevel.ALL);
}