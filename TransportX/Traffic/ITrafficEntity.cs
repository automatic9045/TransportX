using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

namespace TransportX.Traffic
{
    public interface ITrafficEntity : IWorldObject
    {
        float Width { get; }
        float Height { get; }
        float Length { get; }

        bool IsEnabled { get; }
        ILanePath? Path { get; }
        EntityDirection Heading { get; }
        float S { get; }
        float SVelocity { get; }

        bool Spawn(ILanePath path, EntityDirection heading, float s);
    }
}
