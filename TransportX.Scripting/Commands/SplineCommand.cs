using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Spatial;

using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Commands
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

        public JunctionFactoryCommand IntoJunction(string templateKey, string targetPortKey)
        {
            JunctionTemplate? template = World.Commander.Network.Templates.GetJunction(templateKey);

            PortDefinition? targetPort = null;
            template?.Ports.TryGetValue(targetPortKey, out targetPort);

            Junction junction;
            JunctionFactoryCommand factoryCommand;
            if (template is null || targetPort is null)
            {
                junction = new Junction(Splines[^1].PlateX, Splines[^1].PlateZ, Outlet.Offset * Splines[^1].Pose, []);
                factoryCommand = new JunctionFactoryCommand(World, junction);
            }
            else
            {
                factoryCommand = null!;
                junction = ((Spline)Splines[^1]).ConnectNew(targetPort, (plateX, plateZ, pose) =>
                {
                    factoryCommand = template.Build(plateX, plateZ, pose);
                    return factoryCommand.Junction;
                });
            }
            junction.DebugName = World.Commander.Plates[junction.PlateX, junction.PlateZ].CreateJunctionDebugName(templateKey);

            Plate plate = World.Plates.GetOrAdd(junction.PlateX, junction.PlateZ);
            plate.Network.Add(junction);
            return factoryCommand;
        }
    }
}
