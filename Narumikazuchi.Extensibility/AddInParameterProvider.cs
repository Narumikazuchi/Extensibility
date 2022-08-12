namespace Narumikazuchi.Extensibility;

public abstract class AddInParameterProvider
{
    protected AddInParameterProvider()
    { }

    public abstract void AddParameter([DisallowNull] Type parameterType,
                                      [DisallowNull] Object parameter);

    public abstract void AddParameter([DisallowNull] Type parameterType,
                                      [DisallowNull] Func<AddInParameterProvider, Object> factory);

    public abstract void RemoveParameters([DisallowNull] Type parameterType);

    public abstract void RemoveParameter([DisallowNull] Type parameterType,
                                         [DisallowNull] Object parameter);

    public abstract Option<Object> GetParameter([DisallowNull] Type parameterType);
}