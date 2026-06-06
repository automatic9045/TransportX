using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Scripting.Collections;
using TransportX.Scripting.Worlds.Commands.Functions;

namespace TransportX.Scripting.Worlds.Commands
{
    public sealed class Models
    {
        private readonly ScriptWorld World;

        private int DefaultKeyIndex = 0;

        internal Models(ScriptWorld world)
        {
            World = world;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal IModelBundle LoadList(string key, string path)
        {
            return LoadListInternal(key, path);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IModelBundle LoadList(string listPath)
        {
            string key;
            do
            {
                key = DefaultKeyIndex++.ToString(CultureInfo.InvariantCulture);
            }
            while (World.Models.Bundles.Contains(key));

            return LoadListInternal(key, listPath);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private ModelBundle LoadListInternal(string key, string path)
        {
            string listPath = Path.GetFullPath(Path.Combine(BaseDirectory.Find(3) ?? World.BaseDirectory, path));
            if (!File.Exists(listPath))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデルリスト '{listPath}' が見つかりませんでした。");
                World.ErrorCollector.Report(error);
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
                    World.ErrorCollector.Report(error);
                };
                using ModelFactory factory = new(World.DXHost.Context, World.PhysicsHost.Simulation, factoryErrorCollector);

                ModelListInterpreter interpreter = new(parser, factory, listDirectory, factoryErrorCollector);

                ScriptDictionary<string, IModel> models = new(World.ErrorCollector, "モデル", key => Model.Empty());
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
                            World.ErrorCollector.Report(error);
                            continue;
                        }

                        string modelKey = line[0];
                        string modelPath = Path.Combine(listDirectory, line[1]);

                        string commandListText = line.Length < 3 ? string.Empty : line[2].ToLowerInvariant();
                        if (!string.IsNullOrWhiteSpace(commandListText))
                        {
                            if (!commandListText.StartsWith('$'))
                            {
                                Error error = new(ErrorLevel.Error, $"コマンド '{line[2]}' は無効です。冒頭に '$' がありません。", listPath)
                                {
                                    LineNumber = i + 1,
                                };
                                World.ErrorCollector.Report(error);
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
                            collidableModel.CreateColliderDebugModel(World.DXHost.Device);
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
                        World.ErrorCollector.Report(error);
                    }
                }

                ModelBundle bundle = new(key, models);
                return bundle;
                if (World.Models.AdoptBundle(bundle))
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex, $"モデルリスト '{listPath}' を読み込めませんでした。");
                World.ErrorCollector.Report(error);
                return ModelBundle.Empty(key);
            }
        }
    }
}
