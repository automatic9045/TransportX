using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.Vehicles.Input
{
    internal interface IMTShifterInput
    {
        Vector2 Direction { get; }

        void Tick(TimeSpan elapsed);
    }
}
