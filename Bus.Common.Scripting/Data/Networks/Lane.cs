using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

using Bus.Common.Diagnostics;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scripting.Data.Networks
{
    public class Lane : XmlSerializable
    {
        public XmlValue<string?> Kind = new(null);
        public XmlValue<FlowDirections> Directions = new(FlowDirections.InOut);
        public XmlValue<float> X = new(0);
        public XmlValue<float> Y = new(0);

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Kind), "進路種別", true);
            context.ReadAttribute<FlowDirections>(nameof(Directions), "進行方向");
            context.ReadAttribute<float>(nameof(X), "X 座標");
            context.ReadAttribute<float>(nameof(Y), "Y 座標");
        }

        public override void WriteXml(XmlWriter writer)
        {
            if (Kind.Value is not null) writer.WriteAttributeString(nameof(Kind), Kind.Value);
            writer.WriteAttributeString(nameof(Directions), Directions.Value.ToString());
            writer.WriteAttributeString(nameof(X), X.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(Y), Y.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
