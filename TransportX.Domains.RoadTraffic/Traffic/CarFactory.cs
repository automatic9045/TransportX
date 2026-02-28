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

        ParticipantSpec IParticipantFactory.Spec => Spec;

        public CarFactory(IModel model, IModel blinkerLModel, IModel blinkerRModel)
        {
            Model = model;
            BlinkerLModel = blinkerLModel;
            BlinkerRModel = blinkerRModel;
        }

        public ITrafficParticipant Create(in TrafficSpawnContext context)
        {
            Car car = new(context.PhysicsHost, context.Obstacles, Model, BlinkerLModel, BlinkerRModel);
            context.Bodies.Add(car);

            car.Sensor.DebugName = $"{nameof(Car)}_Sensor";

            return car;
        }
    }
}
