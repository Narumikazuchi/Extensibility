using Narumikazuchi.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAddInLib;

[AddIn("ExternalDirectAddInTest", "8849A7A0-2F62-41BB-B687-F4BAF0340E9B", "1.0")]
public class ExternalDirectAddInTest : IAddIn<ExternalDirectAddInTest>
{
    public ExternalDirectAddInTest()
    { }

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

    public static ExternalDirectAddInTest Activate() =>
        new();

    public event Narumikazuchi.EventHandler<IAddIn>? ShutdownInitiated;
    public event Narumikazuchi.EventHandler<IAddIn>? ShutdownFinished;

    public String Name { get; } = "FooBar";
    public String Description { get; } = "Lorem ipsum colorem";
    public Double Rate { get; } = 0.75d;
    public Int32 Count { get; } = 17;

    public static readonly Guid GUID = Guid.Parse("8849A7A0-2F62-41BB-B687-F4BAF0340E9B");
    public static readonly Version MyVersion = new(1, 0);
}
