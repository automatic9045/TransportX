using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public interface ITrafficSpawner
    {
        IList<IParticipantFactory> ParticipantFactories { get; }

        void Initialize(IEnumerable<ILanePath> paths, IEnumerable<NetworkPort> sourcePorts);
        void Tick(TimeSpan elapsed);
    }
}
