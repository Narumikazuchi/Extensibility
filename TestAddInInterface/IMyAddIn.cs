namespace TestAddInInterface;

public interface IMyAddIn :
    Narumikazuchi.Extensibility.IAddIn
{
    public void MyFunctionality(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext context);
}

public interface IMyAddIn<TSelf> : 
    Narumikazuchi.Extensibility.IAddIn<TSelf>,
    IMyAddIn
        where TSelf : IMyAddIn<TSelf>
{ }