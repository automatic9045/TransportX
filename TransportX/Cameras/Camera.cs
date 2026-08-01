using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;
using Vortice.XAudio2;

using TransportX.Spatial;

namespace TransportX.Cameras
{
    public class Camera : WorldObject, ICamera
    {
        private Matrix4x4 View = Matrix4x4.Identity;

        public float Perspective { get; set; } = MathHelper.ToRadians(45);
        public ICamera.VisualLayers VisibleLayers { get; set; } = ICamera.VisualLayers.Normal;

        public Camera() : base()
        {
        }

        public void UpdateView(in WorldPose worldPose)
        {
            Locate(worldPose);
            View = Pose.Inverse(WorldPose.Pose).ToMatrix4x4();
        }

        public ViewContext CreateViewContext(SizeI clientSize)
        {
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
                Perspective * MathHelper.ToRadians(45), (float)clientSize.Width / clientSize.Height, 0.1f, 1000);

            return new ViewContext()
            {
                View = View,
                Projection = projection,
                WorldPose = WorldPose,
            };
        }
    }
}
