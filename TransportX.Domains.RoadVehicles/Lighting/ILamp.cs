using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public interface ILamp
    {
        public static ILamp Empty() => new EmptyLamp();


        void Tick(bool isLighting, TimeSpan elapsed);
        void Reset();


        private class EmptyLamp : ILamp
        {
            public void Tick(bool isLighting, TimeSpan elapsed) { }
            public void Reset() { }
        }
    }
}
