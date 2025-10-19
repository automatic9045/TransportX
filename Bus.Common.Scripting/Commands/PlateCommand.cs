using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;
using Bus.Common.Scenery;

namespace Bus.Common.Scripting.Commands
{
    public class PlateCommand
    {
        private readonly ScriptWorld World;
        private readonly int X;
        private readonly int Z;

        private readonly Plate Target;

        internal PlateCommand(ScriptWorld world, int x, int z)
        {
            World = world;
            X = x;
            Z = z;

            Target = World.Plates.GetOrAdd(X, Z);
        }

        public LocatedModel PutStructure(string modelKey, Matrix4x4 locator)
        {
            LocatedModel locatedModel = DynamicLocatedModel.CreateKinematicOrNonCollision(World.PhysicsHost.Simulation, World.Models[modelKey], locator);
            Target.Models.Add(locatedModel);
            return locatedModel;
        }

        public LocatedModel PutStructure(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF locator = new SixDoF((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKey, locator.CreateTransform());
        }

        public LocatedModel PutStructure(string modelKey, double x, double y, double z)
        {
            return PutStructure(modelKey, x, y, z, 0, 0, 0);
        }

        public SplineCommand BeginSpline(Matrix4x4 locator, string? templateKey = null)
        {
            SplineFactory splineFactory;
            if (templateKey is null)
            {
                splineFactory = new SplineFactory(World.PhysicsHost.Simulation, X, Z, locator, new LaneConnector());
            }
            else
            {
                SplineTemplate template = World.Commander.Splines.Templates[templateKey];
                splineFactory = template.Build(X, Z, locator);

            }

            return new SplineCommand(World, splineFactory);
        }

        public SplineCommand BeginSpline(double x, double y, double z, double rotationX, double rotationY, double rotationZ, string? templateKey = null)
        {
            SixDoF locator = new SixDoF((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return BeginSpline(locator.CreateTransform(), templateKey);
        }

        public SplineCommand BeginSpline(double x, double y, double z, string? templateKey = null)
        {
            return BeginSpline(x, y, z, 0, 0, 0, templateKey);
        }
    }
}
