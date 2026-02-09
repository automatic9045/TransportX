using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Network;

namespace Bus.Common.Traffic
{
    public interface IPoseSolver : ILocatable
    {
        void Tick(IReadOnlyList<LanePathView> pathViewHistory, LanePathView pathView, float viewS, TimeSpan elapsed);
    }
}
