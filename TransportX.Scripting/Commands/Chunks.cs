using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Commands
{
    public class Chunks
    {
        private readonly ScriptWorld World;

        public ChunkCommand this[int x, int z] => new(World, x, z);

        internal Chunks(ScriptWorld world)
        {
            World = world;
        }

        public ChunkCommand For(ILocatable locatable)
        {
            return this[locatable.WorldPose.ChunkX, locatable.WorldPose.ChunkZ];
        }
    }
}
