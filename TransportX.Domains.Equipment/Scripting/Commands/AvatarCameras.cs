using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

using TransportX.Scripting.Avatars;

namespace TransportX.Domains.Equipment.Scripting.Commands
{
    public class AvatarCameras : CamerasBase, IAvatarInstantiable<AvatarCameras>
    {
        internal List<VelocityTrackedWorldSpaceModel> TrackedModels { get; } = [];
        internal ScriptAvatar Avatar { get; }

        public AvatarCameras(ScriptAvatar avatar) : base(avatar.GraphicsHost, avatar.ErrorCollector)
        {
            Avatar = avatar;
        }

        public static AvatarCameras Create(ScriptAvatar avatar) => new(avatar);

        public AvatarSceneCaptureCameraFactory AddSceneCapture(string key)
        {
            AvatarSceneCaptureCameraFactory factory = new(this, key);
            return factory;
        }
    }
}
