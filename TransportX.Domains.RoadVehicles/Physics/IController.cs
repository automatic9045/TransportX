using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Physics
{
    public interface IController
    {
        public static IController Empty() => new EmptyController();


        void Tick(TimeSpan elapsed);


        private class EmptyController : IController
        {
            public void Tick(TimeSpan elapsed)
            {
            }
        }
    }
}
