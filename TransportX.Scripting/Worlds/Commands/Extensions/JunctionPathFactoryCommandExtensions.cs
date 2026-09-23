using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting.Worlds.Commands.Templates;

namespace TransportX.Scripting.Worlds.Commands.Extensions
{
    public static class JunctionPathFactoryCommandExtensions
    {
        public static JunctionPathFactoryCommand TrafficDensity(this JunctionPathFactoryCommand command, double factor)
        {
            if (command.Components.TryGet<TrafficDensity>(out _))
            {
                command.Components.Remove<TrafficDensity>();
            }

            TrafficDensity component = new((float)factor);
            command.Components.Add(component);

            return command;
        }
    }
}
