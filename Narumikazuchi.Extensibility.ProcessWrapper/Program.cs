using Narumikazuchi.Windows.Pipes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Narumikazuchi.Extensibility.ProcessWrapper
{
    // Resolve Arguments
    public static partial class Program
    {
        internal static void Main(String[] args)
        {
            if (args.Length < 2)
            {
                return;
            }
            if (args[0] is not "-d" 
                        and not "-e")
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(args[1]))
            {
                return;
            }

            FileInfo file = new(args[1]);
            if (!file.Exists)
            {
                return;
            }

            Byte[] bytes = File.ReadAllBytes(file.FullName);
            Assembly assembly = Assembly.Load(bytes);

            if (args[0] == "-d")
            {
                Discover(assembly);
                return;
            }

            if (args.Length < 3)
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(args[2]))
            {
                return;
            }
            Type type = assembly.GetType(args[2]);
            if (type is null)
            {
                return;
            }
            AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(type);
            Thread worker = new(() => Run(type, attribute));
            worker.Start();

            Console.ReadLine();
        }

    }

    // Discovery
    partial class Program
    {
        private static void Discover(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes()
                                          .Where(
                                            t => AttributeResolver.HasAttribute<AddInAttribute>(t)))
            {
                AddInAttribute attribute = AttributeResolver.FetchOnlyAllowedAttribute<AddInAttribute>(type);
                String token = String.Join('|',
                                           attribute.Guid.ToString(),
                                           attribute.Name,
                                           attribute.Version.ToString(),
                                           type.FullName,
                                           assembly.FullName,
                                           assembly.Location);
                Console.WriteLine(token);
            }
            Process.GetCurrentProcess().Kill();
        }
    }

    // Run AddIn
    partial class Program
    {
        private static void Run(Type type,
                                AddInAttribute addIn)
        {
            Type adapter = typeof(__AddInAdapter<>).MakeGenericType(type);
            ConstructorInfo ctor = adapter.GetConstructor(Array.Empty<Type>());

            try
            {
                _adapter = (__AddInAdapter)ctor.Invoke(Array.Empty<Object>());
            }
            catch (NullReferenceException)
            {
                Process.GetCurrentProcess().Kill();
                return;
            }
            catch (InvalidCastException)
            {
                Process.GetCurrentProcess().Kill();
                return;
            }
            if (_adapter is null)
            {
                Process.GetCurrentProcess().Kill();
                return;
            }

            Client<__ByteMessage> client = Client<__ByteMessage>.CreateClient("127.0.0.1",
                                                                              addIn.Name);
            client.DataReceived += ProcessIncomingData;
            client.Connect();
        }

        private static void ProcessIncomingData(IClient<__ByteMessage> client, 
                                                DataReceivedEventArgs<__ByteMessage> args)
        {
            if (!_adapter.Process(args.Data, 
                                  client))
            {
                client.Disconnect();
                Process.GetCurrentProcess().Kill();
            }
        }

        private static __AddInAdapter _adapter;
    }
}
