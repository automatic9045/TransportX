using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransportX.Scripting.Data.Rendering
{
    public class CrossSectionVertexReference : XmlSerializable
    {
        public XmlValue<string> Key = new(null!);

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Key), "頂点キー", true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Key), Key.Value);
        }
    }
}
