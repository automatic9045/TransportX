using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransportX.Scripting.Data.Rendering
{
    public class CrossSectionVertex : XmlSerializable
    {
        public XmlValue<string> Key = new(null!);

        public XmlValue<float> X = new(0);
        public XmlValue<float> Y = new(0);

        public XmlValue<float> NormalX = new(0);
        public XmlValue<float> NormalY = new(1);

        public XmlValue<float> U = new(0);
        public XmlValue<Color> Color = new(System.Drawing.Color.White);

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Key), "頂点キー", true);

            context.ReadAttribute<float>(nameof(X), "X 座標", true);
            context.ReadAttribute<float>(nameof(Y), "Y 座標", true);

            context.ReadAttribute<float>(nameof(NormalX), "法線 X");
            context.ReadAttribute<float>(nameof(NormalY), "法線 Y");

            context.ReadAttribute<float>(nameof(U), "テクスチャ U 座標");
            context.ReadAttribute<string, Color>(nameof(Color), ColorTranslator.FromHtml, "頂点カラー");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Key), Key.Value);
            writer.WriteAttributeString(nameof(X), X.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(Y), Y.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(NormalX), NormalX.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(NormalY), NormalY.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(U), U.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(Color), ColorTranslator.ToHtml(Color.Value));
        }
    }
}
