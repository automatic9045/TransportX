using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Components;

namespace TransportX.Scripting.Commands.Extensions
{
    public static class JunctionPathTemplateExtensions
    {
        public static JunctionPathTemplate TrafficDensity(this JunctionPathTemplate template, double factor)
        {
            TrafficDensity component = new((float)factor);
            template.Components.Add(component);
            return template;
        }
    }
}
