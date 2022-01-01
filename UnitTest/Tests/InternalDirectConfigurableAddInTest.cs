using Narumikazuchi.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAddInInterface;

namespace UnitTest;

[AddIn("InternalDirectConfigurableAddInTest", "BCBA0DB3-CD8E-4C21-BE1B-8F147B29F1F4", "2.0.1")]
public class InternalDirectConfigurableAddInTest : IAddIn<InternalDirectConfigurableAddInTest, Configuration>
{
    public InternalDirectConfigurableAddInTest(Configuration configuration)
    {
        this.Configuration = configuration;
    }

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

    public static InternalDirectConfigurableAddInTest Activate(Configuration configuration) => 
        new(configuration);

    public Configuration Configuration { get; }

    public event Narumikazuchi.EventHandler<IAddIn>? ShutdownInitiated;
    public event Narumikazuchi.EventHandler<IAddIn>? ShutdownFinished;

    public static readonly Guid GUID = Guid.Parse("BCBA0DB3-CD8E-4C21-BE1B-8F147B29F1F4");
    public static readonly Version MyVersion = new(2, 0, 1);
}