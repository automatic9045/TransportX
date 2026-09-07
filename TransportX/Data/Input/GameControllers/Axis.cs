using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using TransportX.Input;

namespace TransportX.Data.Input.GameControllers
{
    public class Axis
    {
        [XmlAttribute]
        public JoystickAxisType Type { get; set; }

        [XmlAttribute]
        public int RawMin { get; set; }

        [XmlAttribute]
        public int RawNeutral { get; set; }

        [XmlAttribute]
        public int RawMax { get; set; }

        [XmlAttribute]
        public bool IsInverted { get; set; }
    }
}
