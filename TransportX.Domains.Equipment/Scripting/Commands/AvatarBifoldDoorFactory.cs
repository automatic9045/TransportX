using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars.Commands;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class AvatarBifoldDoorFactory : BifoldDoorFactoryBase<AvatarBifoldDoorFactory>
    {
        private readonly AvatarDoors Parent;

        public AvatarBifoldDoorFactory(AvatarDoors parent, string key) : base(parent, key)
        {
            Parent = parent;
        }

        public AvatarBifoldDoorFactory HingedPanel(string partKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part) ? HingedPanel(part, x, y, z, rotationX, rotationY, rotationZ, width) : this;
        public AvatarBifoldDoorFactory GuidePanel(string partKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part part) ? GuidePanel(part, x, y, z, rotationX, rotationY, rotationZ, width) : this;

        public AvatarBifoldDoorFactory HingedPanel(Part part, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part) ? HingedPanel(part.Model, x, y, z, rotationX, rotationY, rotationZ, width) : this;
        public AvatarBifoldDoorFactory GuidePanel(Part part, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double width)
            => Parent.Avatar.Commander.Structure.Parts.CheckContains(part) ? GuidePanel(part.Model, x, y, z, rotationX, rotationY, rotationZ, width) : this;

        public AvatarBifoldDoorFactory HingedPanel(string partKey, double x, double y, double z, double width)
            => HingedPanel(partKey, x, y, z, 0, 0, 0, width);
        public AvatarBifoldDoorFactory HingedPanel(string partKey, double width)
            => HingedPanel(partKey, 0, 0, 0, width);
        public AvatarBifoldDoorFactory GuidePanel(string partKey, double x, double y, double z, double width)
            => GuidePanel(partKey, x, y, z, 0, 0, 0, width);
        public AvatarBifoldDoorFactory GuidePanel(string partKey, double width)
            => GuidePanel(partKey, 0, 0, 0, width);

        public AvatarBifoldDoorFactory HingedPanel(Part part, double x, double y, double z, double width)
            => HingedPanel(part, x, y, z, 0, 0, 0, width);
        public AvatarBifoldDoorFactory HingedPanel(Part part, double width)
            => HingedPanel(part, 0, 0, 0, width);
        public AvatarBifoldDoorFactory GuidePanel(Part part, double x, double y, double z, double width)
            => GuidePanel(part, x, y, z, 0, 0, 0, width);
        public AvatarBifoldDoorFactory GuidePanel(Part part, double width)
            => GuidePanel(part, 0, 0, 0, width);
    }
}
