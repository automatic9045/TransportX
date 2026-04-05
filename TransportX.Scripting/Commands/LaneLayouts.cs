using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;
using TransportX.Scripting.Data;

namespace TransportX.Scripting.Commands
{
    public class LaneLayouts : IReadOnlyDictionary<string, LaneLayout>
    {
        private readonly ScriptWorld World;
        private readonly ScriptDictionary<string, LaneLayout> Items;

        public LaneLayout this[string key] => Items[key];
        public IEnumerable<string> Keys => Items.Keys;
        public IEnumerable<LaneLayout> Values => Items.Values;
        public int Count => Items.Count;

        internal LaneLayouts(ScriptWorld world)
        {
            World = world;
            Items = new ScriptDictionary<string, LaneLayout>(World.ErrorCollector, "進路レイアウト", key => new LaneLayout());
        }

        public LaneLayout Add(string key, LaneLayout layout)
        {
            Items[key] = layout;
            return layout;
        }

        public LaneLayout AddOpposition(string key, string baseKey)
        {
            Items.GetValue(baseKey, out LaneLayout baseLayout);
            return Add(key, baseLayout.Opposition);
        }

        public LaneLayout Load(string key, string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(BaseDirectory.Find() ?? World.BaseDirectory, path));
            if (!File.Exists(fullPath))
            {
                ScriptError error = new(ErrorLevel.Error, $"進路レイアウトファイル '{fullPath}' が見つかりませんでした。");
                World.ErrorCollector.Report(error);
                return CreateEmptyLayout();
            }

            Data.Network.LaneLayout? layoutData = XmlSerializer<Data.Network.LaneLayout>.FromXml(fullPath, World.ErrorCollector);
            if (layoutData is null) return CreateEmptyLayout();

            List<Lane> lanes = [];
            try
            {
                for (int i = 0; i < layoutData.Lanes.Count; i++)
                {
                    Data.Network.Lane laneData = layoutData.Lanes[i];
                    World.ErrorCollector.ReportRange(laneData.Errors);

                    if (laneData.AllowedTraffic.Value is null) continue;
                    if (!World.Commander.Network.LaneTraffic.Groups.GetValue(laneData.AllowedTraffic.Value, out LaneTrafficGroup group)) continue;

                    LaneWidth width = new(laneData.LeftWidth.Value, laneData.RightWidth.Value);
                    Lane lane = new(group, laneData.Directions.Value, new Vector2(laneData.X.Value, laneData.Y.Value), width);
                    lanes.Add(lane);
                }
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex, $"進路レイアウトファイル '{fullPath}' を読み込めませんでした。");
                World.ErrorCollector.Report(error);
            }

            LaneLayout layout = new(lanes);
            return Add(key, layout);


            LaneLayout CreateEmptyLayout()
            {
                LaneLayout layout = new();
                Items.Add(key, layout);
                return layout;
            }
        }

        public bool ContainsKey(string key) => Items.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, LaneLayout>> GetEnumerator() => Items.GetEnumerator();
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out LaneLayout value) => Items.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
