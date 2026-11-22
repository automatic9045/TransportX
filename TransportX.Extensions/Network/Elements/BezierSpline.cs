using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Network;
using TransportX.Spatial;

using TransportX.Extensions.Mathematics;
using TransportX.Extensions.Network.Paths;

namespace TransportX.Extensions.Network.Elements
{
    public class BezierSpline : SplineBase
    {
        public BezierPoseCurve CenterCurve { get; }

        public override NetworkPort Inlet { get; }
        public override NetworkPort Outlet { get; }

        private readonly List<ILanePath> PathsKey = [];
        public override IReadOnlyList<ILanePath> Paths => PathsKey;

        public override float Length => CenterCurve.Length;

        public override IComponentCollection<IComponent> Components { get; } = new ComponentCollection<IComponent>();

        public BezierSpline(WorldPose from, Pose to, LaneLayout outletLayout, float handleScale = 0.5f) : base(from)
        {
            Pose fromTransformInv = Pose.Inverse(from.Pose);
            Pose transition = Pose.CreateRotationY(-float.Pi) * to * fromTransformInv;

            CenterCurve = new BezierPoseCurve(Pose.Identity, transition, handleScale);

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

        public override Pose GetPose(float s) => CenterCurve.GetPose(s);
    }
}
