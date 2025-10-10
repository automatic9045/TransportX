using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles.Drives
{
    internal interface ITransmission
    {
        string PositionText { get; }
        Vector2 LeverCoord { get; }
        double Torque { get; }
        int Gear { get; }
    }
}
