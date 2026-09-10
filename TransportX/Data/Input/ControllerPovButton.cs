using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using TransportX.Input;

namespace TransportX.Data.Input
{
    public class ControllerPovButton : ControllerButtonBase
    {
        [XmlAttribute]
        public int PovIndex { get; set; } = 0;

        [XmlAttribute]
        public JoystickPovDirection Direction { get; set; } = JoystickPovDirection.Up;
    }
}
