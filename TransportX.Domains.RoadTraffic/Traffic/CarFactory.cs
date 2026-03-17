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
        private readonly IModel BlinkerLightLModel;
        private readonly IModel BlinkerLightRModel;
        private readonly IModel BrakeLightModel;
        private readonly CarSpec CarSpec;

        ParticipantSpec IParticipantFactory.Spec => Spec;

        public CarFactory(IModel model, IModel blinkerLightLModel, IModel blinkerLightRModel, IModel brakeLightModel, CarSpec carSpec)
        {
            Model = model;
            BlinkerLightLModel = blinkerLightLModel;
            BlinkerLightRModel = blinkerLightRModel;
            BrakeLightModel = brakeLightModel;
            CarSpec = carSpec;
        }

        public ITrafficParticipant Create(in TrafficSpawnContext context)
        {
            DriverPersonality personality = new()
            {
                Factor = Random.Shared.NextSingle(),
                TimeHeadway = 1.5f + Random.Shared.NextSingle() * 1.5f, // 1.5～3
                StartUpReactionTime = 0.5f + Random.Shared.NextSingle() * 1, // 0.5～1.5
                CreepSpeed = 1.5f + Random.Shared.NextSingle() * 1.5f, // 1.5～3
            };

            Car car = new(context.PhysicsHost, context.Obstacles, Model, BlinkerLightLModel, BlinkerLightRModel, BrakeLightModel, CarSpec, personality);
            context.Bodies.Add(car);

            car.Sensor.DebugName = $"{nameof(Car)}_Sensor";

            return car;
        }
    }
}
