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

        private readonly DriverViewpoint Driver;
        private readonly List<ViewpointPreset> DriverPresets = [new ViewpointPreset(new Pose(0, 1.5f, 0), float.Pi / 4)];

        internal Viewpoints(ScriptAvatar avatar)
        {
            Avatar = avatar;
            Driver = new DriverViewpoint(Avatar, DriverPresets);
        }

        public void AddDriver(double x, double y, double z, double rotationX, double rotationY, double rotationZ, double fieldOfView = 45)
        {
            if (Avatar.DriverViewpoint != Driver)
            {
                DriverPresets.Clear();
                Avatar.DriverViewpoint = Driver;
            }

            SixDoF offset = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            ViewpointPreset preset = new(offset.ToPose(), (float)fieldOfView * float.Pi / 180);
            DriverPresets.Add(preset);

            if (DriverPresets.Count == 1) Driver.SetPreset(0, true);
        }

        public void AddDriver(double x, double y, double z, double fieldOfView = 45)
            => AddDriver(x, y, z, 0, 0, 0, fieldOfView);

        public void SetBird(double x, double y, double z, double initialDistance, double angleX, double angleY, double fieldOfView = 45)
        {
            Pose offset = new((float)x, (float)y, (float)z);
            Vector2 initialAngle = new Vector2((float)angleX, (float)angleY) * float.Pi / 180;
            Avatar.BirdViewpoint = new BirdViewpoint(Avatar, offset, (float)initialDistance, initialAngle, (float)fieldOfView * float.Pi / 180);
        }
    }
}
