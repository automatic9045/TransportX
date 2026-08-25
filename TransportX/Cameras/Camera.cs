using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

using TransportX.Spatial;

namespace TransportX.Cameras
{
    public class Camera : WorldObject, ICamera
    {
        private Matrix4x4 View = Matrix4x4.Identity;
        private WorldPose OldWorldPose = WorldPose.Zero;

        public Vector3 Velocity { get; private set; } = Vector3.Zero;
        public Vector3 AngularVelocity { get; private set; } = Vector3.Zero;

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

        public void Tick(TimeSpan elapsed)
        {
            float dt = (float)elapsed.TotalSeconds;
            if (0 < dt)
            {
                Velocity = OldWorldPose.GetOffset(WorldPose) / dt;

                Quaternion qDelta = Quaternion.Inverse(OldWorldPose.Pose.Orientation) * WorldPose.Pose.Orientation;
                if (qDelta.W < 0)
                {
                    qDelta = new Quaternion(-qDelta.X, -qDelta.Y, -qDelta.Z, -qDelta.W);
                }

                float angle = 2 * float.Acos(float.Clamp(qDelta.W, -1, 1));
                float sinHalfAngle = float.Sqrt(float.Max(0, 1 - qDelta.W * qDelta.W));

                Vector3 axis = Vector3.Zero;
                if (1e-6f < sinHalfAngle)
                {
                    axis = new Vector3(qDelta.X, qDelta.Y, qDelta.Z) / sinHalfAngle;
                }

                AngularVelocity = axis * (angle / dt);
            }
            else
            {
                Velocity = Vector3.Zero;
                AngularVelocity = Vector3.Zero;
            }

            OldWorldPose = WorldPose;
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
