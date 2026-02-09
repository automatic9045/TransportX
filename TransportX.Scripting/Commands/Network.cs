using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;

namespace TransportX.Scripting.Commands
{
    public class Network
    {
        private readonly ScriptWorld World;

        public LaneTraffic LaneTraffic { get; }
        public LaneLayouts LaneLayouts { get; }
        public Templates Templates { get; }

        internal Network(ScriptWorld world)
        {
            World = world;

            LaneTraffic = new LaneTraffic(World);
            LaneLayouts = new LaneLayouts(World);
            Templates = new Templates(World);
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
