using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Avatars.Commands;
using TransportX.Spatial;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class AvatarSceneCaptureCameraFactory : SceneCaptureCameraFactoryBase<AvatarSceneCaptureCameraFactory>
    {
        private readonly AvatarCameras Parent;

        public AvatarSceneCaptureCameraFactory(AvatarCameras parent, string key) : base(parent, key)
        {
            Parent = parent;
        }

        public AvatarSceneCaptureCameraFactory Position(IWorldObject parent, Pose offset)
        {
            AttachableObject attachable = new(parent, offset);
            return Position(attachable);
        }

        public AvatarSceneCaptureCameraFactory Position(Part parent, Pose offset)
        {
            WorldSpaceModel parentObject = new(Parent.Avatar, parent.Model);
            return Position(parentObject, offset);
        }

        public AvatarSceneCaptureCameraFactory Position(Part parent, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF offset = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return Position(parent, offset.ToPose());
        }
        public AvatarSceneCaptureCameraFactory Position(Part parent, double x, double y, double z)
            => Position(parent, x, y, z, 0, 0, 0);

        public AvatarSceneCaptureCameraFactory Position(string parentPartKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
            => Parent.Avatar.Commander.Structure.Parts.All.GetValue(parentPartKey, out Part part) ? Position(part, x, y, z, rotationX, rotationY, rotationZ) : this;
        public AvatarSceneCaptureCameraFactory Position(string parentPartKey, double x, double y, double z)
            => Position(parentPartKey, x, y, z, 0, 0, 0);

        public AvatarSceneCaptureCameraFactory ProjectOntoPart(Part part, string targetMaterialName)
            => ProjectOnto(part.Model, targetMaterialName);

        public AvatarSceneCaptureCameraFactory ProjectOntoPart(string partKey, string targetMaterialName)
        {
            if (!Parent.Avatar.Commander.Structure.Parts.All.GetValue(partKey, out Part? part)) return this;
            return ProjectOntoPart(part, targetMaterialName);
        }
    }
}
