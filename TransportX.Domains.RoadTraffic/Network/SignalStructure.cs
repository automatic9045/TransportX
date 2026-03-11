using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Spatial;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class SignalStructure
    {
        private readonly LocatedModel Model;
        private readonly ISignalController Controller;
        private readonly string GroupKey;
        private readonly SignalLampRole Role;

        public SignalStructure(LocatedModel model, ISignalController controller, string groupKey, SignalLampRole role)
        {
            Model = model;
            Controller = controller;
            GroupKey = groupKey;
            Role = role;
        }

        public void Tick(DateTime now)
        {
            if (!Controller.Signals.TryGetValue(GroupKey, out SignalColor color))
            {
                Model.IsVisible = false;
                return;
            }

            bool isLighting;
            switch (color)
            {
                case SignalColor.Off:
                    isLighting = false;
                    break;

                case SignalColor.Red:
                case SignalColor.BlinkingRed:
                    isLighting = Role == SignalLampRole.Red;
                    break;

                case SignalColor.Yellow:
                case SignalColor.BlinkingYellow:
                    isLighting = Role == SignalLampRole.Yellow;
                    break;

                case SignalColor.Green:
                case SignalColor.BlinkingGreen:
                    isLighting = Role == SignalLampRole.Green;
                    break;

                default:
                    throw new NotSupportedException();
            }

            switch (color)
            {
                case SignalColor.BlinkingRed:
                case SignalColor.BlinkingYellow:
                case SignalColor.BlinkingGreen:
                    isLighting |= now.Millisecond <= 600;
                    break;
            }

            Model.IsVisible = isLighting;
        }
    }
}
