using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Commands
{
    public class Plates
    {
        private readonly ScriptWorld World;

        public PlateCommand this[int x, int z] => new PlateCommand(World, x, z);

        internal Plates(ScriptWorld world)
        {
            World = world;
        }
    }
}
