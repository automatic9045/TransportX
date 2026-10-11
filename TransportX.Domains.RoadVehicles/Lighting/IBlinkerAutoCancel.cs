using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Lighting
{
    public interface IBlinkerAutoCancel
    {
        public static readonly IBlinkerAutoCancel DoNothing = new NoAutoCancel();


        bool ShouldCancel(BlinkerLeverDirection leverDirection, TimeSpan elapsed);
        void Reset();


        private class NoAutoCancel : IBlinkerAutoCancel
        {
            public bool ShouldCancel(BlinkerLeverDirection leverDirection, TimeSpan elapsed) => false;
            public void Reset() { }
        }
    }
}
