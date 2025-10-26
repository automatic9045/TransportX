using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scripting.Commands
{
    public class SplineTemplate
    {
        private readonly ScriptWorld World;
        private readonly LaneConnector Port;

        private readonly List<SplineStructure> StructuresKey = new();
        public IReadOnlyList<SplineStructure> Structures => StructuresKey;

        internal SplineTemplate(ScriptWorld world, string portKey)
        {
            World = world;
            Port = World.Commander.Splines.Ports[portKey];
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, Matrix4x4 transform, double span, double interval)
        {
            LocatedModel[] models = modelKeys.Select(
                key => DynamicLocatedModel.CreateKinematicOrNonCollision(World.PhysicsHost, World.Models[key], transform)).ToArray();
            SplineStructure structure = new SplineStructure(models, 0, (float)span, (float)interval, int.MaxValue);
            StructuresKey.Add(structure);
            return structure;
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double span, double interval)
        {
            SixDoF position = new SixDoF((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKeys, position.CreateTransform(), span, interval);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, double x, double y, double z, double span, double interval)
        {
            return PutStructure(modelKeys, x, y, z, 0, 0, 0, span, interval);
        }

        internal SplineFactory Build(int plateX, int plateZ, Matrix4x4 transform)
        {
            SplineFactory factory = new SplineFactory(World.PhysicsHost, plateX, plateZ, transform, Port);
            return factory;
        }
    }
}
