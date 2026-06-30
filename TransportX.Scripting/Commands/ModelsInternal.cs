using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Diagnostics;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Rendering.Backend;
using TransportX.Scripting.Collections;
using TransportX.Scripting.Commands.Functions;

namespace TransportX.Scripting.Commands
{
    internal class ModelsInternal
    {
        private readonly IDXHost DXHost;
        private readonly IPhysicsHost PhysicsHost;
        private readonly IModelCollection Models;
        private readonly IErrorCollector ErrorCollector;

        public ModelsInternal(IDXHost dxHost, IPhysicsHost physicsHost, IModelCollection models, IErrorCollector errorCollector)
        {
            DXHost = dxHost;
            PhysicsHost = physicsHost;
            Models = models;
            ErrorCollector = errorCollector;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ModelBundle LoadList(string key, string path, string defaultBaseDirectory)
        {
            string listPath = Path.GetFullPath(Path.Combine(BaseDirectory.Find(3) ?? defaultBaseDirectory, path));
            if (!File.Exists(listPath))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデルリスト '{listPath}' が見つかりませんでした。");
                ErrorCollector.Report(error);
                return ModelBundle.Empty(key);
            }

            try
            {
                using StreamReader sr = new(listPath);
                string listDirectory = Path.GetDirectoryName(listPath)!;

                Parser parser = new(ModelListSignatures.All);

                int i = 0;
                IErrorCollector factoryErrorCollector = IErrorCollector.Default();
                factoryErrorCollector.Reported += (sender, e) =>
                {
                    Error error = e.Error is ModelLoadError modelError && modelError.Source == ModelLoadError.ErrorSource.Reference
                        ? modelError.ChangeSource(listPath, i + 1) : e.Error;
                    ErrorCollector.Report(error);
                };
                using ModelFactory factory = new(DXHost.Context, PhysicsHost.Simulation, factoryErrorCollector);

                ModelListInterpreter interpreter = new(parser, PhysicsHost.Simulation, factory, listDirectory, factoryErrorCollector);

                ScriptDictionary<string, IModel> models = new(ErrorCollector, "モデル", key => Model.Empty());
                for (; !sr.EndOfStream; i++)
                {
                    string[] line = sr.ReadLine()!.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (line.Length == 0) continue;
                    if (line[0].StartsWith('#')) continue;

                    try
                    {
                        if (line.Length < 2)
                        {
                            Error error = new(ErrorLevel.Error, $"レコード '{line[0]}' は無効です。引数が不足しています。", listPath)
                            {
                                LineNumber = i + 1,
                            };
                            ErrorCollector.Report(error);
                            continue;
                        }

                        string modelKey = line[0];
                        string? modelPath =
                            line[1] == string.Empty || line[1].Equals("NONE", StringComparison.OrdinalIgnoreCase) ? null
                            : Path.Combine(listDirectory, line[1]);

                        string commandListText = line.Length < 3 ? string.Empty : line[2].ToLowerInvariant();
                        if (!string.IsNullOrWhiteSpace(commandListText))
                        {
                            if (!commandListText.StartsWith('$'))
                            {
                                Error error = new(ErrorLevel.Error, $"コマンド '{line[2]}' は無効です。冒頭に '$' がありません。", listPath)
                                {
                                    LineNumber = i + 1,
                                };
                                ErrorCollector.Report(error);
                            }
                            else
                            {
                                IEnumerable<string> commandTexts = commandListText.Split('$').Skip(1);
                                foreach (string commandText in commandTexts)
                                {
                                    interpreter.ReadCommand(commandText);
                                }
                            }
                        }

                        Model model = interpreter.Build(modelPath);
                        model.DebugName = key;

                        if (model is CollidableModel collidableModel)
                        {
                            collidableModel.CreateColliderDebugModel(DXHost.Device);
                            collidableModel.ColliderDebugModel!.Color = new Vector4(1, 0, 0, 1);
                        }

                        models.Add(modelKey, model);
                    }
                    catch (Exception ex)
                    {
                        Error error = new(ErrorLevel.Error, $"レコード '{line[0]}' は無効です。", listPath)
                        {
                            LineNumber = i + 1,
                            Exception = ex,
                        };
                        ErrorCollector.Report(error);
                    }
                }

                ModelBundle bundle = new(key, models, factory.Textures.ToArray());
                if (Models.AdoptBundle(bundle))
                {
                    return bundle;
                }
                else
                {
                    bundle.Dispose();
                    return ModelBundle.Empty(key);
                }
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex, $"モデルリスト '{listPath}' を読み込めませんでした。");
                ErrorCollector.Report(error);
                return ModelBundle.Empty(key);
            }
        }
    }
}
