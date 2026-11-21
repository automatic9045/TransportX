using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Network;
using TransportX.Physics;
using TransportX.Spatial;

namespace TransportX.Extensions.Network.Elements
{
    public class SplineFactory : WorldObject
    {
        private Func<NetworkPort?, SplineBase>? Finalizer = null;

        public LaneLayout OutletLayout { get; }
        public NetworkPort? SourcePort { get; }

        public CurveList Curves { get; } = new();
        public GradientList Gradients { get; } = new();
        public CantList Cants { get; } = new();

        public List<SplineProp> Props { get; } = [];
        public List<SplineExtrusion> Extrusions { get; } = [];

        public string? DebugName { get; set; } = null;

        public SplineFactory(WorldPose worldPose, LaneLayout outletLayout, NetworkPort? sourcePort) : base(worldPose)
        {
            OutletLayout = outletLayout;

            if (sourcePort is not null && sourcePort.Layout != OutletLayout) throw new ArgumentException("進路の接続部形状が一致しません。", nameof(sourcePort));
            SourcePort = sourcePort;
        }

        public void InterpolateByBezier(NetworkPort targetPort, float handleScale, out float s)
        {
            WorldPose endPose = WorldPose;
            foreach ((SplineSegment _, Pose localPose) in EnumerateSegments())
            {
                endPose = localPose * endPose;
            }

            NetworkElement targetElement = targetPort.Owner;
            ChunkIndex offset = targetElement.WorldPose.Chunk - endPose.Chunk;
            Pose to = targetPort.Offset * targetElement.WorldPose.Pose * offset.Pose;

            BezierSpline spline = new(endPose, to, OutletLayout, handleScale)
            {
                DebugName = DebugName,
            };

            s = Curves.S + spline.Length;

            Finalizer = fromPort =>
            {
                spline.DebugName = DebugName;
                fromPort?.ConnectTo(spline.Inlet);
                spline.Outlet.ConnectTo(targetPort);
                return spline;
            };
        }

        public List<SplineBase> Build(ID3D11Device device, IPhysicsHost physicsHost)
        {
            List<SplineBase> splines = [];
            NetworkPort? sourcePort = SourcePort;

            WorldPose splinePose = WorldPose;
            Pose splinePoseInv = Pose.Inverse(splinePose.Pose);

            List<SplineSegment> segments = [];
            foreach ((SplineSegment rawSegment, Pose localPose) in EnumerateSegments())
            {
                SplineSegment segment = new()
                {
                    FromS = rawSegment.FromS,
                    ToS = rawSegment.ToS,
                    Length = rawSegment.Length,

                    Pose = WorldPose.Pose * splinePoseInv,

                    Curvature = rawSegment.Curvature,
                    Gradient = rawSegment.Gradient,
                    GradientDelta = rawSegment.GradientDelta,
                    Cant = rawSegment.Cant,
                    CantDelta = rawSegment.CantDelta,
                };
                segments.Add(segment);

                ChunkIndex chunkOffset = Move(localPose);
                if (!chunkOffset.IsZero)
                {
                    AddSpline();

                    splinePose = WorldPose;
                    splinePoseInv = Pose.Inverse(splinePose.Pose);

                    segments = [];
                }
            }

            if (0 < segments.Count)
            {
                AddSpline();
            }

            if (Finalizer is not null)
            {
                SplineBase lastSpline = Finalizer(sourcePort);
                ApplyProps(lastSpline);
                ApplyExtrusions(lastSpline, Curves.S);
                splines.Add(lastSpline);
            }

            return splines;


            void AddSpline()
            {
                float offset = segments[0].FromS;
                SplineSegment[] normalizedSegments = segments.Select(segment => new SplineSegment()
                {
                    FromS = segment.FromS - offset,
                    ToS = segment.ToS - offset,
                    Length = segment.Length,

                    Pose = segment.Pose,

                    Curvature = segment.Curvature,
                    Gradient = segment.Gradient,
                    GradientDelta = segment.GradientDelta,
                    Cant = segment.Cant,
                    CantDelta = segment.CantDelta,
                }).ToArray();

                Spline spline = new(splinePose, OutletLayout, normalizedSegments)
                {
                    DebugName = DebugName,
                };
                sourcePort?.ConnectTo(spline.Inlet);
                sourcePort = spline.Outlet;
                ApplyProps(spline);
                ApplyExtrusions(spline, offset);
                splines.Add(spline);
            }

            void ApplyProps(SplineBase spline)
            {
                List<SplineProp> props = [];

                for (int propIndex = 0; propIndex < Props.Count; propIndex++)
                {
                    SplineProp prop = Props[propIndex];
                    if (prop.Count <= 0) continue;

                    if (spline.Length <= prop.From)
                    {
                        Props[propIndex] = new SplineProp(prop.Models, prop.From - spline.Length, prop.Span, prop.Interval, prop.Count);
                        continue;
                    }

                    int count = int.Clamp((int)float.Ceiling((spline.Length - prop.From) / prop.Interval), 0, prop.Count);
                    if (0 < count)
                    {
                        SplineProp splitProp = new(prop.Models, prop.From, prop.Span, prop.Interval, count);
                        props.Add(splitProp);
                    }

                    if (count == prop.Count)
                    {
                        Props[propIndex] = new SplineProp(prop.Models, 0, prop.Span, prop.Interval, 0);
                        continue;
                    }

                    TransformedModelTemplate[] nextModels = new TransformedModelTemplate[prop.Models.Count];
                    for (int i = 0; i < nextModels.Length; i++)
                    {
                        nextModels[i] = prop.Models[(i + count) % nextModels.Length];
                    }

                    float nextFrom = prop.From + prop.Interval * count - spline.Length;
                    int nextCount = prop.Count - count;

                    Props[propIndex] = new SplineProp(nextModels, nextFrom, prop.Span, prop.Interval, nextCount);
                }

                spline.PutProps(device, physicsHost, props);
            }

            void ApplyExtrusions(SplineBase spline, float sOffset)
            {
                for (int i = 0; i < Extrusions.Count; i++)
                {
                    SplineExtrusion extrusion = Extrusions[i];
                    spline.PutExtrusion(device, physicsHost, extrusion, sOffset);
                }
            }
        }

