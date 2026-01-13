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
        private readonly SplineSegment[] Segments;

        public override float Length { get; }

        public override NetworkPort Inlet { get; }
        public override NetworkPort Outlet { get; }

        private readonly List<LanePath> PathsKey = [];
        public override IReadOnlyList<LanePath> Paths => PathsKey;

        public Spline(ID3D11Device device,IPhysicsHost physicsHost,int plateX, int plateZ, Matrix4x4 transform, LaneLayout outletLayout, SplineSegment[] segments)
            : base(device, physicsHost, plateX, plateZ, transform)
        {
            if (segments.Length == 0) throw new ArgumentException("セグメント列が空です。", nameof(segments));

            Segments = segments;
            Length = segments[segments.Length - 1].ToS;

            Matrix4x4 transition = GetTransform(Length);
            Inlet = new NetworkPort(nameof(Inlet), this, Matrix4x4.CreateRotationY(float.Pi), outletLayout.Opposition);
            Outlet = new NetworkPort(nameof(Outlet), this, transition, outletLayout);

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

        public override Matrix4x4 GetTransform(float s)
        {
            ref readonly SplineSegment seg = ref FindSegment(s);
            float ds = s - seg.FromS;

            (Vector3 localPosition, Quaternion localRotation) = seg.GetRelativeTransform(ds);

            Vector3 position = seg.Position + Vector3.Transform(localPosition, seg.Orientation);
            Quaternion rotation = seg.Orientation * localRotation;

            return Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        }

        public override Vector3 GetPoint(float s)
        {
            return GetTransform(s).Translation;
        }

        public override Vector3 GetUp(float s)
        {
            Matrix4x4 transform = GetTransform(s);
            return Vector3.Normalize(new Vector3(transform.M21, transform.M22, transform.M23));
        }

        private ref readonly SplineSegment FindSegment(float s)
        {
            if (s < Segments[0].FromS) return ref Segments[0];
            if (Segments[^1].ToS <= s) return ref Segments[^1];

            int min = 0;
            int max = Segments.Length - 1;

            while (min <= max)
            {
                int middle = (min + max) / 2;
                ref readonly SplineSegment item = ref Segments[middle];

                if (s < item.FromS) max = middle - 1;
                else if (item.ToS < s) min = middle + 1;
                else return ref item;
            }

            return ref Segments[^1];
        }

        public T ConnectNew<T>(PortDefinition targetPortDef, Func<int, int, Matrix4x4, T> elementFactory) where T : NetworkElement
        {
            Matrix4x4.Invert(targetPortDef.Offset, out Matrix4x4 offsetInv);
            Matrix4x4 currentTrans = Outlet.Offset * Transform;
            Matrix4x4 transform = offsetInv * Matrix4x4.CreateRotationY(-float.Pi) * currentTrans;

            T element = elementFactory(PlateX, PlateZ, transform);
            Outlet.ConnectTo(element.Ports[targetPortDef.Name]);
            return element;
        }
    }
}
