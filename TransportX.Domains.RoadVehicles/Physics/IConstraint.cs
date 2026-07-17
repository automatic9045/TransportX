using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Physics
{
    public interface IConstraint
    {
        void Solve();
    }
}
