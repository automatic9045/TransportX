using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

using TransportX.Scripting;
using TransportX.Scripting.Commands;

using TransportX.Domains.RoadTraffic.Network;
using TransportX.Domains.RoadTraffic.Scripting.Commands.Templates;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands
{
    public static class JunctionPathTemplateExtensions
    {
        public static JunctionPathTemplate Deflection(this JunctionPathTemplate template, double forward, double backward)
        {
            PathDeflectionComponent component = new((float)forward, (float)backward);
            template.Components.Add(component);
            return template;
        }

        public static JunctionPathTemplate Deflection(this JunctionPathTemplate template, double forward)
        {
            return Deflection(template, forward, -forward);
        }

        public static JunctionPathTemplate SpeedLimit(this JunctionPathTemplate template, double maxSpeed)
        {
            SpeedLimitComponent component = new((float)maxSpeed);
            template.Components.Add(component);
            return template;
        }

        public static JunctionPathTemplate Yield(this JunctionPathTemplate template, params JunctionPathSegment[] prioritySegments)
        {
            Yield component = new(prioritySegments);
            template.Components.Add(component);
            return template;
        }

        public static JunctionPathTemplate Signal(this JunctionPathTemplate template, string controllerKey, string groupKey)
        {
            TrafficSignals signals = template.World.Commander.Component<TrafficSignals>();
            ISignalController controller = signals.Controllers[controllerKey];
            SignalComponent component = new(controller, groupKey);
            template.Components.Add(component);
            return template;
        }

        public static JunctionPathTemplate Signal(this JunctionPathTemplate template, string groupKey)
        {
            if (!template.Parent.Components.TryGet<DefaultSignalController>(out DefaultSignalController? defaultComponent))
            {
                ScriptError error = new(ErrorLevel.Error, $"親となるジャンクションに既定の信号制御機が指定されていません。");
                template.World.ErrorCollector.Report(error);
                return template;
            }

            SignalComponent component = new(defaultComponent.Controller, groupKey);
            template.Components.Add(component);
            return template;
        }
    }
}
