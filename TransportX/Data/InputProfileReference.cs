using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Data
{
    public class InputProfileReference
    {
        [XmlAttribute]
        public string Key { get; set; } = string.Empty;

        [XmlAttribute]
        public string Path { get; set; } = string.Empty;
    }
}
