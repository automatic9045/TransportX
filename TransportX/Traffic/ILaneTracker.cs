using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

namespace TransportX.Traffic
{
    public interface ILaneTracker
    {
        float Width { get; }
        float Height { get; }
        float Length { get; }

        bool IsEnabled { get; }
        ILanePath? Path { get; }
        ParticipantDirection Heading { get; }
        float S { get; }
        float SVelocity { get; }

        IReadOnlyList<LanePathView> History { get; }

        event EventHandler<PathChangedEventArgs>? PathChanged;

        void Initialize(ILanePath path, ParticipantDirection heading, float s);
        void Tick(float acceleration, TimeSpan elapsed);
    }
}
