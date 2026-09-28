using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Network;

namespace TransportX.Traffic
{
    public interface IRouteNavigator
    {
        IReadOnlyList<LanePathView> PlannedRoute { get; }
        float PlannedLength { get; }

        void Reset();
        bool TryPop(out LanePathView pathView);
        void Update(LanePathView currentPath, float planLength);
    }
}
