using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

using Bus.Common.Diagnostics;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scripting.Data.Networks
{
    public class Lane : XmlSerializable
    {
        public XmlValue<string?> Kind = new(null);
        public XmlValue<FlowDirections> Directions = new(FlowDirections.InOut);
        public XmlValue<float> X = new(0);
        public XmlValue<float> Y = new(0);

        public override void ReadXml(XmlReader reader)
        {
            Uri uri = new(reader.BaseURI);
            string location = uri.IsFile ? uri.LocalPath : reader.BaseURI;

            reader.MoveToContent();

            string? kindAttribute = reader.GetAttribute(nameof(Kind));
            if (kindAttribute is null)
            {
                ReportError("進路種別が定義されていません。");
            }
            else
            {
                Kind = CreateValue((string?)kindAttribute);
            }

            string? directionsAttribute = reader.GetAttribute(nameof(Directions));
            if (directionsAttribute is not null)
            {
                if (Enum.TryParse(directionsAttribute, out FlowDirections directions))
                {
                    Directions = CreateValue(directions);
                }
                else
                {
                    ReportError($"進行方向 '{directionsAttribute}' は無効です。");
                }
            }

            string? xAttribute = reader.GetAttribute(nameof(X));
            if (xAttribute is not null)
            {
                if (float.TryParse(xAttribute, CultureInfo.InvariantCulture, out float x))
                {
                    X = CreateValue(x);
                }
                else
                {
                    ReportError($"X 座標 '{xAttribute}' は無効です。");
                }
            }

            string? yAttribute = reader.GetAttribute(nameof(Y));
            if (yAttribute != null)
            {
                if (float.TryParse(yAttribute, CultureInfo.InvariantCulture, out float y))
                {
                    Y = CreateValue(y);
                }
                else
                {
                    ReportError($"Y 座標 '{yAttribute}' は無効です。");
                }
            }

            reader.Skip();


            XmlValue<T> CreateValue<T>(T value)
            {
                IXmlLineInfo lineInfo = (IXmlLineInfo)reader;
                return new XmlValue<T>(value, location, lineInfo.LineNumber, lineInfo.LinePosition);
            }

            void ReportError(string message)
            {
                IXmlLineInfo lineInfo = (IXmlLineInfo)reader;
                Error error = new(ErrorLevel.Error, message, location)
                {
                    LineNumber = lineInfo.LineNumber,
                    LinePosition = lineInfo.LinePosition,
                };
                ErrorsKey.Add(error);
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            if (Kind.Value is not null) writer.WriteAttributeString(nameof(Kind), Kind.Value);
            writer.WriteAttributeString(nameof(Directions), Directions.Value.ToString());
            writer.WriteAttributeString(nameof(X), X.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(nameof(Y), Y.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
