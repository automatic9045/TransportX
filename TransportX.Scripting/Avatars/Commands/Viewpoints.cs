using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Cameras;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Viewpoints
    {
        private readonly ScriptAvatar Avatar;

        internal Viewpoints(ScriptAvatar avatar)
        {
            Avatar = avatar;
        }

        public void SetDriver(double x, double y, double z)
        {
            Avatar.DriverViewpoint = new DriverViewpoint(Avatar, new Pose((float)x, (float)y, (float)z));
        }

        public void SetBird(double x, double y, double z, double initialDistance, double angleX, double angleY)
        {
            Pose offset = new((float)x, (float)y, (float)z);
            Vector2 initialAngle = new Vector2((float)angleX, (float)angleY) * float.Pi / 180;
            Avatar.BirdViewpoint = new BirdViewpoint(Avatar, offset, (float)initialDistance, initialAngle);
        }
    }
}
