using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Worlds
{
    public class WorldIdentifierAttribute : Attribute
    {
        public string Identifier { get; }

        public WorldIdentifierAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}
