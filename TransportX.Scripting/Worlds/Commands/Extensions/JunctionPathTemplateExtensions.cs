using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Worlds.Components;

namespace TransportX.Scripting.Worlds.Commands.Extensions
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
