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
    public class SplineCommand
    {
        private readonly ScriptWorld World;
        private readonly SplineFactory SplineFactory;

        internal SplineCommand(ScriptWorld world, SplineFactory splineFactory)
        {
            World = world;
            SplineFactory = splineFactory;
        }

        public Spline Straight(double length)
        {
            Spline spline = SplineFactory.Straight(length);
            AddSplineToPlate(spline);
            return spline;
        }

        public Spline Curve(double radius, double length)
        {
            Spline spline = SplineFactory.ByRadius(radius, length);
            AddSplineToPlate(spline);
            return spline;
        }

        public Spline CurveByCurvature(double curvature, double length)
        {
            Spline spline = SplineFactory.ByCurvature(curvature, length);
            AddSplineToPlate(spline);
            return spline;
        }

        private void AddSplineToPlate(Spline spline)
        {
            Plate plate = World.Plates.GetOrAdd(spline.PlateX, spline.PlateZ);
            plate.Network.Add(spline);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, Matrix4x4 locator, double from, double span, double interval, int count = int.MaxValue)
        {
            LocatedModel[] models = modelKeys.Select(key => LocatedModel.CreateStaticOrNonCollision(World.PhysicsHost.Simulation, World.Models[key], locator)).ToArray();
            SplineStructure structure = new SplineStructure(models, from, span, interval, count);
            SplineFactory.PutStructure(structure);
            return structure;
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF locator = new SixDoF((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKeys, locator.CreateTransform(), from, span, interval, count);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
        {
            return PutStructure(modelKeys, x, y, z, 0, 0, 0, from, span, interval, count);
        }
    }
}
