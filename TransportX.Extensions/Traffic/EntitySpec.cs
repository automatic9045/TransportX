using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;

namespace TransportX.Extensions.Traffic
{
    public class EntitySpec
    {
        public required float Width { get; init; }
        public required float Height { get; init; }
        public required float Length { get; init; }

        public IComponentCollection<IComponent> Components { get; } = new ComponentCollection<IComponent>();

        public EntitySpec()
        {
        }
    }
}
