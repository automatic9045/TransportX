using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Commands;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands
{
    public static class JunctionPathTemplateExtensions
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

        public static JunctionPathTemplate Yield(this JunctionPathTemplate template, params string[] priorityPathKey)
        {
            Templates.Yield component = new(priorityPathKey);
            template.Components.Add(component);
            return template;
        }
    }
}
