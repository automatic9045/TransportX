using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles
{
    internal static class Spec
    {
        public const double Width = 2.485;
        public const double FrontOverhang = 2.3;
        public const double RearOverhang = 2.83;
        public const double Wheelbase = 5.3;
        public const double Length = FrontOverhang + Wheelbase + RearOverhang;

        public const double TurningMinRadius = 8.3;
        public static readonly double MinRadius = double.Sqrt(TurningMinRadius * TurningMinRadius - Wheelbase * Wheelbase) - Width / 2;

        public const double Weight = 9400;
        public const double EngineInertiaRatio = 0.0025;
    }
}
