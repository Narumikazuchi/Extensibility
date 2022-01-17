namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInStore"/> to activate AddIns.
/// </summary>
public interface IAddInActivator
{
    /// <summary>
    /// Checks whether the AddIn that is described by the specified <see cref="IAddInDefinition"/> can be activated in the <see cref="IAddInStore"/> with or without user prompt.
    /// </summary>
    /// <remarks>
    /// If the trust level of this <see cref="IAddInStore"/> has been configured for only system-trusted,
    /// the method will ingore the possible result of user prompts and return <see langword="false"/> for AddIns that are not trusted.
    /// </remarks>
    /// <param name="addIn">The definition which describes the AddIn to register.</param>
    /// <returns><see langword="true"/> if the AddIn can be registered with or without user prompt; otherwise, <see langword="false"/></returns>
    public Boolean CanAddInBeActivated([DisallowNull] IAddInDefinition addIn);
    /// <summary>
    /// Gets whether the AddIn that is described by the specified <see cref="Guid"/> can be activated in the <see cref="IAddInStore"/> with or without user prompt.
    /// </summary>
    /// <remarks>
    /// If the trust level of this <see cref="IAddInStore"/> has been configured for only system-trusted,
    /// the method will ingore the possible result of user prompts and return <see langword="false"/> for AddIns that are not trusted.
    /// </remarks>
    /// <param name="addInUniqueIdentifier">The guid of the AddIn to register.</param>
    /// <returns><see langword="true"/> if the AddIn can be registered with or without user prompt; otherwise, <see langword="false"/></returns>
    public Boolean CanAddInBeActivated(in Guid addInUniqueIdentifier);

    /// <summary>
    /// Checks whether the AddIn which is described by the specified <see cref="IAddInDefinition"/> is currently active.
    /// </summary>
    /// <remarks>
    /// This method also returns <see langword="false"/> when the AddIn in question is not even registered yet.
    /// </remarks>
    /// <param name="addIn">The definition which the describes the AddIn to check.</param>
    /// <returns><see langword="true"/> if the AddIn is currently active; otherwise, <see langword="false"/></returns>
    public Boolean IsAddInActive([DisallowNull] IAddInDefinition addIn);

    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="IAddInDefinition"/> and returns an instance of <see cref="IAddIn"/> that can be casted to your custom AddIn type.
    /// </summary>
    /// <param name="definition">The definition which describes the AddIn to activate.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate([DisallowNull] IAddInDefinition definition,
                               [NotNullWhen(true)] out IAddIn? addIn);
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="IAddInDefinition"/> and returns an instance of <see cref="IAddIn"/> that can be casted to your custom AddIn type.
    /// </summary>
    /// <param name="definition">The definition which describes the AddIn to activate.</param>
    /// <param name="options">The options that are needed to activate that AddIn.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate<TConfigurationOptions>([DisallowNull] IAddInDefinition definition,
                                                      [AllowNull] TConfigurationOptions? options,
                                                      [NotNullWhen(true)] out IAddIn? addIn);
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="IAddInDefinition"/> and returns an instance of <see cref="IAddIn"/> that can be casted to your custom AddIn type.
    /// </summary>
    /// <param name="definition">The definition which describes the AddIn to activate.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate<TAddIn>([DisallowNull] IAddInDefinition definition,
                                       [NotNullWhen(true)] out TAddIn? addIn)
        where TAddIn : IAddIn<TAddIn>;
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="IAddInDefinition"/> and returns an instance of <see cref="IAddIn"/> that can be casted to your custom AddIn type.
    /// </summary>
    /// <param name="definition">The definition which describes the AddIn to activate.</param>
    /// <param name="options">The options that are needed to activate that AddIn.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate<TAddIn, TConfigurationOptions>([DisallowNull] IAddInDefinition definition,
                                                              [AllowNull] TConfigurationOptions? options,
                                                              [NotNullWhen(true)] out TAddIn? addIn)
        where TAddIn : IAddIn<TAddIn, TConfigurationOptions>;
}