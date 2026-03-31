using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Network;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Commands
{
    public class JunctionTemplate
    {
        private readonly KeyedList<string, PortDefinition> PortsKey = new KeyedList<string, PortDefinition>(item => item.Name);
        public IReadOnlyKeyedList<string, PortDefinition> Ports => PortsKey;

        private readonly List<JunctionPathTemplate> PathsKey = [];
        public IReadOnlyList<JunctionPathTemplate> Paths => PathsKey;

        private readonly List<LocatedModelTemplate> StructuresKey = [];
        public IReadOnlyList<LocatedModelTemplate> Structures => StructuresKey;

        public ScriptWorld World { get; }
        public IComponentCollection<ITemplateComponent<Junction>> Components { get; } = new ComponentCollection<ITemplateComponent<Junction>>();

        internal JunctionTemplate(ScriptWorld world)
        {
            World = world;
        }

        public void AddPort(string key, string layoutKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            LaneLayout layout = World.Commander.Network.LaneLayouts[layoutKey];
            SixDoF offset = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            PortDefinition port = new(key, layout, offset.ToPose());
            PortsKey.Add(port);
        }

        internal PortDefinition? GetPort(string portKey)
        {
            if (Ports.TryGetValue(portKey, out PortDefinition? port)) return port;

            ScriptError error = new(ErrorLevel.Error, $"進路端子 '{portKey}' が見つかりません。");
            World.ErrorCollector.Report(error);

            return null;
        }

        public JunctionPathTemplate Wire(string key, string fromPortKey, int fromPinIndex, string toPortKey, int toPinIndex)
        {
            if (!CheckPin(fromPortKey, fromPinIndex, out PortDefinition? fromPort)) return JunctionPathTemplate.Empty(World, this);
            if (!CheckPin(toPortKey, toPinIndex, out PortDefinition? toPort)) return JunctionPathTemplate.Empty(World, this);

            JunctionPathTemplate path = new(World, this, key, fromPort, fromPinIndex, toPort, toPinIndex);
            PathsKey.Add(path);

            return path;


            bool CheckPin(string portKey, int pinIndex, [MaybeNullWhen(false)] out PortDefinition port)
            {
                port = GetPort(portKey);
                if (port is null) return false;

                if (pinIndex < 0 || port.Layout.Lanes.Count <= pinIndex)
                {
                    ScriptError error = new(ErrorLevel.Error,
                        $"進路端子 '{portKey}' にピン {pinIndex} は存在しません。有効なピン番号は 0 以上 {port.Layout.Lanes.Count} 未満です。");
                    World.ErrorCollector.Report(error);

                    return false;
                }

                return true;
            }
        }

        public LocatedModelTemplate PutStructure(string modelKey, Pose pose)
        {
            if (!World.Models.TryGetValue(modelKey, out IModel? model))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデル '{modelKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                model = Model.Empty();
            }

            LocatedModelTemplate structure = KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(World.PhysicsHost, model, pose);
            StructuresKey.Add(structure);
            return structure;
        }

        public LocatedModelTemplate PutStructure(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKey, position.ToPose());
        }

        public LocatedModelTemplate PutStructure(string modelKey, double x, double y, double z)
        {
            return PutStructure(modelKey, x, y, z, 0, 0, 0);
        }

        internal Junction Build(int plateX, int plateZ, Pose pose)
        {
            Junction junction = new(plateX, plateZ, pose, Ports);

            List<(JunctionPathTemplate, ILanePath)> paths = new(PathsKey.Count);
            foreach (JunctionPathTemplate path in PathsKey)
            {
                ILanePath built = path.Build(junction);
                built.DebugColor = World.Commander.Network.LaneTraffic.GetGroupColor(built.AllowedTraffic);
                paths.Add((path, built));
            }

            foreach (LocatedModelTemplate structure in Structures)
            {
                junction.AddStructure(structure);
            }
            junction.BuildStructures(World.DXHost.Device, World.PhysicsHost);

            IErrorCollector componentErrorCollector = IErrorCollector.Default();
            componentErrorCollector.Reported += (sender, e) =>
            {
                ScriptError error = ScriptError.CreateFrom(e.Error);
                World.ErrorCollector.Report(error);
            };
            foreach ((JunctionPathTemplate template, ILanePath built) in paths)
            {
                template.BuildComponents(built, componentErrorCollector);
            }
            foreach (ITemplateComponent<Junction> component in Components.Values)
            {
                component.Build(junction, componentErrorCollector);
            }

            return junction;
        }
    }
}
