using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.XAudio2;

namespace Bus.Sample.Vehicles.Drives
{
    internal abstract class TransmissionBase : ITransmission, IDisposable
    {
        protected readonly Engine Engine;
        protected readonly DoorInterlock Interlock;

        protected abstract IReadOnlyList<double> GearRatios { get; }
        protected abstract IReadOnlyList<double> ReverseGearRatios { get; }
        protected abstract double FinalRatio { get; }

        public abstract string PositionText { get; }
        public abstract Vector2 LeverCoord { get; }
        public abstract double Torque { get; }

        public int Gear { get; protected set; } = 0;

        public TransmissionBase(Engine engine, DoorInterlock interlock)
        {
            Engine = engine;
            Interlock = interlock;
        }

        protected double GetRatio(int gear)
        {
            double gearRatio = gear < 0 ? ReverseGearRatios[-gear - 1] : gear == 0 ? 0 : GearRatios[gear - 1];
            return gearRatio * FinalRatio;
        }

        protected double GetTheoreticalRpm(double vehicleAngularVelocity, int gear)
        {
            return vehicleAngularVelocity * GetRatio(gear) / (2 * double.Pi) * 60;
        }

        public abstract void Dispose();
        public abstract void ComputeTick(double vehicleAngularVelocity, TimeSpan elapsed);
        public abstract void UpdateSound(X3DAudio x3dAudio, Listener listener);
    }
}
