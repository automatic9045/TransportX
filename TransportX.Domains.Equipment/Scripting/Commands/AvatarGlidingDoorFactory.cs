using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars.Commands;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class AvatarGlidingDoorFactory : GlidingDoorFactoryBase<AvatarGlidingDoorFactory>
    {
        private readonly AvatarDoors Parent;

        public AvatarGlidingDoorFactory(AvatarDoors parent, string key) : base(parent, key)
        {
            Parent = parent;
        }

        public AvatarGlidingDoorFactory Panel(Part part,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double openAngle, double slideDistance, double armLength)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part)
            ? Panel(part.Model, x, y, z, rotationX, rotationY, rotationZ, openAngle, slideDistance, armLength) : this;

        public AvatarGlidingDoorFactory Panel(Part part, double x, double y, double z, double openAngle, double slideDistance, double armLength)
            => Panel(part, x, y, z, 0, 0, 0, openAngle, slideDistance, armLength);

        public AvatarGlidingDoorFactory Panel(Part part, double openAngle, double slideDistance, double armLength)
            => Panel(part, 0, 0, 0, openAngle, slideDistance, armLength);

        public AvatarGlidingDoorFactory Panel(string partKey,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double openAngle, double slideDistance, double armLength)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part)
            ? Panel(part, x, y, z, rotationX, rotationY, rotationZ, openAngle, slideDistance, armLength) : this;

        public AvatarGlidingDoorFactory Panel(string partKey, double x, double y, double z, double openAngle, double slideDistance, double armLength)
            => Panel(partKey, x, y, z, 0, 0, 0, openAngle, slideDistance, armLength);

        public AvatarGlidingDoorFactory Panel(string partKey, double openAngle, double slideDistance, double armLength)
            => Panel(partKey, 0, 0, 0, openAngle, slideDistance, armLength);
    }
}
