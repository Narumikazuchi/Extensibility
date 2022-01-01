namespace Narumikazuchi.Extensibility;

/// <summary>
/// Configures the action the store shall take, when the application tries to register a non-trusted AddIn.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAddInNotSystemTrustedConfigurator
{
    /// <summary>
    /// When the application tries to register an AddIn that is not in the supplied "System-Trusted"-list, 
    /// it notifies the user through the provided delegate about the inability to register the AddIn.
    /// </summary>
    /// <remarks>
    /// The provided delegate can be used to either inform the user about the failed action, log the failed action or
    /// do both or anything else that you need to happend after such an event.
    /// </remarks>
    /// <param name="notification">The delegate to call when the described event happens.</param>
    public IAddInSystemTrustOnlyListConfigurator NotifyUserWhenNotSystemTrusted([DisallowNull] Action<AddInDefinition> notification);

    /// <summary>
    /// When the application tries to register an AddIn that is not in the supplied "System-Trusted"-list, 
    /// it will basically silently fail the registration and continue on.
    /// </summary>
    public IAddInSystemTrustOnlyListConfigurator IgnoreWhenNotSystemTrusted();

    /// <summary>
    /// When the application tries to register an AddIn that is not in the supplied "System-Trusted"-list, 
    /// it will critically fail by throwing an exception.
    /// </summary>
    public IAddInSystemTrustOnlyListConfigurator FailWhenNotSystemTrusted();
}