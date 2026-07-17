using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Powertrain
{
    public class PowertrainComponent : IStartableComponent, ISubTickableComponent
    {
        public event Action? Started;

        public Simulation Simulation { get; }

        public PowertrainComponent()
        {
            Simulation = new Simulation();
        }

        public void OnStart()
        {
            Started?.Invoke();
        }

        public void SubTick(TimeSpan elapsed)
        {
            Simulation.Tick(elapsed);
        }
    }
}
