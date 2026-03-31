using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Network;

using TransportX.Extensions.Mathematics;
using TransportX.Extensions.Network.Paths;

namespace TransportX.Extensions.Network.Elements
{
    public class BezierSpline : SplineBase
    {
        private readonly Vector3 FromUp;
        private readonly Vector3 ToUp;

        public CubicBezier3D CenterCurve { get; }

        public override NetworkPort Inlet { get; }
        public override NetworkPort Outlet { get; }

        private readonly List<ILanePath> PathsKey = [];
        public override IReadOnlyList<ILanePath> Paths => PathsKey;

        public override float Length => CenterCurve.Length;

        public override IComponentCollection<IComponent> Components { get; } = new ComponentCollection<IComponent>();

        public BezierSpline(int plateX, int plateZ, Pose fromPose, Pose toPose, LaneLayout outletLayout, float handleScale = 0.5f)
            : base(plateX, plateZ, fromPose)
        {
            Pose fromTranformInv = Pose.Inverse(fromPose);
            Pose transition = Pose.CreateRotationY(-float.Pi) * toPose * fromTranformInv;

            Vector3 p0 = Vector3.Zero;
            Vector3 p1 = Vector3.UnitZ * (transition.Position.Length() * handleScale);
            Vector3 p3 = transition.Position;
            Vector3 toDirection = Vector3.Normalize(Pose.TransformNormal(Vector3.UnitZ, transition));
            Vector3 p2 = p3 - toDirection * (transition.Position.Length() * handleScale);

            FromUp = Vector3.TransformNormal(Vector3.UnitY, Matrix4x4.Identity);
            ToUp = Vector3.Normalize(Pose.TransformNormal(Vector3.UnitY, transition));

            CenterCurve = new CubicBezier3D(p0, p1, p2, p3);

            Inlet = new NetworkPort(nameof(Inlet), this, Pose.CreateRotationY(float.Pi), outletLayout.Opposition);
            Outlet = new NetworkPort(nameof(Outlet), this, transition, outletLayout); // TODO

            for (int i = 0; i < Inlet.Layout.Lanes.Count; i++)
            {
                LanePin inletPin = Inlet.Pins[i];
                LanePin outletPin = Outlet.Pins[Inlet.Layout.Lanes.Count - 1 - i];

                BezierSplineLanePath path = new(FormattableString.Invariant($"{i}"), inletPin, outletPin);
                inletPin.Wire(path);
                outletPin.Wire(path);
                PathsKey.Add(path);
            }
        }

        public override Vector3 GetPoint(float s)
        {
            float t = CenterCurve.GetT(s);
            return CenterCurve.GetPoint(t);
        }

        public override Vector3 GetUp(float s)
        {
            float t = CenterCurve.GetT(s);
            return Vector3.Normalize(Vector3.Lerp(FromUp, ToUp, t));
        }

        public override Pose GetPose(float s)
        {
            float t = CenterCurve.GetT(s);
            Vector3 position = CenterCurve.GetPoint(t);
            Vector3 tangent = CenterCurve.GetTangent(t);
            Vector3 up = GetUp(s);

            return Pose.CreateWorldLH(position, tangent, up);
        }
    }
}
