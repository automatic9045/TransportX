using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TransportX.Network;

namespace TransportX.Scripting.Data.Network
{
    public class Lane : XmlSerializable
    {
        public XmlValue<string?> AllowedTraffic = new(null);
        public XmlValue<FlowDirections> Directions = new(FlowDirections.InOut);
        public XmlValue<float> X = new(0);
        public XmlValue<float> Y = new(0);
        public XmlValue<float> LeftWidth = new(2);
        public XmlValue<float> RightWidth = new(2);

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(AllowedTraffic), "進路種別", true);
            context.ReadAttribute<FlowDirections>(nameof(Directions), "進行方向");
            context.ReadAttribute<float>(nameof(X), "X 座標");
            context.ReadAttribute<float>(nameof(Y), "Y 座標");
            context.ReadAttribute<float>(nameof(LeftWidth), "左側の幅");
            context.ReadAttribute<float>(nameof(RightWidth), "右側の幅");
        }

        public override void WriteXml(XmlWriter writer)
        {
            if (AllowedTraffic.Value is not null) writer.WriteAttributeString(nameof(AllowedTraffic), AllowedTraffic.Value);
            writer.WriteAttributeString(nameof(Directions), Directions.Value.ToString());
            writer.WriteAttributeString(nameof(X), X.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(Y), Y.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(LeftWidth), LeftWidth.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(RightWidth), RightWidth.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
