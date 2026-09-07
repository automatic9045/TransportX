using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Data
{
    public class GameControllerReference
    {
        private static int CreationCount = 0;


        [XmlAttribute]
        public string Key { get; set; } = $"{nameof(GameControllerReference)}_{++CreationCount}";

        [XmlAttribute]
        public Guid DeviceGuid { get; set; } = Guid.Empty;

        [XmlAttribute]
        public string Path { get; set; } = string.Empty;
    }
}
