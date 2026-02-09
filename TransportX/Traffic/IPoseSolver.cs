using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

namespace TransportX.Traffic
{
    public interface IPoseSolver : ILocatable
    {
        void Tick(IReadOnlyList<LanePathView> pathViewHistory, LanePathView pathView, float viewS, TimeSpan elapsed);
    }
}
