using Microsoft.VisualStudio.TestTools.UnitTesting;
using Narumikazuchi.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAddInInterface;

namespace TestAddInLib;

[AddIn("ExternalIndirectAddInTest", "289E2974-AADF-4880-AA3B-452EBE953F67", "5.4.3.2")]
public partial class ExternalIndirectAddInTest : IMyAddIn<ExternalIndirectAddInTest>
{
    public void MyFunctionality(TestContext context)
    {
        context.WriteLine("My functionality happened!");
        Narumikazuchi.Networking.MacAddress mac = default;
        context.WriteLine(mac.ToString());
    }

    public String Name { get; } = "FooBar";
    public String Description { get; } = "Lorem ipsum colorem";
    public Double Rate { get; } = 0.75d;
    public Int32 Count { get; } = 17;

    public static readonly Guid GUID = Guid.Parse("289E2974-AADF-4880-AA3B-452EBE953F67");
    public static readonly Version MyVersion = new(5, 4, 3, 2);
}

partial class ExternalIndirectAddInTest : IAddIn<ExternalIndirectAddInTest>
{
    void IAddIn.Shutdown()
    {
        this._init?
            .Invoke(sender: this,
                    eventArgs: EventArgs.Empty);
        ((IAddIn)this).SilentShutdown();
        this._finish?
            .Invoke(sender: this,
                    eventArgs: EventArgs.Empty);
    }
    void IAddIn.SilentShutdown()
    { }

    public static ExternalIndirectAddInTest Activate() =>
        new();

    event Narumikazuchi.EventHandler<IAddIn>? IAddIn.ShutdownInitiated
    {
        add => this._init += value;
        remove => this._init -= value;
    }
    event Narumikazuchi.EventHandler<IAddIn>? IAddIn.ShutdownFinished
    {
        add => this._finish += value;
        remove => this._finish -= value;
    }

    private Narumikazuchi.EventHandler<IAddIn>? _init = null;
    private Narumikazuchi.EventHandler<IAddIn>? _finish = null;
}