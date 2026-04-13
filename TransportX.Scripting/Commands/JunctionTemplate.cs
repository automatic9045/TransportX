using System;
using System.Collections.Concurrent;
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

using TransportX.Scripting.Collections;

namespace TransportX.Scripting.Commands
{
    public class JunctionTemplate
    {
        private readonly ScriptKeyedList<string, PortDefinition> PortsKey;
        public IReadOnlyKeyedList<string, PortDefinition> Ports => PortsKey;

        private readonly List<(string Input, string Output)> Relays = [];

        private readonly ScriptKeyedList<string, JunctionPathTemplate> PathsKey;
        public IReadOnlyKeyedList<string, JunctionPathTemplate> Paths => PathsKey;

        private readonly List<LocatedModelTemplate> StructuresKey = [];
        public IReadOnlyList<LocatedModelTemplate> Structures => StructuresKey;

        public ScriptWorld World { get; }
        public IComponentCollection<ITemplateComponent<Junction>> Components { get; } = new ComponentCollection<ITemplateComponent<Junction>>();

        internal JunctionTemplate(ScriptWorld world)
        {
            World = world;

            PortsKey = new ScriptKeyedList<string, PortDefinition>(item => item.Name,
                World.ErrorCollector, "進路端子", key => new PortDefinition(string.Empty, new LaneLayout(), Pose.Identity));
            PathsKey = new ScriptKeyedList<string, JunctionPathTemplate>(item => item.Key,
                World.ErrorCollector, "進路パス", key => JunctionPathTemplate.Empty(World, this));
        }

        public void AddPort(string key, string layoutKey, Pose offset)
        {
            LaneLayout layout = World.Commander.Network.LaneLayouts[layoutKey];
            PortDefinition port = new(key, layout, offset);
            PortsKey.Add(port);
        }

        public void AddPort(string key, string layoutKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF offset = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            AddPort(key, layoutKey, offset.ToPose());
        }

        public void AddRelay(string inputKey, string outputKey, string layoutKey, Pose offset)
        {
            LaneLayout layout = World.Commander.Network.LaneLayouts[layoutKey];

            PortDefinition input = new(inputKey, layout, offset);
            PortDefinition output = new(outputKey, layout.Opposition, Pose.CreateRotationY(float.Pi) * offset);
            PortsKey.Add(input);
            PortsKey.Add(output);

            Relays.Add((inputKey, outputKey));
        }

        public void AddRelay(string inputKey, string outputKey, string layoutKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF offset = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            AddRelay(inputKey, outputKey, layoutKey, offset.ToPose());
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
                if (!PortsKey.GetValue(portKey, out port)) return false;

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

        internal JunctionFactoryCommand Build(int plateX, int plateZ, Pose pose)
        {
            Junction junction = new(plateX, plateZ, pose, Ports);

            foreach ((string inputKey, string outputKey) in Relays)
            {
                junction.Ports[inputKey].ConnectTo(junction.Ports[outputKey]);
            }

            ScriptKeyedList<string, JunctionPathFactoryCommand> pathFactories = new(x => x.Key,
                Paths.Select(path =>
                {
                    JunctionPathFactoryCommand built = path.Build(junction);
                    built.Path.DebugColor = World.Commander.Network.LaneTraffic.GetGroupColor(built.Path.AllowedTraffic);

                    foreach ((Type type, ITemplateComponent<ILanePath> component) in path.Components)
                    {
                        built.Components.Add(type, component);
                    }

                    return built;
                }),
                World.ErrorCollector, "進路パス", key => JunctionPathFactoryCommand.Empty(World, junction));

            JunctionFactoryCommand factoryCommand = new(World, junction, pathFactories);
            factoryCommand.AddStructures(Structures);
            foreach ((Type type, ITemplateComponent<Junction> component) in Components)
            {
                factoryCommand.Components.Add(type, component);
            }

            return factoryCommand;
        }
    }
}
