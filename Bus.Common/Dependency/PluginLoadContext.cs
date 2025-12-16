using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyModel;

namespace Bus.Common.Dependency
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private static readonly HashSet<string> SharedAssemblies = [
            "Bus.Common",
        ];


        private readonly AssemblyDependencyResolver Resolver;

        public List<AssemblyLoadContext> Children { get; } = [];

        public PluginLoadContext(string pluginPath) : base(true)
        {
            Resolver = new AssemblyDependencyResolver(pluginPath);

            Unloading += context =>
            {
                foreach (AssemblyLoadContext child in Children) child.Unload();
            };
        }

        public static PluginLoadContext CreateAndLoadPlugin(string pluginPath, out Assembly assembly)
        {
            PluginLoadContext context = new(pluginPath);
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(pluginPath);
            assembly = context.LoadFromAssemblyName(assemblyName);
            return context;
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name is null) return null;
            if (SharedAssemblies.Contains(assemblyName.Name)) return null;
            if (DependencyContext.Default is not null
                && DependencyContext.Default.RuntimeLibraries.Any(library => library.Name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            if (HostAssemblyLoader.TryLoad(assemblyName, out Assembly? hostAssembly))
            {
                return hostAssembly;
            }

            string? pluginAssemblyPath = Resolver.ResolveAssemblyToPath(assemblyName);
            return pluginAssemblyPath is not null ? LoadFromAssemblyPath(pluginAssemblyPath) : null;
        }
    }
}
