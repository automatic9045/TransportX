using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Bodies;
using Bus.Common.Physics;
using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;

namespace Bus.Common.Extensions.Traffic
{
    public abstract class AgentBase : RigidBody, ITrafficParticipant
    {
        private ILaneTracker? SubscribedLocomotion = null;

        public abstract IRouteNavigator Navigator { get; }
        public abstract ILaneTracker LaneTracker { get; }
        public abstract IPoseSolver PoseSolver { get; }
        public abstract ITrafficSensor Sensor { get; }
        public abstract IDriver Driver { get; }

        public float Width => LaneTracker.Width;
        public float Height => LaneTracker.Height;
        public float Length => LaneTracker.Length;

        public bool IsEnabled => LaneTracker.IsEnabled;
        public ILanePath? Path => LaneTracker.Path;
        public ParticipantDirection Heading => LaneTracker.Heading;
        public float S => LaneTracker.S;
        public float SVelocity => LaneTracker.SVelocity;

        public IEnumerable<ITrafficParticipant> Obstacles { get; set; } = [];

        protected AgentBase(IPhysicsHost physicsHost) : base(physicsHost)
        {
        }

        public bool Spawn(ILanePath path, ParticipantDirection heading, float s)
        {
            FlowDirections direction = heading == ParticipantDirection.Forward ? FlowDirections.Out : FlowDirections.In;
            if (!path.Directions.HasFlag(direction)) throw new ArgumentException("進行方向が進路の方向と一致しません。", nameof(heading));

            if (SubscribedLocomotion is null)
            {
                LaneTracker.PathChanged += OnPathChanged;
                SubscribedLocomotion = LaneTracker;
            }

            LaneTracker.Initialize(path, heading, s);

            Tick(TimeSpan.Zero);
            TeleportTo(PlateX, PlateZ, Pose);
            return true;
        }

        private void OnPathChanged(object? sender, PathChangedEventArgs e)
        {
            e.OldPath?.Exit(this);
            e.NewPath?.Enter(this);
        }

        public override void Tick(TimeSpan elapsed)
        {
            if (!IsEnabled) return;
            if (Path is null) throw new InvalidOperationException();
            if (SubscribedLocomotion != LaneTracker) throw new NotSupportedException($"実行中に {nameof(LaneTracker)} プロパティの値を変更することはできません。");

            Sensor.Tick(Navigator.PlannedRoute, Obstacles, elapsed);
            Driver.Tick(elapsed);

            LaneTracker.Tick(Driver.Acceleration, elapsed);
            if (!LaneTracker.IsEnabled)
            {
                Locate(0, 0, Pose.Identity);
                return;
            }

            LanePathView pathView = new(Path!, Heading);
            PoseSolver.Tick(LaneTracker.History, pathView, pathView.ToViewS(S), elapsed);
            Locate(PoseSolver);
        }
    }
}
