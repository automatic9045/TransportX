using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TransportX.Scripting.Data;

namespace TransportX.Scripting.Components.Data
{
    public class Agent : XmlSerializable
    {
        public XmlValue<string> Key = new(null!);

        public Agent()
        {
            PreserveFullElement = true;
        }

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Key), "エージェント", true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Key), Key.Value);
        }
    }
}
