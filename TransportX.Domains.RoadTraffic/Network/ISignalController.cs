using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadTraffic.Network
{
    public interface ISignalController
    {
        IReadOnlyDictionary<string, SignalColor> Signals { get; }

        void Tick(DateTime now);
        SignalColor GetSignal(string groupKey);
    }
}
