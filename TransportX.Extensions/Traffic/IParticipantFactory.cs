using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public interface IParticipantFactory
    {
        ParticipantSpec Spec { get; }

        ITrafficParticipant Create(in TrafficSpawnContext context);
    }
}
