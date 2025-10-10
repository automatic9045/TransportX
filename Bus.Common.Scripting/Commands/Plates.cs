using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scripting.Commands
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
