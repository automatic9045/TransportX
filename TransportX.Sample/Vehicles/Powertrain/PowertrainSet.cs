using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;

using TransportX.Sample.Vehicles.Interfaces;
using TransportX.Sample.Vehicles.Powertrain.Modules;
using TransportX.Sample.Vehicles.Powertrain.Physics;

namespace TransportX.Sample.Vehicles.Powertrain
{
    internal class PowertrainSet : IDisposable
    {
        private readonly Simulation Simulation;

        public Engine Engine { get; }
        public ClutchBase Clutch { get; }
        public TransmissionBase Transmission { get; }
        public Differential Differential { get; }
        public DriveWheel LeftWheel { get; }
        public DriveWheel RightWheel { get; }
        public ASR ASR { get; }

        public PowertrainSet(InterfaceSet interfaces, SoundFactory soundFactory,
            DynamicLocatedModel wheelRL, DynamicLocatedModel wheelRR, Constraint<AngularAxisMotor> motorRL, Constraint<AngularAxisMotor> motorRR)
        {
            Shaft engineToClutch = new Shaft(3);
            Shaft clutchToTransmission = new Shaft(0.15f);
            Shaft transmissionToDifferential = new Shaft(0.25f);
            Shaft differentialToLeftWheel = new Shaft(20);
            Shaft differentialToRightWheel = new Shaft(20);

            engineToClutch.Rpm = 600;

            Actuator clutchActuator = new();

            Engine = new Engine(interfaces.Throttle, engineToClutch, soundFactory);
            //Clutch = new FrictionClutch(interfaces.Clutch, engineToClutch, clutchToTransmission);
            //Transmission = new MT(interfaces.MTShifter, clutchToTransmission, transmissionToDifferential);
            Clutch = new FluidClutch(clutchActuator, engineToClutch, clutchToTransmission);
            Transmission = new AMT(Engine, (FluidClutch)Clutch, interfaces.AMTShifter, clutchActuator, clutchToTransmission, transmissionToDifferential);
            Differential = new Differential(transmissionToDifferential, differentialToLeftWheel, differentialToRightWheel);
            LeftWheel = new DriveWheel(wheelRL, motorRL, differentialToLeftWheel, true);
            RightWheel = new DriveWheel(wheelRR, motorRR, differentialToRightWheel, false);
            ASR = new ASR(LeftWheel, RightWheel);

            Engine.ECU.Clutch = Clutch;
            Engine.ECU.Transmission = Transmission;
            Engine.ECU.ASR = ASR;
            //Engine.ECU.AntiStall = false; // MT
            //Engine.ECU.IdlingGains = (0.01f, 0.05f, 0.0002f); // MT
            Engine.ECU.IdlingGains = (0.01f, 0.05f, 0.0002f); // AMT

            Simulation = new Simulation();
            Simulation.AddModule(Engine);
            Simulation.AddModule(Clutch);
            Simulation.AddModule(Transmission);
            Simulation.AddModule(Differential);
            Simulation.AddModule(LeftWheel);
            Simulation.AddModule(RightWheel);
        }

        public void Dispose()
        {
            Engine.Audio.Dispose();
        }

        public void Tick(TimeSpan elapsed)
        {
            LeftWheel.Pull();
            RightWheel.Pull();

            Engine.Tick(elapsed);
            Clutch.Tick(elapsed);
            Transmission.Tick(elapsed);
            ASR.Tick(elapsed);

            Simulation.Tick(elapsed);

            Clutch.PropagateTorque();
            Transmission.PropagateTorque();
            Differential.PropagateTorque();

            LeftWheel.Push();
            RightWheel.Push();
        }

        public void UpdateSound(Camera camera)
        {
            Engine.UpdateSound(camera);
        }
    }
}
