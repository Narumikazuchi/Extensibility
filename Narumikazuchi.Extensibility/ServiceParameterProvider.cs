namespace Narumikazuchi.Extensibility;

public sealed class ServiceParameterProvider : AddInParameterProvider
{
    public ServiceParameterProvider(IServiceProvider serviceProvider)
    {
        m_ServiceProvider = serviceProvider;
    }

    public override void AddParameter([DisallowNull] Type parameterType,
                                      [DisallowNull] Object parameter)
    { }
    public override void AddParameter([DisallowNull] Type parameterType,
                                      [DisallowNull] Func<AddInParameterProvider, Object> factory)
    { }

    public override Option<Object> GetParameter([DisallowNull] Type parameterType) =>
        m_ServiceProvider.GetService(parameterType);

    public override void RemoveParameter([DisallowNull] Type parameterType,
                                         [DisallowNull] Object parameter)
    { }

    public override void RemoveParameters([DisallowNull] Type parameterType)
    { }

    private readonly IServiceProvider m_ServiceProvider;
}
