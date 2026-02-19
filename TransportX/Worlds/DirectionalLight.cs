using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Worlds
{
    public readonly struct DirectionalLight
    {
        public static readonly DirectionalLight Default = new()
        {
            Color = Vector3.One,
            Direction = -Vector3.UnitY,
            Intensity = 1,
        };


        public required readonly Vector3 Color { get; init; }
        public required readonly Vector3 Direction { get; init; }
        public required readonly float Intensity { get; init; }

        public DirectionalLight()
        {
        }
    }
}
