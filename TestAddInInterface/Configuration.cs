using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAddInInterface;

public readonly struct Configuration
{
    public String Name { get; init; }
    public String Description { get; init; }
    public Double Rate { get; init; }
    public Int32 Count { get; init; }
}