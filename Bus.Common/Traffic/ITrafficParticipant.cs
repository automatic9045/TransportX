using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Network;

namespace Bus.Common.Traffic
{
    public interface ITrafficParticipant : ILocatable
    {
        float Width { get; }
        float Height { get; }
        float Length { get; }

        bool IsEnabled { get; }
        ILanePath? Path { get; }
        ParticipantDirection Heading { get; }
        float S { get; }
        float SVelocity { get; }

        bool Spawn(ILanePath path, ParticipantDirection heading, float s);
    }
}
