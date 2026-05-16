using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Spatial;

using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Worlds.Commands
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
            ChunkCommand chunk = World.Commander.Chunks.For(Outlet.Owner);
            SplineFactoryCommand spline = chunk.BeginSpline(templateKey, Outlet.Offset * Splines[^1].WorldPose.Pose, Outlet);
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
                junction = new Junction(Outlet.Offset * Splines[^1].WorldPose, []);
                factoryCommand = new JunctionFactoryCommand(World, junction);
            }
            else
            {
                factoryCommand = null!;
                junction = ((Spline)Splines[^1]).ConnectNew(targetPort, (worldPose) =>
                {
                    factoryCommand = template.Build(worldPose);
                    return factoryCommand.Junction;
                });
            }
            junction.DebugName = World.Commander.Chunks.For(junction).CreateJunctionDebugName(templateKey);

            Chunk chunk = World.Chunks.GetOrAddFor(junction);
            chunk.Network.Add(junction);
            return factoryCommand;
        }
    }
}
