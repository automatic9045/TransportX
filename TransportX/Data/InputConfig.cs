using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TransportX.Data
{
    public class InputConfig
    {
        [XmlArrayItem("GameController")]
        public List<GameControllerReference> GameControllers { get; set; } = [];

        [XmlArrayItem("InputProfile")]
        public List<InputProfileReference> Profiles { get; set; } = [];
    }
}
