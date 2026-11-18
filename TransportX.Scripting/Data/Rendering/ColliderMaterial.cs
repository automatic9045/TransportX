using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransportX.Scripting.Data.Rendering
{
    public class ColliderMaterial : XmlSerializable
    {
        public XmlValue<string> Name = new(null!);

        public XmlValue<float> FrictionCoefficient = new(1);
        public XmlValue<float> MaxRecoveryVelocity = new(float.MaxValue);

        public XmlValue<float> SpringFrequency = new(30);
        public XmlValue<float> SpringDampingRatio = new(1);

        protected override void ReadXml(ReadContext context)
        {
            context.ReadAttribute<string>(nameof(Name), "物理材質名", true);

            context.ReadAttribute<float>(nameof(FrictionCoefficient), "摩擦係数");
            context.ReadAttribute<float>(nameof(MaxRecoveryVelocity), "最大復元速度");

            context.ReadAttribute<float>(nameof(SpringFrequency), "ばね周波数");
            context.ReadAttribute<float>(nameof(SpringDampingRatio), "ばね減衰比");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Name), Name.Value);

            writer.WriteAttributeString(nameof(FrictionCoefficient), FrictionCoefficient.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(MaxRecoveryVelocity), MaxRecoveryVelocity.Value.ToString(CultureInfo.InvariantCulture));

            writer.WriteAttributeString(nameof(SpringFrequency), SpringFrequency.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(SpringDampingRatio), SpringDampingRatio.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
