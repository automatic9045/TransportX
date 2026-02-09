using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Sample.Vehicles.Powertrain.Physics
{
    internal class Simulation
    {
        private static readonly int IterationCount = 10;


        public List<IConstraint> Constraints { get; } = new List<IConstraint>();

        public Simulation()
        {
        }

        public void AddModule(IModule module)
        {
            Constraints.AddRange(module.Constraints);
        }

        public void Tick(TimeSpan elapsed)
        {
            for (int i = 0; i < IterationCount; i++)
            {
                foreach (IConstraint constraint in Constraints)
                {
                    constraint.Solve();
                }
            }
        }
    }
}
