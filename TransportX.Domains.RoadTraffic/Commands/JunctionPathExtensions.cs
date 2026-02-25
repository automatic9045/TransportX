using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Commands;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Commands
{
    public static class JunctionPathExtensions
    {
        public static JunctionPathTemplate Deflection(this JunctionPathTemplate template, double forward, double backward)
        {
            PathDeflectionComponent component = new((float)forward, (float)backward);
            template.Components.Add(component);
            return template;
        }

        public static JunctionPathTemplate Deflection(this JunctionPathTemplate template, double forward)
        {
            return Deflection(template, forward, -forward);
        }
    }
}
