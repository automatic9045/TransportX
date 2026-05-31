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
    public class Camera : WorldObject
    {
        public Listener Listener { get; } = new Listener();
        public ViewpointSet Viewpoints { get; }

        public VisualLayers VisibleLayers { get; set; } = VisualLayers.Normal;

        public Matrix4x4 View { get; protected set; } = default;
        public Matrix4x4 Projection { get; protected set; } = default;
        public BoundingFrustum Frustum { get; protected set; } = default;

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

        public void UpdateProjection(SizeI clientSize)
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
                Viewpoints.Current.Perspective * MathHelper.ToRadians(45), (float)clientSize.Width / clientSize.Height, 0.1f, 1000);
            Frustum = new(View * Projection);
        }


        [Flags]
        public enum VisualLayers
        {
            None = 0b0000,
            Normal = 0x0001,
            Colliders = 0b0010,
            Network = 0b0100,
            Traffic = 0b1000,
        }
    }
}
