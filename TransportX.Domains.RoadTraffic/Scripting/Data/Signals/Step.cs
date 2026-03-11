using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TransportX.Scripting.Data;

namespace TransportX.Domains.RoadTraffic.Scripting.Data.Signals
{
    public class Step : XmlSerializable
    {
        public XmlValue<TimeSpan> Duration = new(TimeSpan.Zero);

        public List<Group> Groups = [];

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<double, TimeSpan>(nameof(Duration), TimeSpan.FromSeconds, "継続時間", true);

            context.AddSerializedListElement<Group>(nameof(Group), nameof(Groups), "グループリスト", true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Duration), Duration.Value.Seconds.ToString(CultureInfo.InvariantCulture));
            WriteSerializedListElements(writer, nameof(Group), Groups);
        }
    }
}
