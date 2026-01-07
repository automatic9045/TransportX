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

namespace Bus.Common.Extensions.Networks
{
    public class BezierSpline : SplineBase
    {
        private readonly Vector3 FromUp;
        private readonly Vector3 ToUp;

        public CubicBezier3D CenterCurve { get; }

        public override NetworkPort Inlet { get; }
        public override NetworkPort Outlet { get; }

        private readonly List<LanePath> PathsKey = [];
        public override IReadOnlyList<LanePath> Paths => PathsKey;

        public override float Length => CenterCurve.Length;

        public BezierSpline(ID3D11Device device, IPhysicsHost physicsHost,
            int plateX, int plateZ, Matrix4x4 fromTransform, Matrix4x4 toTransform, LaneLayout outletLayout, float handleScale = 0.5f)
            : base(device, physicsHost, plateX, plateZ, fromTransform)
        {
            Matrix4x4.Invert(fromTransform, out Matrix4x4 fromTranformInv);
            Matrix4x4 transition = Matrix4x4.CreateRotationY(-float.Pi) * toTransform * fromTranformInv;

            Vector3 p0 = Vector3.Zero;
            Vector3 p1 = Vector3.UnitZ * (transition.Translation.Length() * handleScale);
            Vector3 p3 = transition.Translation;
            Vector3 toDirection = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, transition));
            Vector3 p2 = p3 - toDirection * (transition.Translation.Length() * handleScale);

            FromUp = Vector3.TransformNormal(Vector3.UnitY, Matrix4x4.Identity);
            ToUp = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitY, transition));

            CenterCurve = new CubicBezier3D(p0, p1, p2, p3);

            Inlet = new NetworkPort(nameof(Inlet), this, Matrix4x4.CreateRotationY(float.Pi), outletLayout.Opposition);
            Outlet = new NetworkPort(nameof(Outlet), this, transition, outletLayout);

            for (int i = 0; i < Inlet.Layout.Lanes.Count; i++)
            {
                LanePin inletPin = Inlet.Pins[i];
                LanePin outletPin = Outlet.Pins[Inlet.Layout.Lanes.Count - 1 - i];

                BezierSplineLanePath path = new(inletPin, outletPin);
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

        public override Matrix4x4 GetTransform(float s)
        {
            float t = CenterCurve.GetT(s);
            Vector3 position = CenterCurve.GetPoint(t);
            Vector3 tangent = CenterCurve.GetTangent(t);
            Vector3 up = GetUp(s);

            return Matrix4x4.CreateWorld(position, -tangent, up);
        }
    }
}
