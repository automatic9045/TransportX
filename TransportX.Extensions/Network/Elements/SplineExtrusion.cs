using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering.Procedural;

using TransportX.Extensions.Mathematics;

namespace TransportX.Extensions.Network.Elements
{
    public class SplineExtrusion
    {
        public static readonly SplineExtrusion Empty = new(CrossSection.Empty, ICurveSampler.Empty, Vector2.Zero);


        public CrossSection CrossSection { get; }
        public ICurveSampler Sampler { get; }
        public Vector2 Offset { get; }

        public float VIntervalScale { get; init; } = 1;
        public float VOffset { get; init; } = 0;

        public string? Key { get; init; } = null;

        public SplineExtrusion(CrossSection crossSection, ICurveSampler sampler, Vector2 offset)
        {
            CrossSection = crossSection;
            Sampler = sampler;
            Offset = offset;
        }
    }
}
