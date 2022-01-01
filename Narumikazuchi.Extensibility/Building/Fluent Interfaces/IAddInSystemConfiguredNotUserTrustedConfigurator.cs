namespace Narumikazuchi.Extensibility;

/// <summary>
/// Configures the action the store shall take, when the application tries to register a non-trusted AddIn.
/// </summary>
public interface IAddInSystemConfiguredNotUserTrustedConfigurator
{
    /// <summary>
    /// When the application tries to register an AddIn that is not in the supplied "User-Trusted"-list, 
    /// it will critically fail by throwing an exception.
    /// </summary>
    public IAddInTrustBothListConfigurator PromptUserWhenNotUserTrusted([DisallowNull] Func<AddInDefinition, Boolean> userPrompt);

    /// <summary>
    /// When the application tries to register an AddIn that is not in the supplied "User-Trusted"-list, 
    /// it will basically silently fail the registration and continue on.
    /// </summary>
    public IAddInTrustBothListConfigurator IgnoreWhenNotUserTrusted();

    /// <summary>
    /// When the application tries to register an AddIn that is not in the supplied "User-Trusted"-list, 
    /// it will critically fail by throwing an exception.
    /// </summary>
    public IAddInTrustBothListConfigurator FailWhenNotUserTrusted();
}