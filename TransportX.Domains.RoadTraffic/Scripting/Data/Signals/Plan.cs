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
    public class Plan : XmlSerializable
    {
        public XmlValue<string> Key = new(string.Empty);
        public XmlValue<TimeSpan> Offset = new(TimeSpan.Zero);

        public List<Step> Steps = [];

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Key), "キー", true);
            context.ReadAttribute<double, TimeSpan>(nameof(Offset), TimeSpan.FromSeconds, "オフセット");

            context.AddSerializedListElement<Step>(nameof(Step), nameof(Steps), "ステップリスト", true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Key), Key.Value);
            writer.WriteAttributeString(nameof(Offset), Offset.Value.Seconds.ToString(CultureInfo.InvariantCulture));

            WriteSerializedListElements(writer, nameof(Step), Steps);
        }
    }
}
