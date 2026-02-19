using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Environment
{
    public class Bloom
    {
        public static readonly Bloom Default = new()
        {
            Threshold = 2,
            Intensity = 0.2f,
            Scatter = 1,
            SoftKnee = 0.5f,
            Tint = Vector3.One,
        };


        public required float Threshold { get; init; }
        public required float Intensity { get; init; }
        public required float Scatter { get; init; }
        public required float SoftKnee { get; init; }
        public required Vector3 Tint { get; init; }

        public Bloom()
        {
        }
    }
}
