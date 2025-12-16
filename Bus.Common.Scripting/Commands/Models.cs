using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using Bus.Common.Physics;
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
            string listPath = Path.Combine(World.BaseDirectory, path);
            using (StreamReader sr = new StreamReader(Path.Combine(World.BaseDirectory, path)))
            {
                string listDirectory = Path.GetDirectoryName(listPath)!;
                Parser parser = new Parser(ModelListSignatures.All);
                for (int i = 0; !sr.EndOfStream; i++)
                {
                    string[] line = sr.ReadLine()!.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (line.Length == 0) continue;
                    if (line[0].StartsWith("#")) continue;

                    try
                    {
                        if (line.Length < 2) throw new FormatException($"レコード '{line[0]}' は無効です。");

                        string key = line[0];
                        string modelPath = Path.Combine(listDirectory, line[1]);
                        ModelListInterpreter interpreter = new(World, parser, listDirectory, key, modelPath);

                        string commandListText = line.Length < 3 ? string.Empty : line[2].ToLowerInvariant();
                        if (!string.IsNullOrWhiteSpace(commandListText))
                        {
                            if (!commandListText.StartsWith('$')) throw new FormatException($"コマンド '{line[2]}' は無効です。冒頭に '$' がありません。");

                            IEnumerable<string> commandTexts = commandListText.Split('$').Skip(1);
                            foreach (string commandText in commandTexts)
                            {
                                interpreter.ReadCommand(commandText);
                            }
                        }

                        Model model = interpreter.Build();

                        if (model is CollidableModel collidableModel)
                        {
                            collidableModel.Collider.CreateDebugModel(World.DXHost.Device, new Vector4(1, 0, 0, 1));
                        }

                        World.Models[key] = model;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"モデルを読み込めませんでした ({i + 1} 行目)。", ex);
                    }
                }
            }
        }
    }
}
