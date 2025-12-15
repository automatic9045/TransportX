using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles.Powertrain.Physics
{
    internal interface IModule
    {
        IEnumerable<IConstraint> Constraints { get; }

        void Tick(TimeSpan elapsed) { }
        void PropagateTorque() { }
    }
}
