using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Powertrain.Controllers
{
    public class ScriptableController : IController
    {
        public Action<TimeSpan> OnTick { get; init; } = _ => { };

        public ScriptableController(Action constructor)
        {
            constructor.Invoke();
        }

        public void Tick(TimeSpan elapsed)
        {
            OnTick.Invoke(elapsed);
        }
    }
}
