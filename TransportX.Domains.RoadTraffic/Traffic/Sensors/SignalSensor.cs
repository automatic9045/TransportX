using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Rendering;
using TransportX.Traffic;

using TransportX.Extensions.Traffic;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Traffic.Sensors
{
    internal class SignalSensor : ITrafficSensor
    {
        private readonly ILaneTracker LaneTracker;
        private readonly TrafficSensorDebugVisual DebugVisual;

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficParticipant? Target { get; private set; } = null;
        public bool IsTargetOncoming => false;
        public float DistanceToTarget { get; private set; } = 0;
        public float StopMargin => 0.5f;

        public Vector4 DebugColor
        {
            get => DebugVisual.DebugColor;
            set => DebugVisual.DebugColor = value;
        }
        public string? DebugName
        {
            get => DebugVisual.DebugName;
            set => DebugVisual.DebugName = value;
        }

        public SignalSensor(ILaneTracker laneTracker, ILocatable location)
        {
            LaneTracker = laneTracker;
            DebugVisual = new TrafficSensorDebugVisual(location);
        }

        public void Dispose()
        {
            DebugVisual?.Dispose();
        }

        public void Tick(IReadOnlyCollection<LanePathView> plannedRoute, IEnumerable<ITrafficParticipant> obstacles, TimeSpan elapsed)
        {
            if (LaneTracker.Path is null) throw new InvalidOperationException();

            float totalLength = LaneTracker.Path.Length - new LanePathView(LaneTracker.Path, LaneTracker.Heading).ToViewS(LaneTracker.S);
            foreach (LanePathView view in plannedRoute)
            {
                if (MaxDistance < totalLength) break;

                if (view.Source.Components.TryGet<SignalComponent>(out SignalComponent? component))
                {
                    if (component.Signal == SignalColor.Red || (component.Signal == SignalColor.Yellow && totalLength < float.Abs(LaneTracker.SVelocity)))
                    {
                        Target = new SignalParticipant(view);
                        DistanceToTarget = totalLength;
                        return;
                    }
                }

                totalLength += view.Source.Length;
            }

            Target = null;
            DistanceToTarget = float.MaxValue;
        }

        public void Draw(in LocatedDrawContext context)
        {
            if (context.Pass != RenderPass.Traffic) throw new InvalidOperationException();

            DebugVisual.Target = Target;
            DebugVisual.IsTargetOncoming = IsTargetOncoming;
            DebugVisual.Draw(context);
        }


        private readonly struct SignalParticipant : ITrafficParticipant
        {
            private readonly LanePathView Target;

            public readonly int PlateX => Target.Source.Owner.PlateX;
            public readonly int PlateZ => Target.Source.Owner.PlateZ;
            public readonly Pose Pose { get; }
            public readonly Vector3 Velocity => Vector3.Zero;

            public readonly float Width => 0;
            public readonly float Height => 0;
            public readonly float Length => 0;

            public readonly bool IsEnabled => true;
            public readonly ILanePath? Path => Target.Source;
            public readonly ParticipantDirection Heading { get; }
            public readonly float S { get; }
            public readonly float SVelocity => 0;

            public event EventHandler? Moved
            {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            public SignalParticipant(in LanePathView target)
            {
                Target = target;
                Pose = Target.GetPose(0);
                Heading = Target.Reverse ? ParticipantDirection.Backward : ParticipantDirection.Forward;
                S = Target.FromViewS(0);
            }

            public readonly bool Spawn(ILanePath path, ParticipantDirection heading, float s)
            {
                throw new NotSupportedException();
            }
        }
    }
}
