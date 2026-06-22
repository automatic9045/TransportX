using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Network;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Worlds.Commands
{
    public class JunctionFactoryCommand
    {
        private readonly ScriptWorld World;

        private readonly IReadOnlyKeyedList<string, JunctionPathFactoryCommand> Paths;
        private readonly List<TransformedModelTemplate> Props = [];

        public Junction Junction { get; }
        public string? Key { get; set; } = null;

        public IComponentCollection<ITemplateComponent<Junction>> Components { get; } = new ComponentCollection<ITemplateComponent<Junction>>();

        public JunctionFactoryCommand(ScriptWorld world, Junction junction, IReadOnlyKeyedList<string, JunctionPathFactoryCommand> paths)
        {
            World = world;
            Junction = junction;
            Paths = paths;
        }

        public JunctionFactoryCommand(ScriptWorld world, Junction junction)
            : this(world, junction, new KeyedList<string, JunctionPathFactoryCommand>(path => path.Key))
        {
        }

        public void AddProp(TransformedModelTemplate prop)
        {
            Props.Add(prop);
        }

        public void AddProps(IEnumerable<TransformedModelTemplate> props)
        {
            Props.AddRange(props);
        }

        public TransformedModelTemplate PutProp(string modelKey, Pose pose)
        {
            IModel model = World.Models.GetModel(modelKey);
            TransformedModelTemplate prop = StaticTransformedModelTemplate.CreateStaticOrNonCollision(World.PhysicsHost, model, pose);
            AddProp(prop);
            return prop;
        }

        public TransformedModelTemplate PutProp(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutProp(modelKey, position.ToPose());
        }

        public TransformedModelTemplate PutProp(string modelKey, double x, double y, double z)
        {
            return PutProp(modelKey, x, y, z, 0, 0, 0);
        }

        public SplineProp PutPathProp(IReadOnlyList<string> modelKeys, string pathKey,
            Pose pose, double from, double span, double interval, int count = int.MaxValue)
        {
            SplineProp prop;
            if (!Paths.TryGetValue(pathKey, out JunctionPathFactoryCommand? path))
            {
                ScriptError error = new(ErrorLevel.Error, $"進路パス '{pathKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                prop = new([], 0, 0, 0, 0);
                return prop;
            }

            prop = path.PutProp(modelKeys, pose, from, span, interval, count);
            return prop;
        }

        public SplineProp PutPathProp(IReadOnlyList<string> modelKeys, string pathKey,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutPathProp(modelKeys, pathKey, position.ToPose(), from, span, interval, count);
        }

        public SplineProp PutPathProp(IReadOnlyList<string> modelKeys, string pathKey,
            double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
        {
            return PutPathProp(modelKeys, pathKey, x, y, z, 0, 0, 0, from, span, interval, count);
        }

        public JunctionCommand Build()
        {
            foreach (JunctionPathFactoryCommand path in Paths)
            {
                List<TransformedModelTemplate> props = path.BuildProps();
                Props.AddRange(props);
            }

            Junction.PutProps(World.DXHost.Device, World.PhysicsHost, Props);

            IErrorCollector componentErrorCollector = IErrorCollector.Default();
            componentErrorCollector.Reported += (sender, e) =>
            {
                ScriptError error = ScriptError.CreateFrom(e.Error);
                World.ErrorCollector.Report(error);
            };
            foreach (JunctionPathFactoryCommand path in Paths)
            {
                path.BuildComponents(componentErrorCollector);
            }
            foreach (ITemplateComponent<Junction> component in Components.Values)
            {
                component.Build(Junction, componentErrorCollector);
            }

            JunctionCommand junctionCommand = new(World, Junction);
            if (Key is not null)
            {
                World.Commander.Network.JunctionsKey[Key] = junctionCommand;
            }

            return junctionCommand;
        }
    }
}
