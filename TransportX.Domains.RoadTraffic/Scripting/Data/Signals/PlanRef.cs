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
    public class PlanRef : XmlSerializable
    {
        public XmlValue<TimeSpan> StartTime = new(TimeSpan.Zero);
        public XmlValue<string> Key = new(string.Empty);

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string, TimeSpan>(nameof(StartTime), x => TimeSpan.Parse(x, CultureInfo.InvariantCulture), "開始時刻", true);
            context.ReadAttribute<string>(nameof(Key), "プランキー", true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(StartTime), StartTime.Value.ToString("c", CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(Key), Key.Value);
        }
    }
}
