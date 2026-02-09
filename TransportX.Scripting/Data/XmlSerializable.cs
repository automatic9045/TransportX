using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Microsoft.CodeAnalysis;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Data
{
    public abstract partial class XmlSerializable : IXmlSerializable
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, FieldInfo>> FieldCache = [];


        protected readonly List<Error> ErrorsKey = [];
        public IReadOnlyList<Error> Errors => ErrorsKey;

        private readonly IReadOnlyDictionary<string, FieldInfo> Fields;

        public XmlSerializable()
        {
            Fields = FieldCache.GetOrAdd(GetType(), type =>
            {
                return type.GetFields()
                    .Where(field => field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(XmlValue<>))
                    .ToDictionary(field => field.Name);
            });
        }

        public XmlSchema? GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            bool isEmpty = reader.IsEmptyElement;

            Uri uri = new(reader.BaseURI);
            string location = uri.IsFile ? uri.LocalPath : reader.BaseURI;

            ReadContext context = new(this, reader, isEmpty, location);
            ReadXml(context);

            context.ParseElements();
            if (!isEmpty && reader.NodeType == XmlNodeType.EndElement)
            {
                reader.ReadEndElement();
            }
        }

        protected abstract void ReadXml(ReadContext context);
        public abstract void WriteXml(XmlWriter writer);
    }
}
