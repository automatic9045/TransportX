using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using Bus.Common.Physics;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks.Elements
{
    public class SplineFactory : LocatableObject
    {
        private readonly ID3D11Device Device;
        private readonly IPhysicsHost PhysicsHost;

        private readonly List<SplineStructure> Structures = [];

        private Func<NetworkEdge, SplineBase>? Finalizer = null;

        public LaneLayout OutletLayout { get; }
        public NetworkPort? SourcePort { get; }

        public CurveList Curves { get; } = new();
        public GradientList Gradients { get; } = new();
        public CantList Cants { get; } = new();

        public SplineFactory(ID3D11Device device, IPhysicsHost physicsHost, int plateX, int plateZ, Pose pose, LaneLayout outletLayout, NetworkPort? sourcePort)
            : base(plateX, plateZ, pose)
        {
            Device = device;
            PhysicsHost = physicsHost;
            OutletLayout = outletLayout;

            if (sourcePort is not null && sourcePort.Layout != OutletLayout) throw new ArgumentException("進路の接続部形状が一致しません。", nameof(sourcePort));
            SourcePort = sourcePort;
        }

        public void PutStructure(SplineStructure structure)
        {
            Structures.Add(structure);
        }

        public void PutStructures(IEnumerable<SplineStructure> structures)
        {
            foreach (SplineStructure structure in structures)
            {
                PutStructure(structure);
            }
        }

        public void InterpolateByBezier(NetworkPort targetPort, float handleScale = 0.5f)
        {
            Finalizer = last =>
            {
                Pose from = last.Outlet.Offset * last.Pose;

                NetworkElement targetElement = targetPort.Owner;
                PlateOffset offset = new PlateOffset(
                    targetElement.PlateX - last.PlateX,
                    targetElement.PlateZ - last.PlateZ
                );
                Pose to = targetPort.Offset * targetElement.Pose * offset.Pose;

                BezierSpline spline = new(Device, PhysicsHost, last.PlateX, last.PlateZ, from, to, last.Outlet.Layout, handleScale);
                last.Outlet.ConnectTo(spline.Inlet);
                spline.Outlet.ConnectTo(targetPort);

                return spline;
            };
        }

        public List<SplineBase> Build()
        {
            List<SplineBase> splines = [];
            NetworkPort? sourcePort = SourcePort;

            int splineX = PlateX;
            int splineZ = PlateZ;
            Pose splinePose = Pose;
            Pose splinePoseInv = Pose.Inverse(splinePose);

            Queue<Span> curves = new(Curves.Spans);
            Queue<Span> gradients = new(Gradients.Spans);
            Queue<Span> cants = new(Cants.Spans);

            float s = 0;
            List<SplineSegment> segments = [];
            while (0 < curves.Count)
            {
                Span curve = 0 < curves.Count ? curves.Peek() : new Span(0, 0, s, float.MaxValue);
                Span gradient = 0 < gradients.Count ? gradients.Peek() : new Span(0, 0, s, float.MaxValue);
                Span cant = 0 < cants.Count ? cants.Peek() : new Span(0, 0, s, float.MaxValue);

                float nextS = float.Min(float.Min(curve.ToS, gradient.ToS), cant.ToS);
                float length = nextS - s;

                Span slicedCurve = curve.Slice(s, length);
                Span slicedGradient = gradient.Slice(s, length);
                Span slicedCant = cant.Slice(s, length);

                Pose segmentPose = Pose * splinePoseInv;
                SplineSegment segment = new()
                {
                    FromS = s,
                    ToS = nextS,
                    Length = nextS - s,

                    Pose = segmentPose,

                    Curvature = slicedCurve.FromValue,
                    GradientDelta = slicedGradient.ValueDelta,
                    Cant = slicedCant.FromValue,
                    CantDelta = slicedCant.ValueDelta,
                };
                segments.Add(segment);

                Pose pose = segment.GetLocalPose(slicedCurve.Length);
                PlateOffset plateOffset = Move(pose);

                if (!plateOffset.IsZero)
                {
                    AddSpline();

                    splineX = PlateX;
                    splineZ = PlateZ;
                    splinePose = Pose;
                    splinePoseInv = Pose.Inverse(splinePose);

                    segments = [];
                }

                s = nextS;

                if (0 < curves.Count && curves.Peek().ToS == nextS) curves.Dequeue();
                if (0 < gradients.Count && gradients.Peek().ToS == nextS) gradients.Dequeue();
                if (0 < cants.Count && cants.Peek().ToS == nextS) cants.Dequeue();
            }

            if (0 < segments.Count)
            {
                AddSpline();
            }

            if (Finalizer is not null)
            {
                SplineBase lastSpline = Finalizer(splines[^1]);
                ApplyStructures(lastSpline);
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
                    GradientDelta = segment.GradientDelta,
                    Cant = segment.Cant,
                    CantDelta = segment.CantDelta,
                }).ToArray();

                Spline spline = new(Device, PhysicsHost, splineX, splineZ, splinePose, OutletLayout, normalizedSegments);
                sourcePort?.ConnectTo(spline.Inlet);
                sourcePort = spline.Outlet;
                ApplyStructures(spline);
                splines.Add(spline);
            }

            void ApplyStructures(SplineBase spline)
            {
                for (int structureIndex = 0; structureIndex < Structures.Count; structureIndex++)
                {
                    SplineStructure structure = Structures[structureIndex];
                    if (structure.Count <= 0) continue;

                    int count = int.Min((int)float.Ceiling((spline.Length - structure.From) / structure.Interval), structure.Count);

                    SplineStructure splittedStructure = new(structure.Models, structure.From, structure.Span, structure.Interval, count);
                    spline.AddStructure(splittedStructure);

                    if (count == structure.Count)
                    {
                        Structures[structureIndex] = new SplineStructure(structure.Models, 0, structure.Span, structure.Interval, 0);
                        continue;
                    }

                    LocatedModelTemplate[] nextModels = new LocatedModelTemplate[structure.Models.Count];
                    for (int i = 0; i < nextModels.Length; i++)
                    {
                        nextModels[i] = structure.Models[(i + count) % nextModels.Length];
                    }

                    float nextFrom = structure.From + structure.Interval * count - spline.Length;
                    int nextCount = structure.Count - count;

                    Structures[structureIndex] = new SplineStructure(nextModels, nextFrom, structure.Span, structure.Interval, nextCount);
                }
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

            public void Constant(float length) => Add(0, 0, length);
            public void TransitionBy(float angle, float length) => Add(0, angle, length);
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
