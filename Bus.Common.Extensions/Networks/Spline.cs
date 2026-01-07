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
    public class Spline : SplineBase
    {
        public override NetworkPort Inlet { get; }
        public override NetworkPort Outlet { get; }

        private readonly List<LanePath> PathsKey = [];
        public override IReadOnlyList<LanePath> Paths => PathsKey;

        public float Curvature { get; }
        public override float Length { get; }

        public Spline(ID3D11Device device, IPhysicsHost physicsHost,
            int plateX, int plateZ, Matrix4x4 transform, LaneLayout outletLayout, float curvature, float length)
            : base(device, physicsHost, plateX, plateZ, transform)
        {
            Curvature = curvature;
            Length = length;

            Inlet = new NetworkPort(nameof(Inlet), this, Matrix4x4.CreateRotationY(float.Pi), outletLayout.Opposition);
            Outlet = new NetworkPort(nameof(Outlet), this, GetTransform(Length), outletLayout);

            for (int i = 0; i < Inlet.Layout.Lanes.Count; i++)
            {
                LanePin inletPin = Inlet.Pins[i];
                LanePin outletPin = Outlet.Pins[Inlet.Layout.Lanes.Count - 1 - i];

                SplineLanePath path = new(inletPin, outletPin);
                inletPin.Wire(path);
                outletPin.Wire(path);
                PathsKey.Add(path);
            }
        }

        public override Vector3 GetPoint(float s)
        {
            float angle = s * Curvature;
            float x = Curvature == 0 ? 0 : (1 - float.Cos(angle)) / Curvature;
            float z = Curvature == 0 ? s : float.Sin(angle) / Curvature;

            return new Vector3(x, 0, z);
        }

        public override Vector3 GetUp(float s)
        {
            return Vector3.UnitY;
        }

        public override Matrix4x4 GetTransform(float s)
        {
            Matrix4x4 transform = Matrix4x4.CreateRotationY(s * Curvature);
            transform.Translation = GetPoint(s);

            return transform;
        }
    }
}
