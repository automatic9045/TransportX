using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

namespace TransportX.Domains.Equipment.Doors
{
    public class DoorCollectionComponent : ITickableComponent
    {
        public List<IDoor> Doors { get; } = [];

        public void Tick(TimeSpan elapsed, DateTime now)
        {
            foreach (IDoor door in Doors)
            {
                door.Tick(elapsed);
            }
        }
    }
}
