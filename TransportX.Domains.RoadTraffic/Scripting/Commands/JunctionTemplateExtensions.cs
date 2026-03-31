using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Scripting;
using TransportX.Scripting.Commands;

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

        private static LocatedModelTemplate PutSignalStructure(this JunctionTemplate template,
            string modelKey, Pose pose, ISignalController controller, string groupKey, int lamp)
        {
            SignalStructureCollection component = template.Components.GetOrAdd(() =>
            {
                SignalStructureCollection component = new SignalStructureCollection();
                template.Components.Add(component);
                return component;
            });

            LocatedModelTemplate structure = template.PutStructure(modelKey, pose);
            (structure as KinematicLocatedModelTemplate)?.ProhibitMerge();
            component.Add(structure, controller, groupKey, (SignalLampRole)lamp);

            return structure;
        }

        public static LocatedModelTemplate PutSignalStructure(this JunctionTemplate template,
            string modelKey, Pose pose, string controllerKey, string groupKey, int lamp)
        {
            TrafficSignals signals = template.World.Commander.Component<TrafficSignals>();
            if (!signals.Controllers.GetValue(controllerKey, out ISignalController controller))
            {
                return template.PutStructure(modelKey, pose);
            }

            return PutSignalStructure(template, modelKey, pose, controller, groupKey, lamp);
        }

        public static LocatedModelTemplate PutSignalStructure(this JunctionTemplate template,
            string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, string controllerKey, string groupKey, int lamp)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutSignalStructure(template, modelKey, position.ToPose(), controllerKey, groupKey, lamp);
        }

        public static LocatedModelTemplate PutSignalStructure(this JunctionTemplate template,
            string modelKey, double x, double y, double z, string controllerKey, string groupKey, int lamp)
        {
            return PutSignalStructure(template, modelKey, x, y, z, 0, 0, 0, controllerKey, groupKey, lamp);
        }

        public static LocatedModelTemplate PutSignalStructure(this JunctionTemplate template,
            string modelKey, Pose pose, string groupKey, int lamp)
        {
            if (!template.Components.TryGet<DefaultSignalController>(out DefaultSignalController? defaultComponent))
            {
                ScriptError error = new(ErrorLevel.Error, $"親となるジャンクションに既定の信号制御機が指定されていません。");
                template.World.ErrorCollector.Report(error);
                return template.PutStructure(modelKey, pose);
            }

            return PutSignalStructure(template, modelKey, pose, defaultComponent.Controller, groupKey, lamp);
        }

        public static LocatedModelTemplate PutSignalStructure(this JunctionTemplate template,
            string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, string groupKey, int lamp)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutSignalStructure(template, modelKey, position.ToPose(), groupKey, lamp);
        }

        public static LocatedModelTemplate PutSignalStructure(this JunctionTemplate template,
            string modelKey, double x, double y, double z, string groupKey, int lamp)
        {
            return PutSignalStructure(template, modelKey, x, y, z, 0, 0, 0, groupKey, lamp);
        }
    }
}
