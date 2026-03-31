using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.IO;

using TransportX.Scripting.Data.Scripts;

namespace TransportX.Scripting
{
    public partial class UserScript<TCommander, TResult>
    {
        public Script<TResult> Script { get; }

        protected UserScript(Script<TResult> script)
        {
            Script = script;
        }

        public static UserScript<TCommander, TResult> FromFile(PluginLoadContext context, string filePath, IErrorCollector errorCollector, bool setErrorAsFatal)
        {
            string manifestPath = Path.ChangeExtension(filePath, ".manifest.xml");
            ScriptManifest manifest = !Path.Exists(manifestPath) ? new()
                : Data.XmlSerializer<ScriptManifest>.FromXml(manifestPath, errorCollector) ?? new();

            ScriptOptions options = ScriptOptionFactory.Default.WithFilePath(filePath);
            string localBaseDirectory = Path.GetDirectoryName(filePath)!;
            foreach (Data.Scripts.Dependency dependency in manifest.Dependencies)
            {
                errorCollector.ReportRange(dependency.Errors);

                string assemblyPath = Path.GetFullPath(Path.Combine(localBaseDirectory, PathMacros.Expand(dependency.Path.Value)));
                if (!File.Exists(assemblyPath))
                {
                    Error error = new(ErrorLevel.Error, $"依存関係 '{assemblyPath}' が見つかりませんでした。", manifestPath);
                    errorCollector.Report(error);
                    continue;
                }

                try
                {
                    Assembly assembly = context.LoadFromAssemblyPath(assemblyPath);
                    options = options.AddReferencesAndImports(assembly);
                }
                catch (Exception ex)
                {
                    Error error = new(ErrorLevel.Error, $"依存関係 '{assemblyPath}' が読み込めませんでした。", manifestPath)
                    {
                        Exception = ex,
                    };
                    errorCollector.Report(error);
                }
            }

            string scriptText;
            try
            {
                UTF8Encoding strictUtf8 = new(false, true);
                using StreamReader reader = new(filePath, strictUtf8, true);
                scriptText = reader.ReadToEnd();
            }
            catch (DecoderFallbackException ex)
            {
                Error error = new(setErrorAsFatal ? ErrorLevel.Fatal : ErrorLevel.Error,
                    "サポートされない文字コードです。Shift-JIS 等で保存されている可能性があります。文字化けを防ぐため、ファイルを UTF-8 で保存し直してください。", filePath)
                {
                    Exception = ex,
                };
                errorCollector.Report(error);
                scriptText = string.Empty;
            }
            catch (Exception ex)
            {
                Error error = new(setErrorAsFatal ? ErrorLevel.Fatal : ErrorLevel.Error, $"スクリプトファイル '{filePath}' を読み込めませんでした。", filePath)
                {
                    Exception = ex,
                };
                errorCollector.Report(error);
                scriptText = string.Empty;
            }

            Script<TResult> script = CSharpScript.Create<TResult>(scriptText, options, typeof(TCommander));

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
