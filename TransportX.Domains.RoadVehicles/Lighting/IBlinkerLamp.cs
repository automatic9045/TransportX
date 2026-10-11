using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public interface IBlinkerLamp
    {
        public static IBlinkerLamp Empty() => new EmptyBlinkerLamp();


        void Tick(in BlinkerContext context, TimeSpan elapsed);
        void Reset();


        private class EmptyBlinkerLamp : IBlinkerLamp
        {
            public void Tick(in BlinkerContext context, TimeSpan elapsed) { }
            public void Reset() { }
        }
    }
}
