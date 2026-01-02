using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Bus.Common.Diagnostics;

namespace Bus.Common.Scripting.Data
{
    public abstract class XmlSerializable : IXmlSerializable
    {
        protected readonly List<Error> ErrorsKey = [];
        public IReadOnlyList<Error> Errors => ErrorsKey;

        public XmlSchema? GetSchema() => null;
        public abstract void ReadXml(XmlReader reader);
        public abstract void WriteXml(XmlWriter writer);
    }
}
