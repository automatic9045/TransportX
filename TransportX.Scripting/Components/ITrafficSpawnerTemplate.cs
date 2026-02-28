using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using TransportX.Network;

using TransportX.Extensions.Traffic;

namespace TransportX.Scripting.Components
{
    public interface ITrafficSpawnerTemplate
    {
        public static ITrafficSpawnerTemplate Default() => new DefaultTemplate();

        ITrafficSpawner Build(LaneTrafficType type, in TrafficSpawnContext context, XElement data);


        private class DefaultTemplate : ITrafficSpawnerTemplate
        {
            public ITrafficSpawner Build(LaneTrafficType type, in TrafficSpawnContext context, XElement data)
            {
                return new Spawner();
            }


            private class Spawner : ITrafficSpawner
            {
                public IList<IParticipantFactory> ParticipantFactories { get; } = [];

                public void Initialize(IEnumerable<ILanePath> paths, IEnumerable<NetworkPort> sourcePorts)
                {
                }

                public void Tick(TimeSpan elapsed)
                {
                }
            }
        }
    }
}
