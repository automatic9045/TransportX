using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Extensions.Networks
{
    public readonly struct SplineSegment
    {
        public required readonly float FromS { get; init; }
        public required readonly float ToS { get; init; }
        public required readonly float Length { get; init; }

        public required readonly Vector3 Position { get; init; }
        public required readonly Quaternion Orientation { get; init; }

        public required readonly float Curvature { get; init; }
        public required readonly float GradientDelta { get; init; }
        public required readonly float Cant { get; init; }
        public required readonly float CantDelta { get; init; }

        public readonly (Vector3 Translation, Quaternion Rotation) GetRelativeTransform(float ds)
        {
            float gradientRate = 1e-6f < Length ? GradientDelta / Length : 0;
            float gradient = gradientRate * ds;

            float vSinIntegral;
            float vCosIntegral;
            if (float.Abs(gradientRate) < 1e-6f)
            {
                float averageGradient = gradientRate * ds / 2;
                vSinIntegral = ds * float.Sin(averageGradient);
                vCosIntegral = ds * float.Cos(averageGradient);
            }
            else
            {
                vSinIntegral = (1 - float.Cos(gradient)) / gradientRate;
                vCosIntegral = (float.Sin(gradient)) / gradientRate;
            }

            float hAngle = Curvature * ds;
            float x, z;
            if (float.Abs(Curvature) < 1e-6f)
            {
                x = 0;
                z = ds;
            }
            else
            {
                x = (1 - float.Cos(hAngle)) / Curvature;
                z = float.Sin(hAngle) / Curvature;
            }

            float horizontalRate = 1e-6f < ds ? vCosIntegral / ds : 1;
            Vector3 position = new(x * horizontalRate, vSinIntegral, z * horizontalRate);

            Quaternion unbank = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Cant);
            position = Vector3.Transform(position, unbank);

            Quaternion turn = Quaternion.CreateFromYawPitchRoll(hAngle, -gradient, 0);

            float cantRate = 1e-6f < Length ? CantDelta / Length : 0;
            float cant = Cant + cantRate * ds;
            Quaternion rebank = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -cant);

            Quaternion rotation = unbank * turn * rebank;

            return (position, rotation);
        }
    }
}
