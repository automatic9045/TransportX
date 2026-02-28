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

namespace TransportX.Domains.RoadTraffic.Commands
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
            IModel model = GetModel("Model") ?? Model.Empty();
            IModel blinkerLModel = GetModel("BlinkerLModel") ?? Model.Empty();
            IModel blinkerRModel = GetModel("BlinkerRModel") ?? Model.Empty();

            CarFactory factory = new(model, blinkerLModel, blinkerRModel);
            return factory;


            IModel? GetModel(string attributeName)
            {
                XAttribute? attribute = data.Attribute(attributeName);
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

            void ReportError(string message, IXmlLineInfo lineInfo)
            {
                Error error = new(ErrorLevel.Error, message, data.BaseUri)
                {
                    LineNumber = lineInfo.LineNumber,
                    LinePosition = lineInfo.LinePosition,
                };
                World.ErrorCollector.Report(error);
            }
        }
    }
}
