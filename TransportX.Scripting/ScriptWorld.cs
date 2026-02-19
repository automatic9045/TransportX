using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

using TransportX.Dependency;
using TransportX.Environment;
using TransportX.Rendering;
using TransportX.Worlds;

using TransportX.Scripting.Commands;

namespace TransportX.Scripting
{
    public class ScriptWorld : WorldBase
    {
        internal static readonly ScriptOptions ScriptOptions = ScriptOptions.Default
            .AddReferences(typeof(MessageBox).Assembly)
            .AddReferences(typeof(Matrix4x4).Assembly)
            .AddReferences(typeof(WorldBase).Assembly)
            .AddReferences(typeof(ScriptWorld).Assembly)
            .AddImports("System", "System.Collections.Generic", "System.Linq", "System.Numerics", "System.Text")
            .AddImports(typeof(MessageBox).Namespace!)
            .AddImports(GetAllNamespaces(typeof(WorldBase).Assembly))
            .AddImports(GetAllNamespaces(typeof(ScriptWorld).Assembly))
            .WithEmitDebugInformation(true);

        private static IEnumerable<string> GetAllNamespaces(Assembly assembly)
        {
            return assembly.GetTypes()
                .Select(t => t.Namespace ?? string.Empty)
                .Where(n => n != string.Empty)
                .Distinct();
        }


        public override IModelCollection Models { get; } = new ModelCollection();

        public string ScriptPath { get; }
        internal Commander Commander { get; }

        public ScriptWorld(PluginLoadContext context, WorldBuilder builder) : base(context, builder)
        {
            if (Info.Args.Count == 0) throw new InvalidOperationException("ワールドファイルのパスが指定されていません。");

            ScriptPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Info.InfoPath)!, Info.Args[0]));
            ScriptError.DefaultLocation = ScriptPath;
            BaseDirectory = Path.GetDirectoryName(ScriptPath)!;

            Commander = new Commander(this);

            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(ScriptPath, ErrorCollector, true);
            if (ErrorCollector.HasFatalError) return;

            script.RunAsync(Commander, ErrorCollector).Wait();
        }

        public override void Dispose()
        {
            Commander.Dispose();
            base.Dispose();
        }

        public override void Tick(TimeSpan elapsed)
        {
            Commander.Triggers.Tick(elapsed);
            base.Tick(elapsed);
        }

        internal void SetDefaultEnvironment(EnvironmentProfile value)
        {
            DefaultEnvironment = value;
        }

        internal void SetDirectionalLight(Worlds.DirectionalLight value)
        {
            DirectionalLight = value;
        }
    }
}
