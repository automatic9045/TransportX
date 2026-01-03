using System;
using System.Collections.Generic;
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
    public class Network
    {
        private readonly ScriptWorld World;

        public Templates Templates { get; }

        private readonly Dictionary<string, LaneLayout> LaneLayoutsKey = [];
        public IReadOnlyDictionary<string, LaneLayout> LaneLayouts => LaneLayoutsKey;

        internal Network(ScriptWorld world)
        {
            World = world;
            Templates = new Templates(World);
        }

        public LaneLayout LoadLaneLayout(string key, string path)
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
                    if (!World.Commander.LaneKinds.TryGetValue(laneData.Kind.Value, out LaneKind? kind))
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
            LaneLayoutsKey.Add(key, layout);
            return layout;


            LaneLayout CreateEmptyLayout()
            {
                LaneLayout layout = new();
                LaneLayoutsKey.Add(key, layout);
                return layout;
            }
        }

        internal LaneLayout GetLaneLayout(string key)
        {
            if (!LaneLayouts.TryGetValue(key, out LaneLayout? layout))
            {
                ScriptError error = new(ErrorLevel.Error, $"進路レイアウト '{key}' が見つかりません。");
                World.ErrorCollector.Report(error);

                layout = new LaneLayout();
            }

            return layout;
        }

        public void Connect(NetworkPort a, NetworkPort b)
        {
            try
            {
                a.ConnectTo(b);
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex);
                World.ErrorCollector.Report(error);
            }
        }
    }
}
