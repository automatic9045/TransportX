using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

        public KeyValuePair<string, LaneLayout> Inlet { get; }

        private readonly List<KeyValuePair<string, OutletDefinition>> OutletsKey = [];
        public IReadOnlyList<KeyValuePair<string, OutletDefinition>> Outlets => OutletsKey;

        private readonly List<PathDefinition> PathsKey = [];
        public IReadOnlyList<PathDefinition> Paths => PathsKey;

        private readonly List<LocatedModelTemplate> StructuresKey = [];
        public IReadOnlyList<LocatedModelTemplate> Structures => StructuresKey;

        internal JunctionTemplate(ScriptWorld world, string inletKey, string inletLayoutKey)
        {
            World = world;
            Inlet = new(inletKey, World.Commander.Network.GetLaneLayout(inletLayoutKey));
        }

        public void AddOutlet(string key, string layoutKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            LaneLayout layout = World.Commander.Network.GetLaneLayout(layoutKey);
            SixDoF offset = SixDoF.Deg((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            OutletDefinition outlet = new(layout, offset.CreateTransform());
            OutletsKey.Add(new(key, outlet));
        }

        public void Wire(string fromKey, int fromPin, string toKey, int toPin)
        {
            if (!CheckPin(fromKey, fromPin, out int fromPort)) return;
            if (!CheckPin(toKey, toPin, out int toPort)) return;

            PathDefinition path = new(fromPort, fromPin, toPort, toPin);
            PathsKey.Add(path);


            bool CheckPin(string portKey, int pinIndex, out int portIndex)
            {
                if (portKey == Inlet.Key)
                {
                    portIndex = 0;
                    return CheckPinIndex(Inlet.Value, pinIndex);
                }

                (int outletIndex, _, OutletDefinition? outlet) = Outlets.Select((o, i) => (Index: i, o.Key, o.Value)).FirstOrDefault(x => x.Key == portKey);
                if (outlet is not null)
                {
                    portIndex = outletIndex + 1;
                    return CheckPinIndex(outlet.Layout, pinIndex);
                }

                ScriptError error = new(ErrorLevel.Error, $"進路端子 '{portKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                portIndex = 0;
                return false;


                bool CheckPinIndex(LaneLayout layout, int pinIndex)
                {
                    if (pinIndex < 0 || layout.Lanes.Count <= pinIndex)
                    {
                        ScriptError error = new(ErrorLevel.Error, $"進路端子 '{portKey}' にピン {pinIndex} は存在しません。有効なピン番号は 0 以上 {layout.Lanes.Count} 未満です。");
                        World.ErrorCollector.Report(error);

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
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
            Junction junction = new(plateX, plateZ, transform, Inlet.Value, Outlets.Select(o => o.Value));

            foreach (PathDefinition path in PathsKey)
            {
                LanePin from = junction.Ports[path.FromPortIndex].Pins[path.FromPinIndex];
                LanePin to = junction.Ports[path.ToPortIndex].Pins[path.ToPinIndex];
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
            public int FromPortIndex { get; }
            public int FromPinIndex { get; }
            public int ToPortIndex { get; }
            public int ToPinIndex { get; }

            public PathDefinition(int fromPortIndex, int fromPinIndex, int toPortIndex, int toPinIndex)
            {
                FromPortIndex = fromPortIndex;
                FromPinIndex = fromPinIndex;
                ToPortIndex = toPortIndex;
                ToPinIndex = toPinIndex;
            }
        }
    }
}
