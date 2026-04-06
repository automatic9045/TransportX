using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Extensions.Network.Elements;

using TransportX.Scripting.Collections;

namespace TransportX.Scripting.Commands
{
    public class Network
    {
        private readonly ScriptWorld World;

        public LaneTraffic LaneTraffic { get; }
        public LaneLayouts LaneLayouts { get; }
        public Templates Templates { get; }

        internal ScriptDictionary<string, SplineCommand> SplinesKey { get; }
        public IReadOnlyScriptDictionary<string, SplineCommand> Splines => SplinesKey;

        internal ScriptDictionary<string, JunctionCommand> JunctionsKey { get; }
        public IReadOnlyScriptDictionary<string, JunctionCommand> Junctions => JunctionsKey;

        internal Network(ScriptWorld world)
        {
            World = world;

            LaneTraffic = new LaneTraffic(World);
            LaneLayouts = new LaneLayouts(World);
            Templates = new Templates(World);

            SplinesKey = new ScriptDictionary<string, SplineCommand>(World.ErrorCollector, "スプライン",
                key => new SplineCommand(World, []));
            JunctionsKey = new ScriptDictionary<string, JunctionCommand>(World.ErrorCollector, "ジャンクション",
                key => new JunctionCommand(World, new Junction(0, 0, Pose.Identity, [])));
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
