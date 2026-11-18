using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Data.Rendering
{
    public class CrossSectionSurface : XmlSerializable
    {
        public XmlValue<float> VInterval = new(1);
        public XmlValue<float> VOffset = new(0);
        public XmlValue<string> Material = new(null!);
        public XmlValue<string?> ColliderMaterial = new(null);

        public List<CrossSectionVertexReference> Vertices = [];

        [XmlIgnore]
        public IEnumerable<Error> AllErrors => [
            .. Errors,
            .. Vertices.SelectMany(v => v.Errors),
        ];

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<float>(nameof(VInterval), "テクスチャ V 間隔");
            context.ReadAttribute<float>(nameof(VOffset), "テクスチャ V オフセット");
            context.ReadAttribute<string>(nameof(Material), "材質", true);
            context.ReadAttribute<string?>(nameof(ColliderMaterial), "物理材質");
            context.AddSerializedWrappedListElement<CrossSectionVertexReference>(nameof(Vertices), "VertexRef", nameof(Vertices), "頂点リスト");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(VInterval), VInterval.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(VOffset), VOffset.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(Material), Material.Value);
            if (ColliderMaterial.Value is not null) writer.WriteAttributeString(nameof(ColliderMaterial), ColliderMaterial.Value);
            WriteSerializedWrappedListElements(writer, nameof(Vertices), "VertexRef", Vertices);
        }
    }
}
