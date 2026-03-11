using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Scripting;

namespace TransportX.Domains.RoadTraffic.Network
{
    public class SignalComponent : ITemplateComponent<ILanePath>
    {
        private readonly ISignalController Controller;

        public string GroupKey { get; }
        public SignalColor Signal => Controller.Signals.TryGetValue(GroupKey, out SignalColor color) ? color : SignalColor.Off;

        public SignalComponent(ISignalController controller, string groupKey)
        {
            Controller = controller;
            GroupKey = groupKey;
        }

        void ITemplateComponent<ILanePath>.Build(ILanePath parent, IErrorCollector errorCollector)
        {
            parent.Components.Add(this);
        }
    }
}
