using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Physics
{
    public interface IModule
    {
        public static IModule Empty() => new EmptyModule();


        IReadOnlyList<Shaft> InputShafts { get; }
        IReadOnlyList<Shaft> OutputShafts { get; }

        IReadOnlyList<IConstraint> Constraints { get; }

        void Tick(TimeSpan elapsed);
        void PropagateTorque();


        private class EmptyModule : IModule
        {
            public IReadOnlyList<Shaft> InputShafts { get; } = [];
            public IReadOnlyList<Shaft> OutputShafts { get; } = [];
            public IReadOnlyList<IConstraint> Constraints { get; } = [];

            public void Tick(TimeSpan elapsed)
            {
            }

            public void PropagateTorque()
            {
            }
        }
    }
}
