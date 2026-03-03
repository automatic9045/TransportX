using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Extensions.Network.Elements;

using TransportX.Scripting;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands.Templates
{
    public class SplineSpeedLimit : ITemplateComponent<IReadOnlyList<SplineBase>>
    {
        private readonly int PinCount;
        private readonly IErrorCollector ErrorCollector;

        private readonly Dictionary<int, float> MaxSpeedsKey = [];
        public IReadOnlyDictionary<int, float> MaxSpeeds => MaxSpeedsKey;

        public SplineSpeedLimit(int pinCount, IErrorCollector errorCollector)
        {
            PinCount = pinCount;
            ErrorCollector = errorCollector;
        }

        public void Add(int pinIndex, float maxSpeed)
        {
            if (pinIndex < 0 || PinCount <= pinIndex)
            {
                Error error = new(ErrorLevel.Error, $"進路ピン {pinIndex} は存在しません。有効なピン番号は 0 以上 {PinCount} 未満です。", null);
                ErrorCollector.Report(error);
                return;
            }

            MaxSpeedsKey[pinIndex] = maxSpeed;
        }

        public void Build(IReadOnlyList<SplineBase> parent, IErrorCollector errorCollector)
        {
            foreach ((int pinIndex, float maxSpeed) in MaxSpeeds)
            {
                for (int i = 0; i < parent.Count; i++)
                {
                    SplineBase spline = parent[i];
                    LanePin pin = spline.Inlet.Pins[pinIndex];
                    ILanePath path = pin.SourcePaths[0];

                    SpeedLimitComponent component = new(maxSpeed);
                    path.Components.Add(component);
                }
            }
        }
    }
}
