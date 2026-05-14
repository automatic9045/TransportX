using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TransportX.Spatial;

namespace TransportX.Cameras
{
    public readonly record struct CameraPose(int ChunkX, int ChunkZ, Vector3 Position, Vector2 Angle)
    {
        public static CameraPose FromWorldPose(WorldPose worldPose)
        {
            Vector3 direction = worldPose.Pose.Direction;

            float yaw = float.Atan2(direction.X, direction.Z);
            float xzLength = float.Sqrt(direction.X * direction.X + direction.Z * direction.Z);
            float pitch = float.Atan2(-direction.Y, xzLength);

            Vector2 angle = new(pitch, yaw);
            return new CameraPose(worldPose.ChunkX, worldPose.ChunkZ, worldPose.Pose.Position, angle);
        }
    }
}
