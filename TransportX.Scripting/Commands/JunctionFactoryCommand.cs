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

namespace TransportX.Scripting.Commands
{
    public class JunctionFactoryCommand
    {
        private readonly ScriptWorld World;

        private readonly IReadOnlyKeyedList<string, JunctionPathTemplate> Paths;
        private readonly List<LocatedModelTemplate> Structures = [];

        public Junction Junction { get; }
        public string? Key { get; set; } = null;

        public IComponentCollection<ITemplateComponent<Junction>> Components { get; } = new ComponentCollection<ITemplateComponent<Junction>>();

        public JunctionFactoryCommand(ScriptWorld world, Junction junction, IReadOnlyKeyedList<string, JunctionPathTemplate> paths)
        {
            World = world;
            Junction = junction;
            Paths = paths;
        }

        public JunctionFactoryCommand(ScriptWorld world, Junction junction) : this(world, junction, new KeyedList<string, JunctionPathTemplate>(path => path.Key))
        {
        }

        public void AddStructure(LocatedModelTemplate structure)
        {
            Structures.Add(structure);
        }

        public void AddStructures(IEnumerable<LocatedModelTemplate> structures)
        {
            Structures.AddRange(structures);
        }

        public LocatedModelTemplate PutStructure(string modelKey, Pose pose)
        {
            if (!World.Models.TryGetValue(modelKey, out IModel? model))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデル '{modelKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                model = Model.Empty();
            }

            LocatedModelTemplate structure = KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(World.PhysicsHost, model, pose);
            AddStructure(structure);
            return structure;
        }

        public LocatedModelTemplate PutStructure(string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKey, position.ToPose());
        }

        public LocatedModelTemplate PutStructure(string modelKey, double x, double y, double z)
        {
            return PutStructure(modelKey, x, y, z, 0, 0, 0);
        }

        public SplineStructure PutPathStructure(IReadOnlyList<string> modelKeys, string pathKey,
            Pose pose, double from, double span, double interval, int count = int.MaxValue)
        {
            SplineStructure structure;
            if (!Paths.TryGetValue(pathKey, out JunctionPathTemplate? path))
            {
                ScriptError error = new(ErrorLevel.Error, $"進路パス '{pathKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                structure = new([], 0, 0, 0, 0);
                return structure;
            }

            structure = path.PutStructure(modelKeys, pose, from, span, interval, count);
            return structure;
        }

        public SplineStructure PutPathStructure(IReadOnlyList<string> modelKeys, string pathKey,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval, int count = int.MaxValue)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutPathStructure(modelKeys, pathKey, position.ToPose(), from, span, interval, count);
        }

        public SplineStructure PutPathStructure(IReadOnlyList<string> modelKeys, string pathKey,
            double x, double y, double z, double from, double span, double interval, int count = int.MaxValue)
        {
            return PutPathStructure(modelKeys, pathKey, x, y, z, 0, 0, 0, from, span, interval, count);
        }

        public JunctionCommand Build()
        {
            List<(JunctionPathTemplate, ILanePath)> paths = new(Paths.Count);
            foreach (JunctionPathTemplate path in Paths)
            {
                ILanePath built = path.Build(this);
                built.DebugColor = World.Commander.Network.LaneTraffic.GetGroupColor(built.AllowedTraffic);
                paths.Add((path, built));
            }

            Junction.PutStructures(World.DXHost.Device, World.PhysicsHost, Structures);

            IErrorCollector componentErrorCollector = IErrorCollector.Default();
            componentErrorCollector.Reported += (sender, e) =>
            {
                ScriptError error = ScriptError.CreateFrom(e.Error);
                World.ErrorCollector.Report(error);
            };
            foreach ((JunctionPathTemplate template, ILanePath built) in paths)
            {
                template.BuildComponents(built, componentErrorCollector);
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
