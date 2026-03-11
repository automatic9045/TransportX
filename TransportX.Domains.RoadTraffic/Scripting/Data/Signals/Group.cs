using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TransportX.Scripting.Data;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Scripting.Data.Signals
{
    public class Group : XmlSerializable
    {
        public XmlValue<string> Key = new(string.Empty);
        public XmlValue<SignalColor> Color = new(SignalColor.Off);

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Key), "キー", true);
            context.ReadAttribute<SignalColor>(nameof(Color), "信号色", true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Key), Key.Value);
            writer.WriteAttributeString(nameof(Color), Color.Value.ToString());
        }
    }
}
