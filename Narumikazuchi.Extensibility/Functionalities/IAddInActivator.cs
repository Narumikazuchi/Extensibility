namespace Narumikazuchi.Extensibility;

/// <summary>
/// Represents the functionality of an <see cref="IAddInStore"/> to activate AddIns.
/// </summary>
public interface IAddInActivator
{
    /// <summary>
    /// Checks whether the AddIn which is described by the specified <see cref="AddInDefinition"/> is currently active.
    /// </summary>
    /// <remarks>
    /// This method also returns <see langword="false"/> when the AddIn in question is not even registered yet.
    /// </remarks>
    /// <param name="addIn">The definition which the describes the AddIn to check.</param>
    /// <returns><see langword="true"/> if the AddIn is currently active; otherwise, <see langword="false"/></returns>
    public Boolean IsAddInActive(in AddInDefinition addIn);

    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="AddInDefinition"/> and returns an instance of <see cref="IAddIn"/> that can be casted to your custom AddIn type.
    /// </summary>
    /// <param name="definition">The definition which describes the AddIn to activate.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate(in AddInDefinition definition,
                               [DisallowNull] out IAddIn addIn);
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="AddInDefinition"/> and returns an instance of <see cref="IAddIn"/> that can be casted to your custom AddIn type.
    /// </summary>
    /// <param name="guid">The guid of the AddIn to activate.</param>
    /// <param name="version">The version of the AddIn to activate.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate(in Guid guid,
                               in AlphanumericVersion version,
                               [DisallowNull] out IAddIn addIn);
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="AddInDefinition"/> and returns an instance of the specified <typeparamref name="TAddIn"/>.
    /// </summary>
    /// <param name="definition">The definition which describes the AddIn to activate.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate<TAddIn>(in AddInDefinition definition,
                                       [DisallowNull] out TAddIn addIn)
        where TAddIn : IAddIn<TAddIn>;
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="AddInDefinition"/> and returns an instance of the specified <typeparamref name="TAddIn"/>.
    /// </summary>
    /// <param name="guid">The guid of the AddIn to activate.</param>
    /// <param name="version">The version of the AddIn to activate.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate<TAddIn>(in Guid guid,
                                       in AlphanumericVersion version,
                                       [DisallowNull] out TAddIn addIn)
        where TAddIn : IAddIn<TAddIn>;
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="AddInDefinition"/> and returns an instance of <see cref="IAddIn"/> that can be casted to your custom AddIn type.
    /// </summary>
    /// <param name="definition">The definition which describes the AddIn to activate.</param>
    /// <param name="options">The options that are needed to activate that AddIn.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate<TConfigurationOptions>(in AddInDefinition definition,
                                                      [AllowNull] TConfigurationOptions? options,
                                                      [DisallowNull] out IAddIn addIn);
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="AddInDefinition"/> and returns an instance of <see cref="IAddIn"/> that can be casted to your custom AddIn type.
    /// </summary>
    /// <param name="guid">The guid of the AddIn to activate.</param>
    /// <param name="version">The version of the AddIn to activate.</param>
    /// <param name="options">The options that are needed to activate that AddIn.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate<TConfigurationOptions>(in Guid guid,
                                                      in AlphanumericVersion version,
                                                      [AllowNull] TConfigurationOptions? options,
                                                      [DisallowNull] out IAddIn addIn);
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="AddInDefinition"/> and returns an instance of the specified <typeparamref name="TAddIn"/>.
    /// </summary>
    /// <param name="definition">The definition which describes the AddIn to activate.</param>
    /// <param name="options">The options that are needed to activate that AddIn.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate<TAddIn, TConfigurationOptions>(in AddInDefinition definition,
                                                              [AllowNull] TConfigurationOptions? options,
                                                              [DisallowNull] out TAddIn addIn)
        where TAddIn : IAddIn<TAddIn, TConfigurationOptions>;
    /// <summary>
    /// Tries to activate the AddIn that is described by the specified <see cref="AddInDefinition"/> and returns an instance of the specified <typeparamref name="TAddIn"/>.
    /// </summary>
    /// <param name="guid">The guid of the AddIn to activate.</param>
    /// <param name="version">The version of the AddIn to activate.</param>
    /// <param name="options">The options that are needed to activate that AddIn.</param>
    /// <param name="addIn">The resulting instance of the AddIn.</param>
    /// <returns><see langword="true"/> if the AddIn was not previously activated; otherwise, <see langword="false"/></returns>
    /// <exception cref="InvalidCastException"/>
    /// <exception cref="NotAllowed"/>
    /// <exception cref="NullReferenceException"/>
    public Boolean TryActivate<TAddIn, TConfigurationOptions>(in Guid guid,
                                                              in AlphanumericVersion version,
                                                              [AllowNull] TConfigurationOptions? options,
                                                              [DisallowNull] out TAddIn addIn)
        where TAddIn : IAddIn<TAddIn, TConfigurationOptions>;
}