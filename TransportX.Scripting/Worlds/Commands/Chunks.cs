using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Spatial;

namespace TransportX.Scripting.Worlds.Commands
{
    public class Chunks
    {
        private readonly ScriptWorld World;

        public ChunkCommand this[ChunkIndex index] => new(World, index);
        public ChunkCommand this[int x, int z] => new(World, new ChunkIndex(x, z));

        internal Chunks(ScriptWorld world)
        {
            World = world;
        }

        public ChunkCommand For(IWorldObject worldObject)
        {
            return this[worldObject.WorldPose.Chunk];
        }
    }
}
