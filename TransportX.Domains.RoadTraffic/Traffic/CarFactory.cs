using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;
using TransportX.Traffic;

using TransportX.Extensions.Traffic;

namespace TransportX.Domains.RoadTraffic.Traffic
{
    public class CarFactory : IParticipantFactory
    {
        private static readonly ParticipantSpec Spec = new()
        {
            Width = Car.Width,
            Height = Car.Height,
            Length = Car.Length,
        };


        private readonly IModel Model;
        private readonly IModel BlinkerLModel;
        private readonly IModel BlinkerRModel;
        private readonly CarSpec CarSpec;

        ParticipantSpec IParticipantFactory.Spec => Spec;

        public CarFactory(IModel model, IModel blinkerLModel, IModel blinkerRModel, CarSpec carSpec)
        {
            Model = model;
            BlinkerLModel = blinkerLModel;
            BlinkerRModel = blinkerRModel;
            CarSpec = carSpec;
        }

        public ITrafficParticipant Create(in TrafficSpawnContext context)
        {
            DriverPersonality personality = new()
            {
                Factor = Random.Shared.NextSingle(),
                DefaultStopMargin = 1.5f + Random.Shared.NextSingle() * 2, // 1.5～3.5
                TimeHeadway = 1.5f + Random.Shared.NextSingle() * 1.5f, // 1.5～3
            };

            Car car = new(context.PhysicsHost, context.Obstacles, Model, BlinkerLModel, BlinkerRModel, CarSpec, personality);
            context.Bodies.Add(car);

            car.Sensor.DebugName = $"{nameof(Car)}_Sensor";

            return car;
        }
    }
}
