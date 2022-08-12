namespace Narumikazuchi.Extensibility;

public static class AddInParameterProviderExtensions
{
    public static void AddParameter<TParameter>(this AddInParameterProvider parameterProvider,
                                                [DisallowNull] TParameter parameter)
        where TParameter : class =>
            parameterProvider.AddParameter(parameterType: typeof(TParameter),
                                           parameter: parameter);
    public static void AddParameter<TParameter>(this AddInParameterProvider parameterProvider,
                                                [DisallowNull] Func<AddInParameterProvider, TParameter> factory)
        where TParameter : class =>
            parameterProvider.AddParameter(parameterType: typeof(TParameter),
                                           factory: factory);
    public static void RemoveParameter<TParameter>(this AddInParameterProvider parameterProvider,
                                                   [DisallowNull] TParameter parameter)
        where TParameter : class =>
            parameterProvider.RemoveParameter(parameterType: typeof(TParameter),
                                              parameter: parameter);
    public static void RemoveParameters<TParameter>(this AddInParameterProvider parameterProvider)
        where TParameter : class =>
            parameterProvider.RemoveParameters(parameterType: typeof(TParameter));
    public static Option<TParameter> GetParameter<TParameter>(this AddInParameterProvider parameterProvider)
        where TParameter : class =>
            (TParameter?)parameterProvider.GetParameter(parameterType: typeof(TParameter));
}