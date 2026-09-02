using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Spatial
{
    public class VelocityTrackedWorldSpaceModel : WorldSpaceModel, IMovable
    {
        private WorldPose OldWorldPose = WorldPose.Zero;

        public Vector3 Velocity { get; private set; } = Vector3.Zero;
        public Vector3 AngularVelocity { get; private set; } = Vector3.Zero;

        public VelocityTrackedWorldSpaceModel(IWorldObject parent, TransformedModel model) : base(parent, model)
        {
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
    }
}
