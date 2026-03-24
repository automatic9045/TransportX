using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.Vehicles.Interfaces;
using TransportX.Sample.Vehicles.Powertrain.Constraints;
using TransportX.Sample.Vehicles.Powertrain.Physics;

namespace TransportX.Sample.Vehicles.Powertrain.Modules
{
    internal class MT : TransmissionBase
    {
        private readonly MTShifter Shifter;

        protected override IReadOnlyList<float> GearRatios { get; } = [6.615f, 4.095f, 2.358f, 1.531f, 1, 0.722f];
        protected override float ReverseGearRatio { get; } = -6.615f;

        public override int Gear => Shifter.Gear;

        public MT(MTShifter shifter, Shaft input, Shaft output) : base(input, output)
        {
            Shifter = shifter;
        }
    }
}
