using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Extensions.Network.Elements;

using TransportX.Scripting.Collections;

namespace TransportX.Scripting.Worlds.Commands
{
    public class JunctionFactoryCommand
    {
        private readonly ScriptWorld World;
        private readonly List<TransformedModelTemplate> Props = [];

        public Junction Junction { get; }
        public IReadOnlyScriptKeyedList<string, JunctionCurveFactoryCommand> Curves { get; }
        public IReadOnlyScriptKeyedList<string, JunctionPathFactoryCommand> Paths { get; }

        public string? Key { get; set; } = null;

        public IComponentCollection<ITemplateComponent<Junction>> Components { get; } = new ComponentCollection<ITemplateComponent<Junction>>();

        public JunctionFactoryCommand(ScriptWorld world, Junction junction,
            IReadOnlyScriptKeyedList<string, JunctionPathFactoryCommand> paths, IReadOnlyScriptKeyedList<string, JunctionCurveFactoryCommand> curves)
        {
            World = world;
            Junction = junction;
            Paths = paths;
            Curves = curves;
        }

        public JunctionFactoryCommand(ScriptWorld world, Junction junction) : this(world, junction,
            new ScriptKeyedList<string, JunctionPathFactoryCommand>(path => path.Key, world.ErrorCollector, "進路パス", key => JunctionPathFactoryCommand.Empty(world, junction)),
            new ScriptKeyedList<string, JunctionCurveFactoryCommand>(path => path.Key, world.ErrorCollector, "曲線", key => JunctionCurveFactoryCommand.Empty(world)))
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
            ModelResourceSet model = World.Models.GetModel(modelKey);
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

        public JunctionCommand Build()
        {
            ReadOnlySpan<CurveFactoryCommandBase> curves = [.. Curves, .. Paths];

            List<TransformedModelTemplate> models = [.. Props];
            foreach (CurveFactoryCommandBase curve in curves)
            {
                foreach (SplineProp prop in curve.Props)
                {
                    IEnumerable<TransformedModelTemplate> props = prop.Build(World.PhysicsHost, curve.Curve);
                    models.AddRange(props);
                }
            }
            Junction.AddProps(World.GraphicsHost.Device, World.PhysicsHost, models);

            foreach (CurveFactoryCommandBase curve in curves)
            {
                Junction.PutExtrusions(World.GraphicsHost.Device, World.PhysicsHost, curve.Curve, curve.Extrusions);
            }

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
