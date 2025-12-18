using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using Bus.Common.Dependency;
using Bus.Common.Diagnostics;
using Bus.Common.Worlds;

using Bus.Common.Scripting.Commands;

namespace Bus.Common.Scripting
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
            .AddImports(GetAllNamespaces(typeof(ScriptWorld).Assembly));

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

            Script<object> script;
            using (FileStream file = new(ScriptPath, FileMode.Open))
            {
                ScriptOptions options = ScriptOptions.WithFilePath(ScriptPath).WithEmitDebugInformation(true);
                script = CSharpScript.Create(file, options, typeof(Commander));
            }

            ImmutableArray<Diagnostic> diagnostics = script.Compile();
            foreach (Diagnostic diagnostic in diagnostics)
            {
                ErrorLevel errorLevel = diagnostic.Severity switch
                {
                    DiagnosticSeverity.Warning => ErrorLevel.Warning,
                    DiagnosticSeverity.Error => ErrorLevel.Fatal,
                    _ => ErrorLevel.Info,
                };
                if (errorLevel == ErrorLevel.Info) continue;

                string message = diagnostic.GetMessage(CultureInfo.CurrentCulture);
                FileLinePositionSpan lineSpan = diagnostic.Location.GetLineSpan();
                Error error = new(errorLevel, message, diagnostic.Location.SourceTree?.FilePath)
                {
                    Code = diagnostic.Id,
                    LineNumber = lineSpan.StartLinePosition.Line + 1,
                    LinePosition = lineSpan.StartLinePosition.Character + 1,
                };

                ErrorCollector.Report(error);
            }

            if (ErrorCollector.HasFatalError) return;

            script.RunAsync(Commander, ex =>
            {
                StackTrace stackTrace = new(ex, true);
                ScriptError error = new(ErrorLevel.Error, ex, useExceptionStackTrace: true);
                ErrorCollector.Report(error);

                return true;
            }).Wait();
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
    }
}
