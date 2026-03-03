using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using TransportX.Diagnostics;
using TransportX.Rendering;

using TransportX.Extensions.Traffic;

using TransportX.Scripting;
using TransportX.Scripting.Components;

using TransportX.Domains.RoadTraffic.Traffic;

namespace TransportX.Domains.RoadTraffic.Scripting
{
    public class CarTemplate : ITrafficAgentTemplate, IWorldInstantiable<CarTemplate>
    {
        private readonly ScriptWorld World;

        public CarTemplate(ScriptWorld world)
        {
            World = world;
        }

        public static CarTemplate Create(ScriptWorld world) => new(world);

        public IParticipantFactory Build(XElement data)
        {
            XElement modelsElement = data.Element("Models") ?? new XElement(string.Empty);
            IModel model = GetModel(modelsElement, "Body") ?? Model.Empty();
            IModel blinkerLModel = GetModel(modelsElement, "BlinkerL") ?? Model.Empty();
            IModel blinkerRModel = GetModel(modelsElement, "BlinkerR") ?? Model.Empty();

            XElement? specElement = data.Element("Spec");
            float minAcceleration = (float?)specElement?.Attribute("MinAcceleration") ?? 7;
            float maxAcceleration = (float?)specElement?.Attribute("MaxAcceleration") ?? 15;
            float minDeceleration = (float?)specElement?.Attribute("MinDeceleration") ?? 20;
            float maxDeceleration = (float?)specElement?.Attribute("MaxDeceleration") ?? 60;
            float maxSpeed = (float?)specElement?.Attribute("MaxSpeed") ?? 120;
            float maxReverseSpeed = (float?)specElement?.Attribute("MaxReverseSpeed") ?? 10;

            CarSpec spec = new()
            {
                MinAcceleration = float.Min(minAcceleration, maxAcceleration) / 3.6f,
                MaxAcceleration = maxAcceleration / 3.6f,

                MinDeceleration = float.Min(minDeceleration, maxDeceleration) / 3.6f,
                MaxDeceleration = minDeceleration / 3.6f,

                MaxSpeed = maxSpeed / 3.6f,
                MaxReverseSpeed = maxReverseSpeed / 3.6f,
            };

            CarFactory factory = new(model, blinkerLModel, blinkerRModel, spec);
            return factory;


            IModel? GetModel(XElement element, string attributeName)
            {
                XAttribute? attribute = element.Attribute(attributeName);
                if (attribute is null)
                {
                    ReportError($"{nameof(attributeName)} 属性が定義されていません。", data);
                    return null;
                }

                string key = (string)attribute;
                if (!World.Models.TryGetValue(key, out IModel? model))
                {
                    ReportError($"モデル '{key}' が見つかりません。", attribute);
                    return null;
                }

                return model;
            }

            void ReportError(string message, XObject obj)
            {
                Error error = new(ErrorLevel.Error, message, obj.BaseUri)
                {
                    LineNumber = ((IXmlLineInfo)obj).LineNumber,
                    LinePosition = ((IXmlLineInfo)obj).LinePosition,
                };
                World.ErrorCollector.Report(error);
            }
        }
    }
}
