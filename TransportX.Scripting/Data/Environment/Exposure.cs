using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransportX.Scripting.Data.Environment
{
    public class Exposure : XmlSerializable
    {
        public XmlValue<float> Key = new(0.118f);
        public XmlValue<float> Min = new(0.01f);
        public XmlValue<float> Max = new(5);
        public XmlValue<float> DarkAdaptationSpeed = new(0.5f);
        public XmlValue<float> LightAdaptationSpeed = new(1.5f);

        protected override void ReadXml(ReadContext context)
        {
            context.AddElement<float>(nameof(Key), "自動露出基準輝度");
            context.AddElement<float>(nameof(Min), "自動露出下限値");
            context.AddElement<float>(nameof(Max), "自動露出上限値");
            context.AddElement<float>(nameof(DarkAdaptationSpeed), "自動露出暗順応速度");
            context.AddElement<float>(nameof(LightAdaptationSpeed), "自動露出明順応速度");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString(nameof(Key), Key.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(Min), Min.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(Max), Max.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(DarkAdaptationSpeed), DarkAdaptationSpeed.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(LightAdaptationSpeed), LightAdaptationSpeed.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
