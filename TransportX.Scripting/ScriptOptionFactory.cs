using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.CodeAnalysis.Scripting;

using TransportX.Worlds;

using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting
{
    internal static class ScriptOptionFactory
    {
        public static readonly ScriptOptions Default = ScriptOptions.Default
            .WithSourceResolver(new ScriptSourceResolver(ScriptOptions.Default.SourceResolver))
            .AddImports("System", "System.Collections.Generic", "System.Linq", "System.Numerics", "System.Text")
            .AddReferences(typeof(MessageBox).Assembly)
            .AddImports(typeof(MessageBox).Namespace!)
            .AddReferencesAndImports(typeof(WorldBase).Assembly)
            .AddReferencesAndImports(typeof(SplineBase).Assembly)
            .AddReferencesAndImports(typeof(ScriptWorld).Assembly)
            .WithFileEncoding(Encoding.UTF8)
            .WithEmitDebugInformation(true);

        public static ScriptOptions AddReferencesAndImports(this ScriptOptions options, Assembly assembly)
        {
            return options
                .AddReferences(assembly)
                .AddImports(GetAllNamespaces(assembly));
        }

        private static IEnumerable<string> GetAllNamespaces(Assembly assembly)
        {
            return assembly.GetTypes()
                .Select(t => t.Namespace ?? string.Empty)
                .Where(n => n != string.Empty)
                .Distinct();
        }
    }
}
