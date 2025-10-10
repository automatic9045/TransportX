using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;

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
                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine()!.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (line.Length == 0) continue;
                    if (line[0].StartsWith("#")) continue;

                    if (line.Length < 2) throw new FormatException($"レコード '{line[0]}' は無効です。");

                    string key = line[0];
                    string modelPath = Path.Combine(listDirectory, line[1]);
                    Model model = Model.FromFile(World.DXHost.Device, World.DXHost.Context, modelPath);
                    World.Models[key] = model;
                }
            }
        }
    }
}
