using Microsoft.VisualStudio.TestTools.UnitTesting;
using Narumikazuchi.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAddInInterface;

namespace UnitTest;

[AddIn("InternalIndirectAddInTest", "DB942E7E-CDF3-441B-B8A9-16D7A0B07A37", "2.0.1")]
public partial class InternalIndirectAddInTest : IMyAddIn<InternalIndirectAddInTest>
{
    public void MyFunctionality(TestContext context)
    {
        context.WriteLine("My functionality happened!");
    }

    public static readonly Guid GUID = Guid.Parse("DB942E7E-CDF3-441B-B8A9-16D7A0B07A37");
    public static readonly Version MyVersion = new(2, 0, 1);
}

partial class InternalIndirectAddInTest : IAddIn<InternalIndirectAddInTest>
{
    public void Shutdown()
    {
        this.ShutdownInitiated?
            .Invoke(sender: this,
                    eventArgs: EventArgs.Empty);
        this.SilentShutdown();
        this.ShutdownFinished?
            .Invoke(sender: this,
                    eventArgs: EventArgs.Empty);
    }
    public void SilentShutdown()
    { }

    public static InternalIndirectAddInTest Activate() =>
        new();

    public event Narumikazuchi.EventHandler<IAddIn>? ShutdownInitiated;
    public event Narumikazuchi.EventHandler<IAddIn>? ShutdownFinished;
}