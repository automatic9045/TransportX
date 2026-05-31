using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TransportX.Spatial;

namespace TransportX.Cameras
{
    public readonly record struct CameraPose
    {
        public ChunkIndex Chunk { get; }
        public Vector3 Position { get; }
        public Vector2 Angle { get; }

        public CameraPose(ChunkIndex chunk, Vector3 position, Vector2 angle)
        {
            Chunk = chunk;
            Position = position;
            Angle = angle;
        }

        public static CameraPose FromWorldPose(WorldPose worldPose)
        {
            Vector3 direction = worldPose.Pose.Direction;

            float yaw = float.Atan2(direction.X, direction.Z);
            float xzLength = float.Sqrt(direction.X * direction.X + direction.Z * direction.Z);
            float pitch = float.Atan2(-direction.Y, xzLength);

            Vector2 angle = new(pitch, yaw);
            return new CameraPose(worldPose.Chunk, worldPose.Pose.Position, angle);
        }
    }
}
