using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransportX.Scripting.Data.Environment
{
    public class EnvironmentProfile : XmlSerializable
    {
        public XmlValue<string?> DiffuseTexturePath = new(null);
        public XmlValue<string?> SpecularTexturePath = new(null);
        public XmlValue<float> Intensity = new(1);
        public XmlValue<float> Saturation = new(1);

        protected override void ReadXml(ReadContext context)
        {
            context.AddElement<string>(nameof(DiffuseTexturePath), "拡散光マップファイル");
            context.AddElement<string>(nameof(SpecularTexturePath), "反射光マップファイル");
            context.AddElement<float>(nameof(Intensity), "強度");
            context.AddElement<float>(nameof(Saturation), "彩度");
        }

        public override void WriteXml(XmlWriter writer)
        {
            if (DiffuseTexturePath.Value is not null) writer.WriteElementString(nameof(DiffuseTexturePath), DiffuseTexturePath.Value);
            if (SpecularTexturePath.Value is not null) writer.WriteElementString(nameof(SpecularTexturePath), SpecularTexturePath.Value);
            writer.WriteElementString(nameof(Intensity), Intensity.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(nameof(Saturation), Saturation.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
