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
        public static Pose ToPose(this RigidPose rigidPose)
        {
            return new Pose(rigidPose.Position, rigidPose.Orientation);
        }

        public static RigidPose ToRigidPose(this Pose pose)
        {
            return new RigidPose(pose.Position, pose.Orientation);
        }
    }
}
