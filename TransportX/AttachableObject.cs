using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX
{
    public sealed class AttachableObject : IMovable
    {
        private ChunkIndex LastChunkIndex;

        public IWorldObject Parent { get; }
        public Pose Offset { get; }

        public WorldPose WorldPose => Offset * Parent.WorldPose;
        public Vector3 Velocity => Parent is IMovable movable ? movable.Velocity : Vector3.Zero;
        public Vector3 AngularVelocity => Parent is IMovable movable ? movable.AngularVelocity : Vector3.Zero;

        public event MovedEventHandler? Moved;

        public AttachableObject(IWorldObject parent, Pose offset)
        {
            Parent = parent;
            Offset = offset;

            LastChunkIndex = WorldPose.Chunk;
            Parent.Moved += _ =>
            {
                ChunkIndex offset = WorldPose.Chunk - LastChunkIndex;
                LastChunkIndex = WorldPose.Chunk;
                Moved?.Invoke(offset);
            };
        }

        public AttachableObject(IWorldObject parent, SixDoF position) : this(parent, position.ToPose())
        {
        }
    }
}
