using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

using Bus.Common.Extensions.Networks.Elements;

namespace Bus.Common.Scripting.Commands
{
    public class SplineTemplate
    {
        private readonly ScriptWorld World;

        public LaneLayout OutletLayout { get; }

        private readonly List<SplineStructure> StructuresKey = new();
        public IReadOnlyList<SplineStructure> Structures => StructuresKey;

        internal SplineTemplate(ScriptWorld world, string outletLayoutKey)
        {
            World = world;
            OutletLayout = World.Commander.Network.LaneLayouts.Get(outletLayoutKey);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval)
        {
            LocatedModelTemplate[] models = modelKeys.Select(key =>
            {
                if (!World.Models.TryGetValue(key, out IModel? model))
                {
                    ScriptError error = new(ErrorLevel.Error, $"モデル '{key}' が見つかりません。");
                    World.ErrorCollector.Report(error);

                    model = Model.Empty();
                }

                return new LocatedModelTemplate(model, pose);
            }).ToArray();
            SplineStructure structure = new(models, (float)from, (float)span, (float)interval, int.MaxValue);
            StructuresKey.Add(structure);
            return structure;
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKeys, position.ToPose(), from, span, interval);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, double x, double y, double z, double from, double span, double interval)
        {
            return PutStructure(modelKeys, x, y, z, 0, 0, 0, from, span, interval);
        }

        internal SplineFactory Build(int plateX, int plateZ, Pose pose, NetworkPort? sourcePort)
        {
            SplineFactory factory = new SplineFactory(World.DXHost.Device, World.PhysicsHost, plateX, plateZ, pose, OutletLayout, sourcePort);
            factory.PutStructures(Structures);
            return factory;
        }
    }
}
