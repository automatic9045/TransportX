using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Data.Rendering
{
    public class Material : XmlSerializable
    {
        public XmlValue<string> Name = new(null!);

        public XmlValue<Color> BaseColor = new(Color.White);
        public XmlValue<float> Metallic = new(0);
        public XmlValue<float> Roughness = new(1);
        public XmlValue<Color> Emissive = new(Color.White);
        public XmlValue<float> EmissiveStrength = new(1);

        public TextureSlot BaseColorTexture = TextureSlot.Empty;
        public TextureSlot NormalTexture = TextureSlot.Empty;
        public TextureSlot OcclusionTexture = TextureSlot.Empty;
        public TextureSlot RoughnessTexture = TextureSlot.Empty;
        public TextureSlot MetallicTexture = TextureSlot.Empty;
        public TextureSlot EmissiveTexture = TextureSlot.Empty;

        [XmlIgnore]
        public IEnumerable<Error> AllErrors =>
        [
            .. Errors,
            .. BaseColorTexture.Errors,
            .. NormalTexture.Errors,
            .. OcclusionTexture.Errors,
            .. RoughnessTexture.Errors,
            .. MetallicTexture.Errors,
            .. EmissiveTexture.Errors,
        ];

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Name), "材質名", true);

            context.ReadAttribute<string, Color>(nameof(BaseColor), ColorTranslator.FromHtml, "基本色");
            context.ReadAttribute<float>(nameof(Metallic), "金属感");
            context.ReadAttribute<float>(nameof(Roughness), "粗さ");
            context.ReadAttribute<string, Color>(nameof(Emissive), ColorTranslator.FromHtml, "発光");
            context.ReadAttribute<float>(nameof(EmissiveStrength), "発光強度");

            context.AddSerializedElement<TextureSlot>(nameof(BaseColorTexture), "基本色テクスチャ");
            context.AddSerializedElement<TextureSlot>(nameof(NormalTexture), "法線テクスチャ");
            context.AddSerializedElement<TextureSlot>(nameof(OcclusionTexture), "遮蔽テクスチャ");
            context.AddSerializedElement<TextureSlot>(nameof(RoughnessTexture), "粗さテクスチャ");
            context.AddSerializedElement<TextureSlot>(nameof(MetallicTexture), "金属感テクスチャ");
            context.AddSerializedElement<TextureSlot>(nameof(EmissiveTexture), "発光テクスチャ");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Name), Name.Value);

            writer.WriteAttributeString(nameof(BaseColor), ColorTranslator.ToHtml(BaseColor.Value));
            writer.WriteAttributeString(nameof(Metallic), Metallic.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(Roughness), Roughness.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(Emissive), ColorTranslator.ToHtml(Emissive.Value));
            writer.WriteAttributeString(nameof(EmissiveStrength), EmissiveStrength.Value.ToString(CultureInfo.InvariantCulture));

            if (BaseColorTexture != TextureSlot.Empty) WriteSerializedElement(writer, nameof(BaseColorTexture), BaseColorTexture);
            if (NormalTexture != TextureSlot.Empty) WriteSerializedElement(writer, nameof(NormalTexture), NormalTexture);
            if (OcclusionTexture != TextureSlot.Empty) WriteSerializedElement(writer, nameof(OcclusionTexture), OcclusionTexture);
            if (RoughnessTexture != TextureSlot.Empty) WriteSerializedElement(writer, nameof(RoughnessTexture), RoughnessTexture);
            if (MetallicTexture != TextureSlot.Empty) WriteSerializedElement(writer, nameof(MetallicTexture), MetallicTexture);
            if (EmissiveTexture != TextureSlot.Empty) WriteSerializedElement(writer, nameof(EmissiveTexture), EmissiveTexture);
        }
    }
}
