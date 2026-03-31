using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Diagnostics;
using TransportX.Spatial;

using TransportX.Extensions.Network.Elements;

namespace TransportX.Scripting.Commands
{
    public class JunctionCommand
    {
        private readonly ScriptWorld World;

        public Junction Junction { get; }

        public string Name
        {
            get => Junction.DebugName ?? nameof(Extensions.Network.Elements.Junction);
            set => Junction.DebugName = value;
        }

        public JunctionCommand(ScriptWorld world, Junction junction)
        {
            World = world;
            Junction = junction;
        }

        internal NetworkPort? GetPort(string portKey)
        {
            if (!Junction.Ports.TryGetValue(portKey, out NetworkPort? port))
            {
                ScriptError error = new(ErrorLevel.Error, $"進路端子 '{portKey}' が見つかりません。");
                World.ErrorCollector.Report(error);
            }

            return port;
        }

        public SplineFactoryCommand IntoSpline(string portKey, string? templateKey)
        {
            NetworkPort? port = GetPort(portKey);
            Pose pose = port is null
                ? Pose.Identity
                : Junction.GetConnectionPose(port, Pose.CreateRotationY(float.Pi));

            PlateCommand plate = World.Commander.Plates[Junction.PlateX, Junction.PlateZ];
            SplineFactoryCommand spline = plate.BeginSpline(templateKey, pose, port);

            return spline;
        }

        public JunctionFactoryCommand IntoJunction(string portKey, string templateKey, string targetPortKey)
        {
            NetworkPort? port = GetPort(portKey);
            JunctionTemplate? template = World.Commander.Network.Templates.GetJunction(templateKey);
            PortDefinition? targetPort = template?.GetPort(targetPortKey);

            Junction junction;
            JunctionFactoryCommand factoryCommand;
            if (port is null || template is null || targetPort is null)
            {
                junction = new(Junction.PlateX, Junction.PlateZ, Junction.Pose, []);
                factoryCommand = new JunctionFactoryCommand(World, junction);
            }
            else
            {
                factoryCommand = null!;
                junction = Junction.ConnectNew(port, targetPort, (plateX, plateZ, pose) =>
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
