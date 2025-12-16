using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Dependency
{
    public static class HostAssemblyLoader
    {
        private static readonly string HostDirectory = Path.GetDirectoryName(typeof(HostAssemblyLoader).Assembly.Location)!;

        public static bool TryLoad(AssemblyName assemblyName, out Assembly? assembly)
        {
            string assemblyPath = Path.Combine(HostDirectory, assemblyName.Name + ".dll");
            if (File.Exists(assemblyPath))
            {
                try
                {
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                    return true;
                }
                catch { }
            }

            assembly = null;
            return false;
        }
    }
}
