using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Data.Input
{
    public class KeyboardAction
    {
        [XmlAttribute]
        public string Key { get; set; } = string.Empty;

        [XmlElement("Key")]
        public List<Key> Keys { get; set; } = [];
    }
}
