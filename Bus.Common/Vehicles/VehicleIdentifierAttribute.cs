using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Vehicles
{
    public class VehicleIdentifierAttribute : Attribute
    {
        public string Identifier { get; }

        public VehicleIdentifierAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}
