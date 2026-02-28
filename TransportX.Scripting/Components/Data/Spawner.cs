using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TransportX.Scripting.Data;

namespace TransportX.Scripting.Components.Data
{
    public class Spawner : XmlSerializable
    {
        public XmlValue<string> Key = new(null!);
        public List<Agent> Agents = [];

        public Spawner()
        {
            PreserveFullElement = true;
        }

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Key), "エージェント発生ルール", true);
            context.AddSerializedElement<List<Agent>>(nameof(Agents), "エージェントリスト");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Key), Key.Value);
            WriteSerializedElement(writer, nameof(Agents), Agents);
        }
    }
}
