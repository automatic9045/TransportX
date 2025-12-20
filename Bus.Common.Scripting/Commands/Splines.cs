using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scripting.Commands
{
    public class Splines
    {
        private readonly ScriptWorld World;

        private readonly Dictionary<string, LaneLayout> LaneLayoutsKey = [];
        public IReadOnlyDictionary<string, LaneLayout> LaneLayouts => LaneLayoutsKey;

        private readonly Dictionary<string, SplineTemplate> TemplatesKey = [];
        public IReadOnlyDictionary<string, SplineTemplate> Templates => TemplatesKey;

        internal Splines(ScriptWorld world)
        {
            World = world;
        }

        public LaneLayout LoadLaneLayout(string key, string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(World.BaseDirectory, path));
            if (!File.Exists(fullPath))
            {
                ScriptError error = new(ErrorLevel.Error, $"進路レイアウトファイル '{fullPath}' が見つかりませんでした。");
                World.ErrorCollector.Report(error);
                return new LaneLayout();
            }

            List<LanePin> pins = [];
            try
            {
                using StreamReader sr = new StreamReader(fullPath);
                for (int i = 0; !sr.EndOfStream; i++)
                {
                    string lineText = sr.ReadLine()!;
                    string[] line = lineText.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (line.Length == 0) continue;
                    if (line[0].StartsWith('#')) continue;

                    try
                    {
                        if (line.Length < 4)
                        {
                            ReportError($"行 '{lineText}' は無効です。引数が不足しています。");
                            continue;
                        }

                        string kindKey = line[0];
                        if (!World.Commander.LaneKinds.TryGetValue(kindKey, out LaneKind? kind))
                        {
                            ReportError($"進路種別 '{kindKey}' が見つかりません。");
                            continue;
                        }

                        if (!Enum.TryParse(line[1], out LanePin.FlowDirection direction)) ReportError($"進行方向 '{lineText}' は無効です。");
                        if (!float.TryParse(line[2], out float x)) ReportError($"X 座標 '{lineText}' は無効です。");
                        if (!float.TryParse(line[3], out float y)) ReportError($"Y 座標 '{lineText}' は無効です。");

                        LanePin pin = new(kind, direction, new Vector2(x, y));
                        pins.Add(pin);
                    }
                    catch (Exception ex)
                    {
                        ReportError($"行 '{lineText}' は無効です。", ex);
                    }


                    void ReportError(string message, Exception? exception = null)
                    {
                        Error error = new(ErrorLevel.Error, message, fullPath)
                        {
                            LineNumber = i + 1,
                            Exception = exception,
                        };
                        World.ErrorCollector.Report(error);
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex, $"進路レイアウトファイル '{fullPath}' を読み込めませんでした。");
                World.ErrorCollector.Report(error);
            }

            LaneLayout layout = new(pins);
            LaneLayoutsKey.Add(key, layout);
            return layout;
        }

        public SplineTemplate CreateTemplate(string key, string layoutKey)
        {
            if (!LaneLayouts.TryGetValue(layoutKey, out LaneLayout? layout))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデル '{layoutKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                layout = new LaneLayout();
            }

            SplineTemplate template = new SplineTemplate(World, layout);
            TemplatesKey.Add(key, template);
            return template;
        }
    }
}
