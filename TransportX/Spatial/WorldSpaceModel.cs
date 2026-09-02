using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Spatial
{
    public class WorldSpaceModel : IWorldObject
    {
        private ChunkIndex LastChunkIndex;

        public IWorldObject Parent { get; }
        public TransformedModel Model { get; }

        public WorldPose WorldPose => new(Parent.WorldPose.Chunk, Model.Pose);

        public event MovedEventHandler? Moved;

        public WorldSpaceModel(IWorldObject parent, TransformedModel model)
        {
            Parent = parent;
            Model = model;

            LastChunkIndex = WorldPose.Chunk;
            Parent.Moved += _ =>
            {
                ChunkIndex offset = WorldPose.Chunk - LastChunkIndex;
                LastChunkIndex = WorldPose.Chunk;
                Moved?.Invoke(offset);
            };
        }
    }
}
