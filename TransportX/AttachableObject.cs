using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX
{
    public sealed class AttachableObject : IWorldObject
    {
        private ChunkIndex LastChunkIndex;

        public IWorldObject Parent { get; }
        public Pose Offset { get; }

        public WorldPose WorldPose => Offset * Parent.WorldPose;
        public Vector3 Velocity => Parent.Velocity;

        public event MovedEventHandler? Moved;

        public AttachableObject(IWorldObject parent, Pose offset, bool notifyWhenMoved = true)
        {
            Parent = parent;
            Offset = offset;

            if (notifyWhenMoved)
            {
                LastChunkIndex = WorldPose.Chunk;
                Parent.Moved += _ =>
                {
                    ChunkIndex offset = WorldPose.Chunk - LastChunkIndex;
                    LastChunkIndex = WorldPose.Chunk;
                    Moved?.Invoke(offset);
                };
            }
        }

        public AttachableObject(IWorldObject parent, SixDoF position, bool notifyWhenMoved = true) : this(parent, position.ToPose(), notifyWhenMoved)
        {
        }

        public Vector3 GetOffset(IWorldObject to) => ((IWorldObject)this).GetOffset(to);
    }
}
