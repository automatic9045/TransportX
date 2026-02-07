using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Traffic
{
    public interface IRouteNavigator
    {
        IReadOnlyCollection<LanePathView> PlannedRoute { get; }
        float PlannedLength { get; }

        void Reset();
        bool TryPop(out LanePathView pathView);
        void Update(LanePathView currentPath, float planLength);
    }
}
