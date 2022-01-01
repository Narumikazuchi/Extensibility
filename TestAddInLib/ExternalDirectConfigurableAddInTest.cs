using Narumikazuchi.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAddInInterface;

namespace TestAddInLib;

[AddIn("ConfigurableTestAddIn", "15D432FD-C336-49BD-BC50-DE80BE57CE51", "1.2")]
public class ExternalDirectConfigurableAddInTest : IAddIn<ExternalDirectConfigurableAddInTest, Configuration>
{
    public ExternalDirectConfigurableAddInTest(Configuration configuration)
    {
        this.Name = configuration.Name;
        this.Description = configuration.Description;
        this.Rate = configuration.Rate;
        this.Count = configuration.Count;
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

    public static ExternalDirectConfigurableAddInTest Activate(Configuration configuration) =>
        new(configuration);

    public event Narumikazuchi.EventHandler<IAddIn>? ShutdownInitiated;
    public event Narumikazuchi.EventHandler<IAddIn>? ShutdownFinished;

    public String Name { get; }
    public String Description { get; }
    public Double Rate { get; }
    public Int32 Count { get; }

    public static readonly Guid GUID = Guid.Parse("15D432FD-C336-49BD-BC50-DE80BE57CE51");
    public static readonly Version MyVersion = new(1, 2);
}
