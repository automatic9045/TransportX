using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Mathematics;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Rendering.Procedural;
using TransportX.Spatial;

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

        public TransformedModel? Build(ID3D11Device device, IPhysicsHost physicsHost, IPoseCurve curve, Pose pose, float sOffset)
        {
            if (curve.Length < 1e-3f) return null;

            IReadOnlyList<float> sSamples = Sampler.Sample(curve, sOffset);
            if (sSamples.Count < 2) return null;

            List<(Pose Pose, float S)> samples = new(sSamples.Count);
            for (int i = 0; i < sSamples.Count; i++)
            {
                float localS = float.Clamp(sSamples[i], 0, curve.Length);
                samples.Add((curve.GetPose(localS), localS + sOffset));
            }

            ModelResourceSet resource = CrossSection.Build(device, physicsHost.Simulation, samples, Offset, VIntervalScale, VOffset);
            return StaticTransformedModel.CreateStaticOrNonCollision(physicsHost, resource, pose);
        }
    }
}
