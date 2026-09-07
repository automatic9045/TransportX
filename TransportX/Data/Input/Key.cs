using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Data.Input
{
    [XmlType("InputKey")]
    public class Key
    {
        [XmlAttribute]
        public Silk.NET.Input.Key Code { get; set; }
    }
}
