using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransportX.Scripting.Data.Scripts
{
    public class Dependency : XmlSerializable
    {
        public XmlValue<string> Path = new(null!);

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Path), "依存関係のパス", true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Path), Path.Value);
        }
    }
}
