using Narumikazuchi.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TestAddInLib
{
    [AddIn("TestAddIn", "8849A7A0-2F62-41BB-B687-F4BAF0340E9B", "1.1")]
    public class TestAddIn
    {
        public TestAddIn() 
        {
            this._random = new();
            Int32 max = this._random.Next(64, 260);
            for (Int32 i = 0; i < max; i++)
            {
                this._rates.Add(this._random.NextDouble());
            }
            for (Int32 x = 0; x < 256; x++)
            {
                for (Int32 y = 0; y < 256; y++)
                {
                    for (Int32 z = 0; z < 256; z++)
                    {
                        Tuple<Int32, Int32, Int32, Double> tuple = new(x,
                                                                       y,
                                                                       z,
                                                                       this._random.NextDouble());
                        this._matrix.Add(tuple);
                    }
                }
            }
        }

        [AddInExposed]
        public void SomeProcedure()
        {
            Thread.Sleep(100);
        }

        [AddInExposed]
        public Boolean SomeValidation(Boolean isTrue)
        {
            return isTrue;
        }

        [AddInExposed]
        public Int32 SomeGetOnlyProperty => this._value;

        [AddInExposed]
        public Int32 SomeSetOnlyProperty
        {
            set => this._value = value;
        }

        [AddInExposed]
        public String SomeBothAccessorProperty
        {
            get => this._str;
            set => this._str = value;
        }

        [AddInExposed]
        public Double this[Int32 x, Int32 y, Int32 z]
        {
            get
            {
                Tuple<Int32, Int32, Int32, Double> tuple = this._matrix.FirstOrDefault(t => t.Item1 == x &&
                                                                                       t.Item2 == y &&
                                                                                       t.Item3 == z);
                return tuple is null
                            ? default
                            : tuple.Item4;
            }
        }

        [AddInExposed]
        public Double this[Int32 x, Int32 y]
        {
            set => this._matrix.Add(new(x, y, 0, value));
        }

        [AddInExposed]
        public Double this[Int32 index]
        {
            get => this._rates[index];
            set => this._rates[index] = value;
        }

        private readonly List<Double> _rates = new();
        private readonly List<Tuple<Int32, Int32, Int32, Double>> _matrix = new();
        private readonly Random _random;
        private Int32 _value;
        private String _str = "Test";

        public static readonly Guid GUID = Guid.Parse("8849A7A0-2F62-41BB-B687-F4BAF0340E9B");
    }
}
