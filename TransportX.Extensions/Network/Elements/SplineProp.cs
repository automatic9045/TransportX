using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Mathematics;
using TransportX.Physics;
using TransportX.Spatial;

namespace TransportX.Extensions.Network.Elements
{
    public class SplineProp
    {
        public IReadOnlyList<TransformedModelTemplate> Models { get; }
        public float From { get; }
        public float Span { get; }
        public float Interval { get; }
        public int Count { get; }

        public string? Key { get; init; } = null;

        public SplineProp(IReadOnlyList<TransformedModelTemplate> models, float from, float span, float interval, int count)
        {
            Models = models;
            From = from;
            Span = span;
            Interval = float.Max(0.1f, interval);
            Count = count;
        }

        public IEnumerable<TransformedModelTemplate> Build(IPhysicsHost physicsHost, IPoseCurve curve)
        {
            if (Models.Count == 0) yield break;

            for (int i = 0; i < Count; i++)
            {
                float s = From + Interval * i;
                if (curve.Length < s) break;

                TransformedModelTemplate template = Models[i % Models.Count];
                Pose curvePose = GetSpanPose(curve, s, Span);
                Pose localPose = template.Pose * curvePose;

                if (template is StaticTransformedModelTemplate staticTemplate)
                {
                    StaticTransformedModelTemplate compiled = new(physicsHost, staticTemplate.Resource, localPose);
                    if (!staticTemplate.CanMerge) compiled.ProhibitMerge();
                    yield return compiled;
                }
                else
                {
                    yield return new TransformedModelTemplate(template.Resource, localPose);
                }
            }
        }

        private static Pose GetSpanPose(IPoseCurve curve, float s, float span)
        {
            Pose back = curve.GetPose(s);
            Pose front = curve.GetPose(s + span);
            Vector3 forward = front.Position - back.Position;
            if (forward.LengthSquared() < 1e-6f) return back;

            Vector3 up = Vector3.Normalize(Vector3.Lerp(front.Up, back.Up, 0.5f));
            Vector3 tangent = Vector3.Normalize(forward);
            return Pose.CreateWorldLH(back.Position, tangent, up);
        }
    }
}
