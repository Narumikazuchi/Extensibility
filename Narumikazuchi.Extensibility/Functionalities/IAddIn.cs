namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the very basic functionality that an AddIn needs to implement for the <see cref="IAddInStore"/> to use it in a non-generic context.<para/>
/// If you are building an AddIn for the <see cref="IAddInStore"/> be sure to not only implement this interface but also either the <see cref="IAddIn{TSelf}"/>
/// or <see cref="IAddIn{TSelf, TOptions}"/> interface. If you don't implement either the <see cref="IAddInStore"/> can't register your AddIn.
/// </summary>
/// <remarks>
/// If create your own AddIn <see langword="interface"/>, <see langword="class"/> or <see langword="struct"/> 
/// inherit from either <see cref="IAddIn{TSelf}"/> or <see cref="IAddIn{TSelf, TOptions}"/>, since
/// you won't be able to register them in the store otherwise.
/// </remarks>
public interface IAddIn
{
    /// <summary>
    /// Decouples every refence this instance holds and prepares it so the <see cref="IAddInStore"/> it's
    /// registred in can delete the reference to this instance.
    /// </summary>
    /// <remarks>
    /// This method will raise both the <see cref="ShutdownInitiated"/> and <see cref="ShutdownFinished"/> event, 
    /// unlike the <see cref="SilentShutdown()"/> method.
    /// </remarks>
    public void Shutdown();

    /// <summary>
    /// Decouples every refence this instance holds and prepares it so the <see cref="IAddInStore"/> it's
    /// registred in can delete the reference to this instance.
    /// </summary>
    /// <remarks>
    /// This method will NOT raise either the <see cref="ShutdownInitiated"/> or <see cref="ShutdownFinished"/> event, 
    /// unlike the <see cref="Shutdown()"/> method.
    /// </remarks>
    public void SilentShutdown();

    /// <summary>
    /// Occurs when the instance gets prepared to be deleted from the <see cref="IAddInStore"/>.
    /// </summary>
    public event EventHandler<IAddIn>? ShutdownInitiated;
    /// <summary>
    /// Occurs right before the reference to the instance will get deleted from the <see cref="IAddInStore"/>.
    /// </summary>
    public event EventHandler<IAddIn>? ShutdownFinished;

    /// <summary>
    /// Gets whether the <see cref="IAddIn"/> is currently shutting down.
    /// </summary>
    public Boolean IsShuttingDown { get; }
}

/// <summary>
/// Represents the basic functionality that an AddIn needs to implement.
/// </summary>
/// <remarks>
/// Remember to include &lt;PropertyGroup&gt; &lt;EnableDynamicLoading&gt;true&lt;/EnableDynamicLoading&gt; &lt;/PropertyGroup&gt;
/// into the project file of your AddIn projects, since it can otherwise be difficult to load AddIns.
/// </remarks>
public interface IAddIn<TSelf> :
    IAddIn
        where TSelf : IAddIn<TSelf>
{
    /// <summary>
    /// Initializes an instance of this object.
    /// </summary>
    /// <remarks>
    /// This method will get called from the <see cref="IAddInStore"/> to get a reference to this AddIn.
    /// </remarks>
    /// <returns>A reference to this AddIn</returns>
    internal protected static abstract TSelf Activate();
}

/// <summary>
/// Represents the basic functionality that an AddIn needs to implement.
/// </summary>
/// <remarks>
/// Remember to include &lt;PropertyGroup&gt; &lt;EnableDynamicLoading&gt;true&lt;/EnableDynamicLoading&gt; &lt;/PropertyGroup&gt;
/// into the project file of your AddIn projects, since it can otherwise be difficult to load AddIns.
/// </remarks>
public interface IAddIn<TSelf, TOptions> : 
    IAddIn
        where TSelf : IAddIn<TSelf, TOptions>
{
    /// <summary>
    /// Initializes an instance of this object.
    /// </summary>
    /// <remarks>
    /// This method will get called from the <see cref="IAddInStore"/> to get a reference to this AddIn.
    /// </remarks>
    /// <param name="options">The options used to configure this AddIn for activation.</param>
    /// <returns>A reference to this AddIn</returns>
    internal protected static abstract TSelf Activate(TOptions options);
}