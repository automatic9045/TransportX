using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.XAudio2;

using Bus.Common;

using Bus.Sample.Vehicles.Interfaces;

namespace Bus.Sample.Vehicles.Drives
{
    internal class DriveSet : IDisposable
    {
        private readonly Engine _Engine;
        private readonly DoorInterlock _Interlock;
        private readonly TransmissionBase _Transmission;

        public IEngine Engine => _Engine;
        public IDoorInterlock Interlock => _Interlock;
        public ITransmission Transmission => _Transmission;
        public Chassis Chassis { get; }

        public DriveSet(InterfaceSet interfaces, SoundFactory soundFactory, LocatableObject body)
        {
            _Engine = new Engine(soundFactory, body);
            _Interlock = new DoorInterlock(interfaces.Clutch, interfaces.Brake, interfaces.Throttle, soundFactory);

            _Transmission = new AT(_Engine, _Interlock, interfaces.ATShifter, soundFactory, body);

            Chassis = new Chassis(Transmission, interfaces.Steering);
        }

        public void Dispose()
        {
            _Engine.Dispose();
            _Interlock.Dispose();
            _Transmission.Dispose();
        }

        public void ComputeTick(TimeSpan elapsed)
        {
            _Transmission.ComputeTick(Chassis.AverageAngularVelocity, elapsed);
            Chassis.ComputeTick(elapsed);
        }

        public void Tick(TimeSpan elapsed)
        {
            //_Interlock.Tick(doors.GetSide(DoorSide.Left).IsOpen, doors.GetSide(DoorSide.Right).IsOpen, elapsed);
        }

        public void UpdateSound(X3DAudio x3dAudio, Listener listener)
        {
            _Transmission.UpdateSound(x3dAudio, listener);
        }
    }
}
