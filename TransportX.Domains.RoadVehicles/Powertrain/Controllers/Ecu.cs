using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;
using TransportX.Mathematics;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Modules;

namespace TransportX.Domains.RoadVehicles.Powertrain.Controllers
{
    public class Ecu : IController
    {
        private readonly PidController IdlingController = new(0, 1);

        private readonly Engine Engine;
        private readonly ClutchBase Clutch;
        private readonly Gearbox Gearbox;

        public required Signal<float> PedalThrottle { get; init; }
        public Signal<float> MinThrottle { get; init; } = new(0);
        public Signal<float> MaxThrottle { get; init; } = new(0);

        public required bool AntiStall { get; init; }
        public required PidGains IdlingGains { get; init; }

        public required float NIdlingRpm { get; init; }
        public required float NonNIdlingRpm { get; init; }
        public required float LimitRpm { get; init; }

        public Ecu(Engine engine, ClutchBase clutch, Gearbox gearbox)
        {
            Engine = engine;
            Clutch = clutch;
            Gearbox = gearbox;
        }

        public void Tick(TimeSpan elapsed)
        {
            float throttle = float.Clamp(PedalThrottle.Value, MinThrottle.Value, MaxThrottle.Value);

            if (/*ASR.IsSlipping || */LimitRpm < Engine.Rpm)
            {
                throttle = 0;
                IdlingController.Reset();
            }

            if (AntiStall || Gearbox.Gear == 0 || Clutch.Engagement < 1e-3f)
            {
                IdlingController.K = IdlingGains;

                float targetRpm = Gearbox.Gear == 0 ? NIdlingRpm : NonNIdlingRpm;
                float idlingThrottle = IdlingController.Next(targetRpm - Engine.Rpm, elapsed);
                if (throttle < idlingThrottle)
                {
                    Engine.Throttle = idlingThrottle;
                }
                else
                {
                    Engine.Throttle = throttle;
                    IdlingController.Reset();
                }
            }
            else
            {
                Engine.Throttle = throttle;
                IdlingController.Reset();
            }
        }
    }
}
