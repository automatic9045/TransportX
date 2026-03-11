using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

using TransportX.Extensions.Network.Elements;

using TransportX.Scripting;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands.Templates
{
    internal class DefaultSignalController : ITemplateComponent<Junction>
    {
        public ISignalController Controller { get; }

        public DefaultSignalController(ISignalController controller)
        {
            Controller = controller;
        }

        public void Build(Junction parent, IErrorCollector errorCollector)
        {
        }
    }
}
