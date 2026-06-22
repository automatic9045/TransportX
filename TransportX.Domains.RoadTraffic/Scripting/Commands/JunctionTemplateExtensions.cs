using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Scripting;
using TransportX.Scripting.Worlds.Commands;

using TransportX.Domains.RoadTraffic.Network;
using TransportX.Domains.RoadTraffic.Scripting.Commands.Templates;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands
{
    public static class JunctionTemplateExtensions
    {
        public static JunctionTemplate SignalController(this JunctionTemplate template, string key)
        {
            TrafficSignals signals = template.World.Commander.Component<TrafficSignals>();
            if (signals.Controllers.GetValue(key, out ISignalController controller))
            {
                DefaultSignalController component = new(controller);
                template.Components.Add(component);
            }

            return template;
        }

        private static TransformedModelTemplate PutSignalProp(this JunctionTemplate template,
            string modelKey, Pose pose, ISignalController controller, string groupKey, int lamp)
        {
            SignalPropCollection component = template.Components.GetOrAdd(() =>
            {
                SignalPropCollection component = new();
                template.Components.Add(component);
                return component;
            });

            TransformedModelTemplate prop = template.PutProp(modelKey, pose);
            (prop as StaticTransformedModelTemplate)?.ProhibitMerge();
            component.Add(prop, controller, groupKey, (SignalLampRole)lamp);

            return prop;
        }

        public static TransformedModelTemplate PutSignalProp(this JunctionTemplate template,
            string modelKey, Pose pose, string controllerKey, string groupKey, int lamp)
        {
            TrafficSignals signals = template.World.Commander.Component<TrafficSignals>();
            if (!signals.Controllers.GetValue(controllerKey, out ISignalController controller))
            {
                return template.PutProp(modelKey, pose);
            }

            return PutSignalProp(template, modelKey, pose, controller, groupKey, lamp);
        }

        public static TransformedModelTemplate PutSignalProp(this JunctionTemplate template,
            string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, string controllerKey, string groupKey, int lamp)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutSignalProp(template, modelKey, position.ToPose(), controllerKey, groupKey, lamp);
        }

        public static TransformedModelTemplate PutSignalProp(this JunctionTemplate template,
            string modelKey, double x, double y, double z, string controllerKey, string groupKey, int lamp)
        {
            return PutSignalProp(template, modelKey, x, y, z, 0, 0, 0, controllerKey, groupKey, lamp);
        }

        public static TransformedModelTemplate PutSignalProp(this JunctionTemplate template,
            string modelKey, Pose pose, string groupKey, int lamp)
        {
            if (!template.Components.TryGet<DefaultSignalController>(out DefaultSignalController? defaultComponent))
            {
                ScriptError error = new(ErrorLevel.Error, $"親となるジャンクションに既定の信号制御機が指定されていません。");
                template.World.ErrorCollector.Report(error);
                return template.PutProp(modelKey, pose);
            }

            return PutSignalProp(template, modelKey, pose, defaultComponent.Controller, groupKey, lamp);
        }

        public static TransformedModelTemplate PutSignalProp(this JunctionTemplate template,
            string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, string groupKey, int lamp)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutSignalProp(template, modelKey, position.ToPose(), groupKey, lamp);
        }

        public static TransformedModelTemplate PutSignalProp(this JunctionTemplate template,
            string modelKey, double x, double y, double z, string groupKey, int lamp)
        {
            return PutSignalProp(template, modelKey, x, y, z, 0, 0, 0, groupKey, lamp);
        }
    }
}
