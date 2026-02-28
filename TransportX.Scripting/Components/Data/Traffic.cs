using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Scripting.Components.Data
{
    [XmlRoot]
    public class Traffic
    {
        [XmlElement(nameof(Spawner))]
        public List<Spawner> Spawners = [];
    }
}
