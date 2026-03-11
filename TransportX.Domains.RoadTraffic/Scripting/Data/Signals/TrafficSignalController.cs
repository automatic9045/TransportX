using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TransportX.Scripting.Data;

namespace TransportX.Domains.RoadTraffic.Scripting.Data.Signals
{
    public class TrafficSignalController : XmlSerializable
    {
        public List<PlanRef> Schedule = [];
        public List<Plan> Plans = [];

        protected override void ReadXml(ReadContext context)
        {
            context.AddSerializedElement<List<PlanRef>>(nameof(Schedule), "スケジュール", true);
            context.AddSerializedElement<List<Plan>>(nameof(Plans), "プランリスト", true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            WriteSerializedElement(writer, nameof(Schedule), Schedule);
            WriteSerializedElement(writer, nameof(Plans), Plans);
        }
    }
}
