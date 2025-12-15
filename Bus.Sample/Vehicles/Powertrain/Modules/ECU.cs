using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Interfaces;

namespace Bus.Sample.Vehicles.Powertrain.Modules
{
    internal class ECU
    {
        private readonly Pedal ThrottlePedal;
        private readonly PIDController IdlingController;

        public Clutch? Clutch { private get; set; }
        public MT? Transmission { private get; set; }
        public ASR? ASR { private get; set; }

        public float Throttle { get; private set; } = 0;

        public ECU(Pedal throttlePedal)
        {
            ThrottlePedal = throttlePedal;
            IdlingController = new PIDController(0.01f, 0.05f, 0.0001f, 0, 1);
        }

        public void Tick(float rpm, TimeSpan elapsed)
        {
            float throttle = ThrottlePedal.Rate;

            if (ASR!.IsSlipping)
            {
                Throttle = 0;
                IdlingController.Reset();
                return;
            }

            if (rpm < 1000 && (Clutch!.Engagement < 1e-3f || Transmission!.Gear == 0))
            {
                float idlingThrottle = IdlingController.Next(600 - rpm, elapsed);
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
            else if (3500 < rpm)
            {
                Throttle = 0;
                IdlingController.Reset();
            }
            else
            {
                Throttle = throttle;
                IdlingController.Reset();
            }
        }
    }
}
