using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Traffic
{
    public interface ITrafficParticipant
    {
        float Length { get; }

        LanePath Path { get; }
        ParticipantDirection Heading { get; }
        float S { get; }
        float Velocity { get; }
    }
}
