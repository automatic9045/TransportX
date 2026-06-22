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

namespace TransportX.Scripting.Worlds.Commands
{
    public class ChunkCommand
    {
        private readonly ScriptWorld World;
        private readonly Chunk Target;

        private int SplineCounter = 0;
        private int JunctionCounter = 0;

        internal ChunkCommand(ScriptWorld world, ChunkIndex index)
        {
            World = world;
            Target = World.Chunks.GetOrAdd(index);
        }

        public TransformedModel PutProp(string modelKey, Pose pose)
        {
            IModel model = World.Models.GetModel(modelKey);
            TransformedModel transformedModel = StaticTransformedModel.CreateStaticOrNonCollision(World.PhysicsHost, model, pose);
            Target.Models.Add(transformedModel);
            return transformedModel;
        }

        public TransformedModel PutProp(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutProp(modelKey, position.ToPose());
        }

        public TransformedModel PutProp(string modelKey, double x, double y, double z)
        {
            return PutProp(modelKey, x, y, z, 0, 0, 0);
        }

        public SplineFactoryCommand BeginSpline(string? templateKey, Pose pose, NetworkPort? sourcePort = null)
        {
            WorldPose worldPose = new(Target.Index, pose);

            SplineFactoryCommand? factoryCommand = null;
            if (templateKey is not null)
            {
                SplineTemplate? template = World.Commander.Network.Templates.GetSpline(templateKey);
                if (template is not null)
                {
                    factoryCommand = template.Build(worldPose, sourcePort);
                }
            }

            factoryCommand ??= new SplineFactoryCommand(World, new SplineFactory(worldPose, new LaneLayout(), sourcePort));
            factoryCommand.SplineFactory.DebugName = CreateSplineDebugName(templateKey);

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

        public JunctionFactoryCommand PutJunction(string templateKey, Pose pose)
        {
            WorldPose worldPose = new(Target.Index, pose);

            JunctionFactoryCommand factoryCommand;
            if (World.Commander.Network.Templates.Junctions.TryGetValue(templateKey, out JunctionTemplate? template))
            {
                factoryCommand = template.Build(worldPose);
            }
            else
            {
                ScriptError error = new(ErrorLevel.Error, $"ジャンクションテンプレート '{templateKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                factoryCommand = new JunctionFactoryCommand(World, new Junction(worldPose, []));
            }
            factoryCommand.Junction.DebugName = CreateJunctionDebugName(templateKey);

            Target.Network.Add(factoryCommand.Junction);
            return factoryCommand;
        }

        public JunctionFactoryCommand PutJunction(string templateKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutJunction(templateKey, position.ToPose());
        }

        public JunctionFactoryCommand PutJunction(string templateKey, double x, double y, double z)
        {
            return PutJunction(templateKey, x, y, z, 0, 0, 0);
        }

        internal string CreateSplineDebugName(string? templateKey)
        {
            return $"{templateKey ?? "Spline"}_{Target.Index.X};{Target.Index.Z};{SplineCounter++}";
        }

        internal string CreateJunctionDebugName(string? templateKey)
        {
            return $"{templateKey ?? "Junction"}_{Target.Index.X};{Target.Index.Z};{JunctionCounter++}";
        }
    }
}
