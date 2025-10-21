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
            return Matrix4x4.CreateFromQuaternion(pose.Orientation) * Matrix4x4.CreateTranslation(pose.Position);
        }

        public static RigidPose ToRigidPose(this Matrix4x4 transform)
        {
            Matrix4x4.Decompose(transform, out _, out Quaternion orientation, out Vector3 position);
            return new RigidPose(position, orientation);
        }
    }
}
