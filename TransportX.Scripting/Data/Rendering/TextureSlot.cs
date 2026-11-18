using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TransportX.Rendering;

namespace TransportX.Scripting.Data.Rendering
{
    public class TextureSlot : XmlSerializable
    {
        public static readonly TextureSlot Empty = new();


        public XmlValue<string?> Path = new(null);
        public XmlValue<TextureSampleMode> SampleMode = new(TextureSampleMode.Texture);

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string?>(nameof(Path), "テクスチャ", true);
            context.ReadAttribute<TextureSampleMode>(nameof(SampleMode), "テクスチャサンプルモード");
        }

        public override void WriteXml(XmlWriter writer)
        {
            if (Path.Value is not null) writer.WriteAttributeString(nameof(Path), Path.Value);
            writer.WriteAttributeString(nameof(SampleMode), SampleMode.Value.ToString());
        }
    }
}