        private IEnumerable<(SplineSegment Segment, Pose LocalPose)> EnumerateSegments()
        {
            int curveIndex = 0;
            int gradientIndex = 0;
            int cantIndex = 0;

            float s = 0;
            while (curveIndex < Curves.Spans.Count)
            {
                Span curve = Curves.Spans[curveIndex];
                Span gradient = gradientIndex < Gradients.Spans.Count ? Gradients.Spans[gradientIndex] : new Span(0, 0, s, float.MaxValue);
                Span cant = cantIndex < Cants.Spans.Count ? Cants.Spans[cantIndex] : new Span(0, 0, s, float.MaxValue);

                float nextS = float.Min(float.Min(curve.ToS, gradient.ToS), cant.ToS);
                float length = nextS - s;

                Span slicedCurve = curve.Slice(s, length);
                Span slicedGradient = gradient.Slice(s, length);
                Span slicedCant = cant.Slice(s, length);

                SplineSegment segment = new()
                {
                    FromS = s,
                    ToS = nextS,
                    Length = length,

                    Pose = Pose.Identity,

                    Curvature = slicedCurve.FromValue,
                    Gradient = slicedGradient.FromValue,
                    GradientDelta = slicedGradient.ValueDelta,
                    Cant = slicedCant.FromValue,
                    CantDelta = slicedCant.ValueDelta,
                };

                Pose localPose = segment.GetLocalPose(length);
                yield return (segment, localPose);

                s = nextS;

                if (curveIndex < Curves.Spans.Count && Curves.Spans[curveIndex].ToS == nextS) curveIndex++;
                if (gradientIndex < Gradients.Spans.Count && Gradients.Spans[gradientIndex].ToS == nextS) gradientIndex++;
                if (cantIndex < Cants.Spans.Count && Cants.Spans[cantIndex].ToS == nextS) cantIndex++;
            }
        }


        public readonly struct Span
        {
            public readonly float FromValue { get; }
            public readonly float ValueDelta { get; }
            public readonly float ToValue => FromValue + ValueDelta;

            public readonly float FromS { get; }
            public readonly float Length { get; }
            public readonly float ToS => FromS + Length;

            public Span(float fromValue, float valueDelta, float fromS, float length)
            {
                FromValue = fromValue;
                ValueDelta = valueDelta;

                FromS = fromS;
                Length = length;
            }

            public readonly float GetValueAt(float s)
            {
                if (s < FromS || ToS < s) throw new ArgumentOutOfRangeException(nameof(s));
                return FromValue + ValueDelta * (s - FromS) / Length;
            }

            public readonly Span Slice(float startS, float length)
            {
                float fromValue = FromValue + ValueDelta * (startS - FromS) / Length;
                float valueDelta = ValueDelta * (length / Length);
                return new Span(fromValue, valueDelta, startS, length);
            }
        }

        public abstract class SpanList
        {
            private readonly List<Span> SpansKey = [];
            public IReadOnlyList<Span> Spans => SpansKey;

            public float Value { get; private set; } = 0;
            public float S { get; private set; } = 0;

            protected SpanList()
            {
            }

            protected void Add(float value, float valueDelta, float length)
            {
                Span span = new(value, valueDelta, S, length);
                SpansKey.Add(span);

                Value = span.ToValue;
                S = span.ToS;
            }
        }

        public class CurveList : SpanList
        {
            internal protected CurveList()
            {
            }

            public void ByCurvature(float curvature, float length) => Add(curvature, 0, length);
            public void Straight(float length) => ByCurvature(0, length);
            public void ByRadius(float radius, float length) => ByCurvature(1 / radius, length);
        }

        public class GradientList : SpanList
        {
            internal protected GradientList()
            {
            }

            public void Constant(float length) => Add(Value, 0, length);
            public void TransitionBy(float angle, float length) => Add(Value, angle, length);
        }

        public class CantList : SpanList
        {
            internal protected CantList()
            {
            }

            public void Constant(float length) => Add(Value, 0, length);
            public void TransitionTo(float angle, float length) => Add(Value, angle - Value, length);
        }
    }
}
