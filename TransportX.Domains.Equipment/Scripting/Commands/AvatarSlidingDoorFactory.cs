using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars.Commands;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class AvatarSlidingDoorFactory : SlidingDoorFactoryBase<AvatarSlidingDoorFactory>
    {
        private readonly AvatarDoors Parent;

        public AvatarSlidingDoorFactory(AvatarDoors parent, string key) : base(parent, key)
        {
            Parent = parent;
        }

        public AvatarSlidingDoorFactory Panel(string partKey, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part) ? Panel(part, rotationX, rotationY, rotationZ, width) : this;

        public AvatarSlidingDoorFactory Panel(Part part, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part) ? Panel(part.Model, rotationX, rotationY, rotationZ, width) : this;

        public AvatarSlidingDoorFactory Panel(string partKey, double width) => Panel(partKey, 0, 0, 0, width);
        public AvatarSlidingDoorFactory Panel(Part part, double width) => Panel(part, 0, 0, 0, width);
    }
}
