using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

namespace Bus.Common.Physics
{
    public static class RigidPoseExtensions
    {
        public static Matrix4x4 ToMatrix4x4(this RigidPose pose)
        {
            Matrix4x4 result = Matrix4x4.CreateFromQuaternion(pose.Orientation);
            result.Translation = pose.Position;
            return result;
        }

        public static RigidPose ToRigidPose(this Matrix4x4 transform)
        {
            if (!Matrix4x4.Decompose(transform, out _, out Quaternion orientation, out Vector3 position))
            {
                throw new ArgumentException("指定された行列は剛体変換ではありません。", nameof(transform));
            }

            return new RigidPose(position, orientation);
        }
    }
}
