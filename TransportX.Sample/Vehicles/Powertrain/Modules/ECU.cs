using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.Mathematics;
using TransportX.Sample.Vehicles.Interfaces;

namespace TransportX.Sample.Vehicles.Powertrain.Modules
{
    internal class ECU
    {
        private readonly Pedal ThrottlePedal;
        private readonly PIDController IdlingController;

        public ClutchBase? Clutch { private get; set; }
        public TransmissionBase? Transmission { private get; set; }
        public ASR? ASR { private get; set; }

        public bool AntiStall { get; set; } = true;
        public (float P, float I, float D) IdlingGains { get; set; } = (0, 0, 0);

        public float ThrottleInput => ThrottlePedal.Rate;
        public float Throttle { get; private set; } = 0;

        public ECU(Pedal throttlePedal)
        {
            ThrottlePedal = throttlePedal;
            IdlingController = new PIDController(0, 1);
        }

        public void Tick(float rpm, TimeSpan elapsed)
        {
            float throttle = float.Clamp(ThrottlePedal.Rate, Transmission!.MinThrottle, Transmission!.MaxThrottle);

            if (ASR!.IsSlipping || 3500 < rpm)
            {
                throttle = 0;
                IdlingController.Reset();
            }

            if (AntiStall || Transmission!.Gear == 0 || Clutch!.Engagement < 1e-3f)
            {
                IdlingController.K = IdlingGains;

                float targetRpm = Transmission!.Gear == 0 ? 600 : 575;
                float idlingThrottle = IdlingController.Next(targetRpm - rpm, elapsed);
                if (throttle < idlingThrottle)
                {
                    Throttle = idlingThrottle;
                }
                else
                {
                    Throttle = throttle;
                    IdlingController.Reset();
                }
            }
            else
            {
                Throttle = throttle;
                IdlingController.Reset();
            }
        }
    }
}
