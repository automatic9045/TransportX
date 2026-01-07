using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Collections;
using Bus.Common.Diagnostics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

using Bus.Common.Extensions.Networks;

namespace Bus.Common.Scripting.Commands
{
    public class JunctionTemplate
    {
        private readonly ScriptWorld World;

        private readonly KeyedList<string, PortDefinition> PortsKey = new KeyedList<string, PortDefinition>(item => item.Name);
        public IReadOnlyKeyedList<string, PortDefinition> Ports => PortsKey;

        private readonly List<PathDefinition> PathsKey = [];
        public IReadOnlyList<PathDefinition> Paths => PathsKey;

        private readonly List<LocatedModelTemplate> StructuresKey = [];
        public IReadOnlyList<LocatedModelTemplate> Structures => StructuresKey;

        internal JunctionTemplate(ScriptWorld world)
        {
            World = world;
        }

        public void AddPort(string key, string layoutKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            LaneLayout layout = World.Commander.Network.LaneLayouts.Get(layoutKey);
            SixDoF offset = SixDoF.Deg((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            PortDefinition port = new(key, layout, offset.CreateTransform());
            PortsKey.Add(port);
        }

        internal PortDefinition? GetPort(string portKey)
        {
            if (Ports.TryGetValue(portKey, out PortDefinition? port)) return port;

            ScriptError error = new(ErrorLevel.Error, $"進路端子 '{portKey}' が見つかりません。");
            World.ErrorCollector.Report(error);

            return null;
        }

        public void Wire(string fromKey, int fromPin, string toKey, int toPin)
        {
            if (!CheckPin(fromKey, fromPin)) return;
            if (!CheckPin(toKey, toPin)) return;

            PathDefinition path = new(fromKey, fromPin, toKey, toPin);
            PathsKey.Add(path);


            bool CheckPin(string portKey, int pinIndex)
            {
                PortDefinition? port = GetPort(portKey);
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

        public LocatedModelTemplate PutStructure(string modelKey, Matrix4x4 transform)
        {
            if (!World.Models.TryGetValue(modelKey, out IModel? model))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデル '{modelKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                model = Model.Empty();
            }

            LocatedModelTemplate structure = KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(World.PhysicsHost, model, transform);
            StructuresKey.Add(structure);
            return structure;
        }

        public LocatedModelTemplate PutStructure(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF transform = SixDoF.Deg((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKey, transform.CreateTransform());
        }

        public LocatedModelTemplate PutStructure(string modelKey, double x, double y, double z)
        {
            return PutStructure(modelKey, x, y, z, 0, 0, 0);
        }

        internal Junction Build(int plateX, int plateZ, Matrix4x4 transform)
        {
            Junction junction = new(plateX, plateZ, transform, Ports);

            foreach (PathDefinition path in PathsKey)
            {
                LanePin from = junction.Ports[path.FromPortKey].Pins[path.FromPinIndex];
                LanePin to = junction.Ports[path.ToPortKey].Pins[path.ToPinIndex];
                junction.Wire(from, to);
            }

            foreach (LocatedModelTemplate model in Structures)
            {
                LocatedModel structure = model.Build(localTransform => localTransform * junction.Transform);
                junction.AddStructure(structure);
            }

            return junction;
        }


        public struct PathDefinition
        {
            public string FromPortKey { get; }
            public int FromPinIndex { get; }
            public string ToPortKey { get; }
            public int ToPinIndex { get; }

            public PathDefinition(string fromPortKey, int fromPinIndex, string toPortKey, int toPinIndex)
            {
                FromPortKey = fromPortKey;
                FromPinIndex = fromPinIndex;
                ToPortKey = toPortKey;
                ToPinIndex = toPinIndex;
            }
        }
    }
}
