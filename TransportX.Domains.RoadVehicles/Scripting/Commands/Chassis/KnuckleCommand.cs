using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars;
using TransportX.Scripting.Avatars.Commands;

using TransportX.Domains.RoadVehicles.Chassis;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Chassis
{
    public class KnuckleCommand
    {
        public Knuckle Source { get; }

        public DynamicPart Beam { get; }
        public DynamicPart Wheel { get; }
        public Joint Hinge { get; }

        internal KnuckleCommand(ScriptAvatar avatar, Knuckle source, DynamicPart beam, DynamicPart wheel, string jointKey)
        {
            Source = source;

            Beam = beam;
            Wheel = wheel;
            Hinge = avatar.Commander.Structure.Joints.Add(jointKey, beam, wheel, source.Hinge);
        }
    }
}
