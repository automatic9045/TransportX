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

        public Listener Listener { get; } = new Listener();
        public ViewpointSet Viewpoints { get; }

        public ICamera.VisualLayers VisibleLayers { get; set; } = ICamera.VisualLayers.Normal;

        public Camera() : base()
        {
            Viewpoints = new ViewpointSet();
        }

        public void UpdateView()
        {
            Locate(Viewpoints.Current.WorldPose);

            Listener.OrientFront = WorldPose.Pose.Direction;
            Listener.OrientTop = WorldPose.Pose.Up;
            Listener.Position = WorldPose.Pose.Position;
            Listener.Velocity = Velocity;

            View = Pose.Inverse(WorldPose.Pose).ToMatrix4x4();
        }

        public ViewContext CreateViewContext(SizeI clientSize)
        {
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
                Viewpoints.Current.Perspective * MathHelper.ToRadians(45), (float)clientSize.Width / clientSize.Height, 0.1f, 1000);
            BoundingFrustum frustum = new(View * projection);

            return new ViewContext()
            {
                View = View,
                Projection = projection,
                Frustum = frustum,
                WorldPose = WorldPose,
            };
        }
    }
}
