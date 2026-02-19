using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Data.Environment
{
    public class Bloom : XmlSerializable
    {
        public XmlValue<float> Threshold = new(2);
        public XmlValue<float> Intensity = new(0.2f);
        public XmlValue<float> Scatter = new(1);
        public XmlValue<float> SoftKnee = new(0.5f);
        public XmlValue<Color> Tint = new(Color.White);

        protected override void ReadXml(ReadContext context)
        {
            context.AddElement<float>(nameof(Threshold), "ブルーム閾値");
            context.AddElement<float>(nameof(Intensity), "ブルーム強度");
            context.AddElement<float>(nameof(Scatter), "ブルーム拡散範囲");
            context.AddElement<float>(nameof(SoftKnee), "ブルーム ソフト閾値");
            context.AddElement<string, Color>(nameof(Tint), ColorTranslator.FromHtml, "ブルーム色");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString(nameof(Threshold), Threshold.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(Intensity), Intensity.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(Scatter), Scatter.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(SoftKnee), SoftKnee.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(Tint), ColorTranslator.ToHtml(Tint.Value));
        }
    }
}
