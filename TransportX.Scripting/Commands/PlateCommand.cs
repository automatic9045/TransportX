using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Commands
{
    public class PlateCommand
    {
        private readonly ScriptWorld World;
        private readonly int X;
        private readonly int Z;

        private readonly Plate Target;

        private int SplineCounter = 0;
        private int JunctionCounter = 0;

        internal PlateCommand(ScriptWorld world, int x, int z)
        {
            World = world;
            X = x;
            Z = z;

            Target = World.Plates.GetOrAdd(X, Z);
        }

        public LocatedModel PutStructure(string modelKey, Pose pose)
        {
            if (!World.Models.TryGetValue(modelKey, out IModel? model))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデル '{modelKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                model = Model.Empty();
            }

            LocatedModel locatedModel = KinematicLocatedModel.CreateKinematicOrNonCollision(World.PhysicsHost, model, pose);
            Target.Models.Add(locatedModel);
            return locatedModel;
        }

        public LocatedModel PutStructure(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKey, position.ToPose());
        }

        public LocatedModel PutStructure(string modelKey, double x, double y, double z)
        {
            return PutStructure(modelKey, x, y, z, 0, 0, 0);
        }

        public SplineFactoryCommand BeginSpline(string? templateKey, Pose pose, NetworkPort? sourcePort = null)
        {
            SplineFactory? splineFactory = null;
            SplineTemplate? template = null;
            if (templateKey is not null)
            {
                template = World.Commander.Network.Templates.GetSpline(templateKey);
                if (template is not null)
                {
                    splineFactory = template.Build(X, Z, pose, sourcePort);
                }
            }
            splineFactory ??= new SplineFactory(World.DXHost.Device, World.PhysicsHost, X, Z, pose, new LaneLayout(), sourcePort);
            splineFactory.DebugName = CreateSplineDebugName(templateKey);

            SplineFactoryCommand factoryCommand = new(World, splineFactory);
            template?.CopyComponentsTo(factoryCommand);

            return factoryCommand;
        }

        public SplineFactoryCommand BeginSpline(Pose pose)
        {
            return BeginSpline(null, pose);
        }

        public SplineFactoryCommand BeginSpline(string? templateKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return BeginSpline(templateKey, position.ToPose());
        }

        public SplineFactoryCommand BeginSpline(double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            return BeginSpline(null, x, y, z, rotationX, rotationY, rotationZ);
        }

        public SplineFactoryCommand BeginSpline(string? templateKey, double x, double y, double z)
        {
            return BeginSpline(templateKey, x, y, z, 0, 0, 0);
        }

        public SplineFactoryCommand BeginSpline(double x, double y, double z)
        {
            return BeginSpline(null, x, y, z);
        }

        public JunctionCommand PutJunction(string templateKey, Pose pose)
        {
            Junction junction;
            if (World.Commander.Network.Templates.Junctions.TryGetValue(templateKey, out JunctionTemplate? template))
            {
                junction = template.Build(X, Z, pose);
            }
            else
            {
                ScriptError error = new(ErrorLevel.Error, $"ジャンクションテンプレート '{templateKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                junction = new Junction(X, Z, pose, []);
            }
            junction.DebugName = CreateJunctionDebugName(templateKey);

            Target.Network.Add(junction);
            return new JunctionCommand(World, junction);
        }

        public JunctionCommand PutJunction(string templateKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutJunction(templateKey, position.ToPose());
        }

        public JunctionCommand PutJunction(string templateKey, double x, double y, double z)
        {
            return PutJunction(templateKey, x, y, z, 0, 0, 0);
        }

        internal string CreateSplineDebugName(string? templateKey)
        {
            return $"{templateKey ?? "Spline"}_{X};{Z};{SplineCounter++}";
        }

        internal string CreateJunctionDebugName(string? templateKey)
        {
            return $"{templateKey ?? "Junction"}_{X};{Z};{JunctionCounter++}";
        }
    }
}
