using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Data.Input
{
    public class Axis
    {
        [XmlAttribute]
        public string Key { get; set; } = string.Empty;

        [XmlArrayItem("Action")]
        public List<KeyboardAction> KeyboardPlus { get; set; } = [];

        [XmlArrayItem("Action")]
        public List<KeyboardAction> KeyboardMinus { get; set; } = [];

        [XmlArrayItem("Action")]
        public List<KeyboardAction> KeyboardReset { get; set; } = [];

        [XmlArrayItem("Controller", typeof(ControllerAxis))]
        [XmlArrayItem("ControllerPov", typeof(ControllerPovAxis))]
        public List<ControllerAxisBase> Controllers { get; set; } = [];
    }
}
