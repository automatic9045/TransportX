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
            IModel blinkerLightLModel = GetModel(modelsElement, "BlinkerLightL") ?? Model.Empty();
            IModel blinkerLightRModel = GetModel(modelsElement, "BlinkerLightR") ?? Model.Empty();
            IModel brakeLightModel = GetModel(modelsElement, "BrakeLight") ?? Model.Empty();

            XElement? specElement = data.Element("Spec");
            float minThrottle = (float?)specElement?.Attribute("MinThrottle") ?? 7;
            float maxThrottle = (float?)specElement?.Attribute("MaxThrottle") ?? 15;
            float minBrake = (float?)specElement?.Attribute("MinBrake") ?? 5;
            float maxBrake = (float?)specElement?.Attribute("MaxBrake") ?? 12;
            float emergencyBrake = (float?)specElement?.Attribute("EmergencyBrake") ?? 35;
            float minBrakeJerk = (float?)specElement?.Attribute("MinBrakeJerk") ?? 3;
            float maxBrakeJerk = (float?)specElement?.Attribute("MaxBrakeJerk") ?? 8;
            float emergencyBrakeJerk = (float?)specElement?.Attribute("EmergencyBrakeJerk") ?? 150;
            float maxSpeed = (float?)specElement?.Attribute("MaxSpeed") ?? 120;
            float maxReverseSpeed = (float?)specElement?.Attribute("MaxReverseSpeed") ?? 10;

            float brakeLightDecelerationThreshold = (float?)specElement?.Attribute("BrakeLightDecelerationThreshold") ?? 1;

            CarSpec spec = new()
            {
                MinThrottle = float.Min(minThrottle, maxThrottle) / 3.6f,
                MaxThrottle = maxThrottle / 3.6f,

                MinBrake = float.Min(minBrake, maxBrake) / 3.6f,
                MaxBrake = maxBrake / 3.6f,
                EmergencyBrake = emergencyBrake / 3.6f,

                MinBrakeJerk = float.Min(minBrakeJerk, maxBrakeJerk) / 3.6f,
                MaxBrakeJerk = maxBrakeJerk / 3.6f,
                EmergencyBrakeJerk = emergencyBrakeJerk / 3.6f,

                MaxSpeed = maxSpeed / 3.6f,
                MaxReverseSpeed = maxReverseSpeed / 3.6f,

                BrakeLightDecelerationThreshold = brakeLightDecelerationThreshold / 3.6f,
            };

            CarFactory factory = new(model, blinkerLightLModel, blinkerLightRModel, brakeLightModel, spec);
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
