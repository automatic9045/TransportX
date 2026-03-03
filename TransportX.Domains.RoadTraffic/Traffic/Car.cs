using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;
using TransportX.Traffic;

using TransportX.Extensions.Traffic;

using TransportX.Domains.RoadTraffic.Network;
using TransportX.Domains.RoadTraffic.Traffic.Sensors;

namespace TransportX.Domains.RoadTraffic.Traffic
{
    public class Car : AgentBase
    {
        private const float FrontWheelOffset = -0.7f;
        private const float RearWheelOffset = -3.2f;

        internal new const float Width = 1.8f;
        internal new const float Height = 1.5f;
        internal new const float Length = 3.6f;


        private readonly float BlinkerDistance;

        private readonly Blinker LeftBlinker;
        private readonly Blinker RightBlinker;

        public override IRouteNavigator Navigator { get; }
        public override ILaneTracker LaneTracker { get; }
        public override IPoseSolver PoseSolver { get; }
        public override ITrafficSensor Sensor { get; }
        public override IDriver Driver { get; }

        public Car(IPhysicsHost physicsHost, IEnumerable<ITrafficParticipant> obstacles,
            IModel model, IModel blinkerLModel, IModel blinkerRModel, CarSpec spec, DriverPersonality personality) : base(physicsHost, obstacles)
        {
            Navigator = new RandomRouteNavigator();
            LaneTracker = new LaneTracker(Navigator, Width, Height, Length);
            PoseSolver = new TwoPointPoseSolver(FrontWheelOffset, RearWheelOffset);


            NetworkTrafficSensor networkSensor = new(LaneTracker, PoseSolver)
            {
                DebugColor = new Vector4(0, 1, 1, 1),
            };
            PriorityTrafficSensor prioritySensor = new(LaneTracker, PoseSolver)
            {
                DebugColor = new Vector4(1, 1, 0, 1),
            };
            SpatialTrafficSensor spatialSensor = new(LaneTracker, PoseSolver, obstacle => obstacle != networkSensor.Target && obstacle == this)
            {
                DebugColor = new Vector4(1, 0, 1, 1),
            };
            Sensor = new CompositeTrafficSensor([networkSensor, spatialSensor, prioritySensor]);

            Driver = new CarDriver(Navigator, LaneTracker, Sensor, spec, personality);
            BlinkerDistance = 40 - personality.Factor * 20; // 20～40

            Structure.AttachKinematicOrNonCollision(model, Pose.Identity);
            LocatedModel blinkerL = Structure.Attach(blinkerLModel, Pose.Identity);
            LocatedModel blinkerR = Structure.Attach(blinkerRModel, Pose.Identity);

            TimeSpan blinkerPeriod = TimeSpan.FromSeconds(0.8f);
            LeftBlinker = new Blinker(blinkerL, blinkerPeriod);
            RightBlinker = new Blinker(blinkerR, blinkerPeriod);
        }

        public override void Tick(TimeSpan elapsed)
        {
            base.Tick(elapsed);
            if (!IsEnabled) return;

            float deflection = 0;
            if (Path!.Components.TryGet<PathDeflectionComponent>(out PathDeflectionComponent? component))
            {
                deflection = component.GetDeflection(Heading);
            }
            else
            {
                float distance = Path.Length - new LanePathView(Path, Heading).ToViewS(S);
                foreach (LanePathView planned in Navigator.PlannedRoute)
                {
                    if (BlinkerDistance < distance) break;

                    if (planned.Source.Components.TryGet<PathDeflectionComponent>(out component))
                    {
                        deflection = component.GetDeflection(planned.Reverse);
                        break;
                    }

                    distance += planned.Source.Length;
                }
            }

            LeftBlinker.IsActive = deflection <= -0.5f;
            RightBlinker.IsActive = 0.5f <= deflection;

            LeftBlinker.Tick(elapsed);
            RightBlinker.Tick(elapsed);
        }

        public override void Draw(in LocatedDrawContext context)
        {
            base.Draw(context);
            if (IsEnabled && context.Pass == RenderPass.Traffic) Sensor.Draw(context);
        }
    }
}
