using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Scripting.Data.Network
{
    [XmlRoot]
    public class LaneLayout
    {
        public List<Lane> Lanes = [];
    }
}
