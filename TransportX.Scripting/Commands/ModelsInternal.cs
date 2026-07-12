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
            using AssetList list = new(path, defaultBaseDirectory, "モデルリスト", [2, 3], ErrorCollector);
            if (!list.IsValid) return ModelBundle.Empty(key);

            try
            {
                Parser parser = new(ModelListSignatures.All);

                IErrorCollector factoryErrorCollector = IErrorCollector.Default();
                factoryErrorCollector.Reported += (sender, e) =>
                {
                    Error error = e.Error is ModelLoadError modelError && modelError.Source == ModelLoadError.ErrorSource.Reference
                        ? modelError.ChangeSource(list.ListPath, list.LineNumber) : e.Error;
                    ErrorCollector.Report(error);
                };
                using ModelFactory factory = new(DXHost.Context, PhysicsHost.Simulation, factoryErrorCollector);

                ModelListInterpreter interpreter = new(parser, PhysicsHost.Simulation, factory, list.ListDirectory, factoryErrorCollector);

                ScriptDictionary<string, IModel> models = new(ErrorCollector, "モデル", key => Model.Empty());

                while (list.ReadLine(out string[] line))
                {
                    try
                    {
                        string modelKey = line[0];
                        string? modelPath =
                            line[1] == string.Empty || line[1].Equals("NONE", StringComparison.OrdinalIgnoreCase) ? null
                            : Path.Combine(list.ListDirectory, line[1]);

                        string commandListText = line.Length < 3 ? string.Empty : line[2].ToLowerInvariant();
                        if (!string.IsNullOrWhiteSpace(commandListText))
                        {
                            if (!commandListText.StartsWith('$'))
                            {
                                Error error = new(ErrorLevel.Error, $"コマンド '{line[2]}' は無効です。冒頭に '$' がありません。", list.ListPath)
                                {
                                    LineNumber = list.LineNumber,
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
                        Error error = new(ErrorLevel.Error, $"レコード '{line[0]}' は無効です。", list.ListPath)
                        {
                            LineNumber = list.LineNumber,
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
                ScriptError error = new(ErrorLevel.Error, ex, $"モデルリスト '{list.ListPath}' を読み込めませんでした。");
                ErrorCollector.Report(error);
                return ModelBundle.Empty(key);
            }
        }
    }
}
