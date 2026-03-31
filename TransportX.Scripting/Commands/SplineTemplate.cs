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
    public class SplineTemplate
    {
        private readonly ScriptWorld World;

        public LaneLayout OutletLayout { get; }

        private readonly List<SplineStructure> StructuresKey = [];
        public IReadOnlyList<SplineStructure> Structures => StructuresKey;

        public IComponentCollection<ITemplateComponent<IReadOnlyList<SplineBase>>> Components { get; }
            = new ComponentCollection<ITemplateComponent<IReadOnlyList<SplineBase>>>();
        public IErrorCollector ErrorCollector => World.ErrorCollector;

        internal SplineTemplate(ScriptWorld world, string outletLayoutKey)
        {
            World = world;
            OutletLayout = World.Commander.Network.LaneLayouts[outletLayoutKey];
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, Pose pose, double from, double span, double interval)
        {
            LocatedModelTemplate[] models = modelKeys.Select(key =>
            {
                IModel? model;
                if (key == string.Empty)
                {
                    model = Model.Empty();
                }
                else if (!World.Models.TryGetValue(key, out model))
                {
                    ScriptError error = new(ErrorLevel.Error, $"モデル '{key}' が見つかりません。");
                    World.ErrorCollector.Report(error);

                    model = Model.Empty();
                }

                return new LocatedModelTemplate(model, pose);
            }).ToArray();
            SplineStructure structure = new(models, (float)from, (float)span, (float)interval, int.MaxValue);
            StructuresKey.Add(structure);
            return structure;
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double from, double span, double interval)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return PutStructure(modelKeys, position.ToPose(), from, span, interval);
        }

        public SplineStructure PutStructure(IReadOnlyList<string> modelKeys, double x, double y, double z, double from, double span, double interval)
        {
            return PutStructure(modelKeys, x, y, z, 0, 0, 0, from, span, interval);
        }

        internal SplineFactory Build(int plateX, int plateZ, Pose pose, NetworkPort? sourcePort)
        {
            SplineFactory factory = new(plateX, plateZ, pose, OutletLayout, sourcePort);
            factory.PutStructures(Structures);
            return factory;
        }

        internal void CopyComponentsTo(SplineFactoryCommand factoryCommand)
        {
            foreach ((Type type, ITemplateComponent<IReadOnlyList<SplineBase>> component) in Components)
            {
                factoryCommand.Components.Add(type, component);
            }
        }
    }
}
