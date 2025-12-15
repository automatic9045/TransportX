using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles
{
    internal static class Spec
    {
        public const float Width = 2.485f;
        public const float FrontOverhang = 2.3f;
        public const float RearOverhang = 2.83f;
        public const float Wheelbase = 5.3f;
        public const float Length = FrontOverhang + Wheelbase + RearOverhang;

        public const float TurningMinRadius = 8.3f;
        public static readonly float MinRadius = float.Sqrt(TurningMinRadius * TurningMinRadius - Wheelbase * Wheelbase) - Width / 2;

        public const float Weight = 9400;
        public const float EngineInertiaRatio = 0.0025f;

        public const float MaxSteeringWheelAngle = 2.5f * float.Pi;
        public const float InnerSteeringAngle = 53f / 180 * float.Pi;
        public const float OuterSteeringAngle = 38.5f / 180 * float.Pi;
    }
}
