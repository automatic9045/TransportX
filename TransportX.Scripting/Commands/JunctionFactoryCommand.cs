using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly List<LocatedModelTemplate> Structures = [];

        public Junction Junction { get; }

        public IComponentCollection<ITemplateComponent<Junction>> Components { get; } = new ComponentCollection<ITemplateComponent<Junction>>();

        public JunctionFactoryCommand(ScriptWorld world, Junction junction)
        {
            World = world;
            Junction = junction;
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

        public JunctionCommand Build()
        {
            Junction.PutStructures(World.DXHost.Device, World.PhysicsHost, Structures);

            IErrorCollector componentErrorCollector = IErrorCollector.Default();
            componentErrorCollector.Reported += (sender, e) =>
            {
                ScriptError error = ScriptError.CreateFrom(e.Error);
                World.ErrorCollector.Report(error);
            };
            foreach (ITemplateComponent<Junction> component in Components.Values)
            {
                component.Build(Junction, componentErrorCollector);
            }

            return new JunctionCommand(World, Junction);
        }
    }
}
