using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.Vehicles.Powertrain.Physics
{
    internal interface IConstraint
    {
        void Solve();
    }
}
