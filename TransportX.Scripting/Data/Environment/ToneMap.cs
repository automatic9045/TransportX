using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransportX.Scripting.Data.Environment
{
    public class ToneMap : XmlSerializable
    {
        public XmlValue<float> Contrast = new(1.6f);
        public XmlValue<float> Shoulder = new(0.977f);
        public XmlValue<float> MaxLuminance = new(12);
        public XmlValue<float> MidtoneScale = new(1.5f);

        protected override void ReadXml(ReadContext context)
        {
            context.AddElement<float>(nameof(Contrast), "トーンマップ コントラスト");
            context.AddElement<float>(nameof(Shoulder), "トーンマップ ショルダー");
            context.AddElement<float>(nameof(MaxLuminance), "トーンマップ最大輝度");
            context.AddElement<float>(nameof(MidtoneScale), "トーンマップ中間調スケール");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString(nameof(Contrast), Contrast.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(Shoulder), Shoulder.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(MaxLuminance), MaxLuminance.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(MidtoneScale), MidtoneScale.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
