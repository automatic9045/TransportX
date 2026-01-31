using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Rendering;

using Bus.Common.Scripting.Commands.Functions;

namespace Bus.Common.Scripting.Commands
{
    public sealed class Models
    {
        private readonly ScriptWorld World;

        internal Models(ScriptWorld world)
        {
            World = world;
        }

        public void LoadList(string path)
        {
            string listPath = Path.GetFullPath(Path.Combine(World.BaseDirectory, path));
            if (!File.Exists(listPath))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデルリスト '{listPath}' が見つかりませんでした。");
                World.ErrorCollector.Report(error);
                return;
            }

            try
            {
                using (StreamReader sr = new StreamReader(listPath))
                {
                    string listDirectory = Path.GetDirectoryName(listPath)!;
                    Parser parser = new Parser(ModelListSignatures.All);
                    for (int i = 0; !sr.EndOfStream; i++)
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

                            string key = line[0];
                            string modelPath = Path.Combine(listDirectory, line[1]);

                            ModelListInterpreter interpreter = new(World, parser, listDirectory, modelPath);
                            interpreter.ErrorReported += (sender, e) =>
                            {
                                Error error = e.Error is ModelLoadError modelError && modelError.Types.HasFlag(ModelLoadErrorTypes.Critical)
                                    ? modelError.ChangeSource(listPath, i + 1) : e.Error;
                                World.ErrorCollector.Report(error);
                            };

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

                            Model model = interpreter.Build();
                            model.DebugName = key;

                            if (model is CollidableModel collidableModel)
                            {
                                collidableModel.Collider.CreateDebugModel(World.DXHost.Device);
                                collidableModel.Collider.DebugModelColor = new Vector4(1, 0, 0, 1);
                            }

                            World.Models[key] = model;
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
                }
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex, $"モデルリスト '{listPath}' を読み込めませんでした。");
                World.ErrorCollector.Report(error);
            }
        }
    }
}
