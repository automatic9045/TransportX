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
            Spline spline = SplineFactory.Straight((float)length);
            AddSplineToPlate(spline);
            return spline;
        }

        public Spline Curve(double radius, double length)
        {
            Spline spline = SplineFactory.ByRadius((float)radius, (float)length);
            AddSplineToPlate(spline);
            return spline;
        }

        public Spline CurveByCurvature(double curvature, double length)
        {
            Spline spline = SplineFactory.ByCurvature((float)curvature, (float)length);
            AddSplineToPlate(spline);
            return spline;
        }

        private void AddSplineToPlate(Spline spline)
        {
            Plate plate = World.Plates.GetOrAdd(spline.PlateX, spline.PlateZ);
            plate.Network.Add(spline);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, Matrix4x4 transform, double from, double span, double interval, int count = int.MaxValue)
        {
            LocatedModel[] models = modelKeys.Select(key =>
            {
                if (!World.Models.TryGetValue(key, out IModel? model))
                {
                    ScriptError error = new(ErrorLevel.Error, $"モデル '{key}' が見つかりません。");
                    World.ErrorCollector.Report(error);

                    model = Model.Empty();
                }

                return KinematicLocatedModel.CreateKinematicOrNonCollision(World.PhysicsHost, model, transform);
            }).ToArray();
            SplineStructure structure = new SplineStructure(models, (float)from, (float)span, (float)interval, count);
            SplineFactory.PutStructure(structure);

            foreach (LocatedModel model in models)
            {
                if (model is CollidableLocatedModel collidableModel) collidableModel.Dispose();
            }

            return structure;
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF position = new SixDoF((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKeys, position.CreateTransform(), from, span, interval, count);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
        {
            return PutStructure(modelKeys, x, y, z, 0, 0, 0, from, span, interval, count);
        }
    }
}
