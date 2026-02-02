using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

using Bus.Common.Extensions.Networks.Elements;

namespace Bus.Common.Scripting.Commands
{
    public class SplineCommand
    {
        private readonly ScriptWorld World;

        public IReadOnlyList<SplineBase> Splines { get; }

        public NetworkPort Inlet => Splines[0].Inlet;
        public NetworkPort Outlet => Splines[^1].Outlet;

        internal SplineCommand(ScriptWorld world, IReadOnlyList<SplineBase> splines)
        {
            World = world;
            Splines = splines;
        }

        public SplineFactoryCommand IntoSpline(string? templateKey)
        {
            PlateCommand plate = World.Commander.Plates[Outlet.Owner.PlateX, Outlet.Owner.PlateZ];
            SplineFactoryCommand spline = plate.BeginSpline(templateKey, Outlet.Offset * Splines[^1].Pose, Outlet);
            return spline;
        }

        public JunctionCommand IntoJunction(string templateKey, string targetPortKey)
        {
            JunctionTemplate? template = World.Commander.Network.Templates.GetJunction(templateKey);
            PortDefinition? targetPort = template?.GetPort(targetPortKey);

            Junction junction = template is null || targetPort is null
                ? new Junction(Splines[^1].PlateX, Splines[^1].PlateZ, Outlet.Offset * Splines[^1].Pose, [])
                : ((Spline)Splines[^1]).ConnectNew(targetPort, template.Build);
            junction.DebugName = World.Commander.Plates[junction.PlateX, junction.PlateZ].CreateJunctionDebugName(templateKey);

            AddElementToPlate(junction);
            return new JunctionCommand(World, junction);
        }

        private void AddElementToPlate(NetworkElement element)
        {
            Plate plate = World.Plates.GetOrAdd(element.PlateX, element.PlateZ);
            plate.Network.Add(element);
        }
    }
}
