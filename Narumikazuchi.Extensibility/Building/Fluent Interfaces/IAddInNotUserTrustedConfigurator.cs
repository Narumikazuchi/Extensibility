namespace Narumikazuchi.Extensibility;

/// <summary>
/// Configures the action the store shall take, when the application tries to register a non-trusted AddIn.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAddInNotUserTrustedConfigurator
{
    /// <summary>
    /// When the application tries to register an AddIn that is not in the supplied "System-Trusted"-list, 
    /// it will critically fail by throwing an exception.
    /// </summary>
    public IAddInUserTrustOnlyListConfigurator PromptUserWhenNotUserTrusted([DisallowNull] Func<IAddInDefinition, Boolean> userPrompt);

    /// <summary>
    /// When the application tries to register an AddIn that is not in the supplied "User-Trusted"-list, 
    /// it will basically silently fail the registration and continue on.
    /// </summary>
    public IAddInUserTrustOnlyListConfigurator IgnoreWhenNotUserTrusted();

    /// <summary>
    /// When the application tries to register an AddIn that is not in the supplied "User-Trusted"-list, 
    /// it will critically fail by throwing an exception.
    /// </summary>
    public IAddInUserTrustOnlyListConfigurator FailWhenNotUserTrusted();
}