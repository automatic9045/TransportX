using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Data.Input
{
    public class ControllerButton
    {
        [XmlAttribute]
        public string Key { get; set; } = string.Empty;

        [XmlAttribute]
        public int ButtonIndex { get; set; }
    }
}
