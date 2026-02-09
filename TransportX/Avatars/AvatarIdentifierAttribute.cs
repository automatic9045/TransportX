using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Avatars
{
    public class AvatarIdentifierAttribute : Attribute
    {
        public string Identifier { get; }

        public AvatarIdentifierAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}
