using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Traffic;

using TransportX.Extensions.Traffic;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Traffic.Sensors
{
    internal class SignalSensor : ITrafficSensor
    {
        private readonly ILaneTracker LaneTracker;
        private readonly TrafficSensorDebugVisual DebugVisual;

        private readonly SignalEntity EntityCache = new();

        public float MaxDistance { get; set; } = float.MaxValue;

        public ITrafficEntity? Target { get; private set; } = null;
        public bool IsTargetOncoming => false;
        public float DistanceToTarget { get; private set; } = 0;
        public float StopMargin { get; init; } = 0.75f;

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

        public SignalSensor(ILaneTracker laneTracker, IWorldObject origin)
        {
            LaneTracker = laneTracker;
            DebugVisual = new TrafficSensorDebugVisual(origin);
        }

        public void Dispose()
        {
            DebugVisual?.Dispose();
        }

        public void Tick(IReadOnlyList<LanePathView> plannedRoute, TimeSpan elapsed)
        {
            if (LaneTracker.Path is null) throw new InvalidOperationException();

            float totalLength = LaneTracker.Path.Length - new LanePathView(LaneTracker.Path, LaneTracker.Heading).ToViewS(LaneTracker.S);
            for (int i = 0; i < plannedRoute.Count; i++)
            {
                if (MaxDistance < totalLength) break;
                LanePathView view = plannedRoute[i];

                if (view.Source.Components.TryGet<SignalComponent>(out SignalComponent? component))
                {
                    if (component.Signal == SignalColor.Red || (component.Signal == SignalColor.Yellow && totalLength < float.Abs(LaneTracker.SVelocity)))
                    {
                        EntityCache.Update(view);
                        Target = EntityCache;
                        DistanceToTarget = totalLength;
                        return;
                    }
                }

                totalLength += view.Source.Length;
            }

            Target = null;
            DistanceToTarget = float.MaxValue;
        }

        public void Draw(in TransformedDrawContext context)
        {
            if (context.Layer != RenderLayer.Traffic) throw new InvalidOperationException();

            DebugVisual.Target = Target;
            DebugVisual.IsTargetOncoming = IsTargetOncoming;
            DebugVisual.Draw(context);
        }


        private sealed class SignalEntity : ITrafficEntity
        {
            private LanePathView Target = default;

            public WorldPose WorldPose { get; private set; } = WorldPose.Zero;
            public Vector3 Velocity => Vector3.Zero;
            public Vector3 AngularVelocity => Vector3.Zero;

            public float Width => 0;
            public float Height => 0;
            public float Length => 0;

            public bool IsEnabled => true;
            public ILanePath? Path => Target.Source;
            public EntityDirection Heading { get; private set; } = EntityDirection.Forward;
            public float S { get; private set; } = 0;
            public float SVelocity => 0;

            public event MovedEventHandler? Moved
            {
                add => throw new NotSupportedException();
                remove => throw new NotSupportedException();
            }

            public SignalEntity()
            {
            }

            public void Update(in LanePathView target)
            {
                Target = target;
                WorldPose = Target.GetWorldPose(0);
                Heading = Target.Reverse ? EntityDirection.Backward : EntityDirection.Forward;
                S = Target.FromViewS(0);
            }

            public bool Spawn(ILanePath path, EntityDirection heading, float s)
            {
                throw new NotSupportedException();
            }
        }
    }
}
