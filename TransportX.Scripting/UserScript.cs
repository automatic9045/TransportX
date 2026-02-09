using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using TransportX.Diagnostics;
using TransportX.Scripting.Commands;

namespace TransportX.Scripting
{
    public class UserScript<TCommander, TResult>
    {
        public Script<TResult> Script { get; }

        protected UserScript(Script<TResult> script)
        {
            Script = script;
        }

        public static UserScript<TCommander, TResult> FromFile(string filePath, IErrorCollector errorCollector, bool setErrorAsFatal)
        {
            Script<TResult> script;
            using (FileStream file = new(filePath, FileMode.Open))
            {
                ScriptOptions options = ScriptWorld.ScriptOptions.WithFilePath(filePath);
                script = CSharpScript.Create<TResult>(file, options, typeof(TCommander));
            }

            ImmutableArray<Diagnostic> diagnostics = script.Compile();
            foreach (Diagnostic diagnostic in diagnostics)
            {
                ErrorLevel errorLevel = diagnostic.Severity switch
                {
                    DiagnosticSeverity.Warning => ErrorLevel.Warning,
                    DiagnosticSeverity.Error => setErrorAsFatal ? ErrorLevel.Fatal : ErrorLevel.Error,
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

                errorCollector.Report(error);
            }

            return new UserScript<TCommander, TResult>(script);
        }

        public async Task<ScriptState<TResult>> RunAsync(TCommander commander, IErrorCollector errorCollector)
        {
            return await Script.RunAsync(commander, ex =>
            {
                StackTrace stackTrace = new(ex, true);
                ScriptError error = new(ErrorLevel.Error, ex, useExceptionStackTrace: true);
                errorCollector.Report(error);

                return true;
            });
        }
    }
}
