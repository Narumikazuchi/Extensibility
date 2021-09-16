using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Narumikazuchi.Extensibility
{
    internal static class __AssemblyHelper
    {
        internal static Assembly? GetAssembly(FileInfo file)
        {
            Byte[] bytes = File.ReadAllBytes(file.FullName);
            Assembly? assembly = null;
            try
            {
                assembly = Assembly.Load(bytes);
                Assembly? already = AppDomain.CurrentDomain.GetAssemblies()
                                                           .FirstOrDefault(a => a == assembly);
                if (already is not null)
                {
                    assembly = already;
                }
            }
            catch
            {
                return null;
            }
            return assembly;
        }
    }
}
