using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Scenery.Networks;
using Bus.Common.Scripting.Data;

namespace Bus.Common.Scripting.Commands
{
    public class LaneLayouts : IReadOnlyDictionary<string, LaneLayout>
    {
        private readonly ScriptWorld World;
        private readonly Dictionary<string, LaneLayout> Items = [];

        public LaneLayout this[string key] => Items[key];
        public IEnumerable<string> Keys => Items.Keys;
        public IEnumerable<LaneLayout> Values => Items.Values;
        public int Count => Items.Count;

        internal LaneLayouts(ScriptWorld world)
        {
            World = world;
        }

        public LaneLayout Load(string key, string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(World.BaseDirectory, path));
            if (!File.Exists(fullPath))
            {
                ScriptError error = new(ErrorLevel.Error, $"進路レイアウトファイル '{fullPath}' が見つかりませんでした。");
                World.ErrorCollector.Report(error);
                return CreateEmptyLayout();
            }

            Data.Networks.LaneLayout? layoutData = XmlSerializer<Data.Networks.LaneLayout>.FromXml(fullPath, World.ErrorCollector);
            if (layoutData is null) return CreateEmptyLayout();

            List<Lane> lanes = [];
            try
            {
                for (int i = 0; i < layoutData.Lanes.Count; i++)
                {
                    Data.Networks.Lane laneData = layoutData.Lanes[i];
                    World.ErrorCollector.ReportRange(laneData.Errors);

                    if (laneData.Kind.Value is null) continue;
                    if (!World.Commander.Network.LaneKinds.TryGetValue(laneData.Kind.Value, out LaneKind? kind))
                    {
                        ReportError(laneData.Kind, $"進路種別 '{laneData.Kind.Value}' が見つかりません。");
                        continue;
                    }

                    Lane lane = new(kind, laneData.Directions.Value, new Vector2(laneData.X.Value, laneData.Y.Value));
                    lanes.Add(lane);


                    void ReportError<T>(XmlValue<T> source, string message)
                    {
                        Error error = source.CreateError(message);
                        World.ErrorCollector.Report(error);
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex, $"進路レイアウトファイル '{fullPath}' を読み込めませんでした。");
                World.ErrorCollector.Report(error);
            }

            LaneLayout layout = new(lanes);
            Items.Add(key, layout);
            return layout;


            LaneLayout CreateEmptyLayout()
            {
                LaneLayout layout = new();
                Items.Add(key, layout);
                return layout;
            }
        }

        internal LaneLayout Get(string key)
        {
            if (!TryGetValue(key, out LaneLayout? layout))
            {
                ScriptError error = new(ErrorLevel.Error, $"進路レイアウト '{key}' が見つかりません。");
                World.ErrorCollector.Report(error);

                layout = new LaneLayout();
            }

            return layout;
        }

        public bool ContainsKey(string key) => Items.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, LaneLayout>> GetEnumerator() => Items.GetEnumerator();
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out LaneLayout value) => Items.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
