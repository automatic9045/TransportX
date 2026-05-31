using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Bodies;
using TransportX.Network;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public abstract class AgentBase : RigidBody, IAutonomousEntity
    {
        private ILaneTracker? SubscribedTracker = null;
        private WireframeDebugModel? DebugModel = null;

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
        public EntityDirection Heading => LaneTracker.Heading;
        public float S => LaneTracker.S;
        public float SVelocity => LaneTracker.SVelocity;

        public IEnumerable<ITrafficEntity> Obstacles { get; }

        protected Vector4 DebugColor
        {
            get => field;
            set => DebugModel?.Color = field = value;
        } = new Vector4(0, 0, 1, 1);

        protected AgentBase(IPhysicsHost physicsHost, IEnumerable<ITrafficEntity> obstacles) : base(physicsHost)
        {
            Obstacles = obstacles;
        }

        public override void Dispose()
        {
            base.Dispose();
            DebugModel?.Dispose();
            Sensor.Dispose();
        }

        public bool Spawn(ILanePath path, EntityDirection heading, float s)
        {
            FlowDirections direction = heading == EntityDirection.Forward ? FlowDirections.Out : FlowDirections.In;
            if (!path.Directions.HasFlag(direction)) throw new ArgumentException("進行方向が進路の方向と一致しません。", nameof(heading));

            if (SubscribedTracker is null)
            {
                LaneTracker.PathChanged += OnPathChanged;
                SubscribedTracker = LaneTracker;
            }

            LaneTracker.Initialize(path, heading, s);

            Tick(TimeSpan.Zero);
            TeleportTo(WorldPose);
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
            if (SubscribedTracker != LaneTracker) throw new NotSupportedException($"実行中に {nameof(LaneTracker)} プロパティの値を変更することはできません。");

            Sensor.Tick(Navigator.PlannedRoute, Obstacles, elapsed);
            Driver.Tick(elapsed);

            LaneTracker.Tick(Driver.Acceleration, elapsed);
            if (!LaneTracker.IsEnabled)
            {
                Spatial.WorldPose worldPose = new(ChunkIndex.Zero, new Pose(0, -1000 - Random.Shared.NextSingle() * 1000, 0));
                Locate(worldPose);
                return;
            }

            LanePathView pathView = new(Path!, Heading);
            PoseSolver.Tick(LaneTracker.History, pathView, pathView.ToViewS(S), elapsed);
            Locate(PoseSolver.WorldPose);
        }

        public override void Draw(in TransformedDrawContext context)
        {
            base.Draw(context);

            if (context.Layer == RenderLayer.Traffic)
            {
                if (DebugModel is null)
                {
                    DebugModel = this.CreateDebugModel(context.DeviceContext.Device);
                    DebugModel.DebugName = GetType().Name;
                    DebugModel.Color = DebugColor;
                }

                Matrix4x4 world = (WorldPose.Pose * context.ChunkOffset.Pose).ToMatrix4x4();
                context.DrawModel(DebugModel, world);
            }
        }
    }
}
