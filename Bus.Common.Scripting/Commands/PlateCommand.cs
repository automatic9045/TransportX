using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Rendering;
using Bus.Common.Scenery.Networks;
using Bus.Common.Scenery;

using Bus.Common.Extensions.Networks;

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

        public LocatedModel PutStructure(string modelKey, Matrix4x4 transform)
        {
            if (!World.Models.TryGetValue(modelKey, out IModel? model))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデル '{modelKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                model = Model.Empty();
            }

            LocatedModel locatedModel = KinematicLocatedModel.CreateKinematicOrNonCollision(World.PhysicsHost, model, transform);
            Target.Models.Add(locatedModel);
            return locatedModel;
        }

        public LocatedModel PutStructure(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = new SixDoF((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKey, position.CreateTransform());
        }

        public LocatedModel PutStructure(string modelKey, double x, double y, double z)
        {
            return PutStructure(modelKey, x, y, z, 0, 0, 0);
        }

        public SplineCommand BeginSpline(string? templateKey, Matrix4x4 transform)
        {
            SplineFactory splineFactory;
            if (templateKey is null)
            {
                splineFactory = new SplineFactory(World.DXHost.Device, World.PhysicsHost, X, Z, transform, new LaneLayout());
            }
            else if (!World.Commander.Network.Templates.Splines.TryGetValue(templateKey, out SplineTemplate? template))
            {
                ScriptError error = new(ErrorLevel.Error, $"スプラインテンプレート '{templateKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                splineFactory = new SplineFactory(World.DXHost.Device, World.PhysicsHost, X, Z, transform, new LaneLayout());
            }
            else
            {
                splineFactory = template.Build(X, Z, transform);
            }

            return new SplineCommand(World, splineFactory);
        }

        public SplineCommand BeginSpline(Matrix4x4 transform)
        {
            return BeginSpline(null, transform);
        }

        public SplineCommand BeginSpline(string? templateKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = new SixDoF((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return BeginSpline(templateKey, position.CreateTransform());
        }

        public SplineCommand BeginSpline(double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            return BeginSpline(null, x, y, z, rotationX, rotationY, rotationZ);
        }

        public SplineCommand BeginSpline(string? templateKey, double x, double y, double z)
        {
            return BeginSpline(templateKey, x, y, z, 0, 0, 0);
        }

        public SplineCommand BeginSpline(double x, double y, double z)
        {
            return BeginSpline(null, x, y, z);
        }
    }
}
