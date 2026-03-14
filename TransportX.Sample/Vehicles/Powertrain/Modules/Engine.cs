using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

using TransportX.Sample.Mathematics;
using TransportX.Sample.Vehicles.Interfaces;
using TransportX.Sample.Vehicles.Powertrain.Modules.Audio;
using TransportX.Sample.Vehicles.Powertrain.Physics;

namespace TransportX.Sample.Vehicles.Powertrain.Modules
{
    internal class Engine : IModule
    {
        private static readonly float FrictionTorque = 200;

        private static readonly Diagram Performance = new Diagram([
            new DiagramPoint(0, 0),
            new DiagramPoint(990, 626),
            new DiagramPoint(1200, 700),
            new DiagramPoint(1390, 734),
            new DiagramPoint(1950, 735),
            new DiagramPoint(2415, 700),
            new DiagramPoint(3775, 0),
        ]);


        private readonly Shaft Output;

        public IEnumerable<IConstraint> Constraints { get; } = [];

        public ECU ECU { get; }
        public EngineAudio Audio { get; }

        public float AngularVelocity => Output.AngularVelocity;
        public float Rpm => Output.Rpm;

        public Engine(Pedal throttlePedal, Shaft output, SoundFactory soundFactory)
        {
            Output = output;

            ECU = new ECU(throttlePedal);
            Audio = new EngineAudio(this, soundFactory);
        }

        public void Tick(TimeSpan elapsed)
        {
            ECU.Tick(Output.Rpm, elapsed);

            if (Output.AngularVelocity < 1e-3f)
            {
                //Output.AngularVelocity = 0;
                //return;
            }

            float maxTorque = Performance.GetValue(Rpm) + FrictionTorque;
            float driveTorque = maxTorque * ECU.Throttle;
            Output.ApplyImpulse(driveTorque * (float)elapsed.TotalSeconds);
            float oldAngularVelocity = Output.AngularVelocity;

            float frictionTorque = -float.Sign(Output.AngularVelocity) * FrictionTorque;
            Output.ApplyImpulse(frictionTorque * (float)elapsed.TotalSeconds);
            if (float.Sign(oldAngularVelocity) != float.Sign(Output.AngularVelocity)) Output.AngularVelocity = 0;

            Output.Torque = driveTorque + frictionTorque;
        }

        public void UpdateSound(Camera camera)
        {
            Audio.UpdateSound(camera);
        }
    }
}
