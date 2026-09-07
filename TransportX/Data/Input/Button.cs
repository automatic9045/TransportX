using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Data.Input
{
    public class Button
    {
        [XmlAttribute]
        public string Key { get; set; } = string.Empty;

        [XmlArrayItem("Key")]
        public List<Key> Keyboard { get; set; } = [];

        [XmlArrayItem("Controller")]
        public List<ControllerButton> Controllers { get; set; } = [];
    }
}
