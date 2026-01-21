using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Traffic
{
    public interface ITrafficParticipant : ILocatable
    {
        float Width { get; }
        float Length { get; }

        bool IsEnabled { get; }
        LanePath? Path { get; }
        ParticipantDirection Heading { get; }
        float S { get; }
        float SVelocity { get; }

        bool Spawn(LanePath path, ParticipantDirection heading, float s);
    }
}
