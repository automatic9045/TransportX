using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Audio;
using Bus.Common.Input;

using Bus.Sample.Vehicles.Interfaces;

namespace Bus.Sample.Vehicles.Drives
{
    internal class DoorInterlock : IDoorInterlock, IDisposable
    {
        private readonly Pedal Clutch;
        private readonly Pedal Brake;
        private readonly Pedal Throttle;

        private readonly Sound ReleaseSound;

        private TimeSpan SinceDoorClosed = TimeSpan.Zero;

        public DoorInterlockModes Modes { get; private set; } = DoorInterlockModes.None;

        public double ClutchRate => Modes.HasFlag(DoorInterlockModes.Clutch) ? 0 : Clutch.Rate;
        public double BrakeRate => Modes.HasFlag(DoorInterlockModes.Brake) ? 1 : Brake.Rate;
        public double ThrottleRate => Modes.HasFlag(DoorInterlockModes.Throttle) ? 0 : Throttle.Rate;

        public DoorInterlock(Pedal clutch, Pedal brake, Pedal throttle, SoundFactory soundFactory)
        {
            Clutch = clutch;
            Brake = brake;
            Throttle = throttle;

            ReleaseSound = soundFactory.FromFile("AirRelease.wav");
        }

        public void Dispose()
        {
            ReleaseSound.Dispose();
        }

        public void Tick(bool isFrontDoorOpened, bool isMiddleDoorOpened, TimeSpan elapsed)
        {
            switch (Modes)
            {
                case DoorInterlockModes.None:
                case DoorInterlockModes.Throttle:
                    UpdateMode();
                    break;

                case DoorInterlockModes.Throttle | DoorInterlockModes.Brake:
                    if (isMiddleDoorOpened)
                    {
                        SinceDoorClosed = TimeSpan.Zero;
                    }
                    else
                    {
                        SinceDoorClosed += elapsed;
                        if (1.5 < SinceDoorClosed.TotalSeconds)
                        {
                            UpdateMode();
                            SinceDoorClosed = TimeSpan.Zero;
                            ReleaseSound.Play(false);
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }


            void UpdateMode()
            {
                Modes = isMiddleDoorOpened ? DoorInterlockModes.Throttle | DoorInterlockModes.Brake
                    : isFrontDoorOpened ? DoorInterlockModes.Throttle
                    : DoorInterlockModes.None;
            }
        }
    }
}
