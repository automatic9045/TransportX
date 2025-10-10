using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery
{
    public class LocatedPlate
    {
        public int X { get; }
        public int Z { get; }
        public Plate Plate { get; }

        public LocatedPlate(int x, int z, Plate plate)
        {
            X = x;
            Z = z;
            Plate = plate;
        }
    }
}
