using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Data
{
    public abstract partial class XmlSerializable : IXmlSerializable
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, FieldInfo>> FieldCache = [];
        private static readonly ConcurrentDictionary<(Type Type, string ElementName), XmlSerializer> SerializerCache = [];


        private readonly IReadOnlyDictionary<string, FieldInfo> Fields;

        protected bool PreserveFullElement { get; set; } = false;
        public XElement? FullElement { get; private set; } = null;

        protected readonly List<Error> ErrorsKey = [];
        public IReadOnlyList<Error> Errors => ErrorsKey;

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

            string location = string.IsNullOrEmpty(reader.BaseURI) ? string.Empty : reader.BaseURI;
            if (!string.IsNullOrEmpty(location))
            {
                if (Uri.TryCreate(location, UriKind.Absolute, out Uri? uri) && uri.IsFile)
                {
                    location = uri.LocalPath;
                }
            }

            if (PreserveFullElement)
            {
                XElement fullElement;
                using (XmlReader subReader = reader.ReadSubtree())
                {
                    subReader.MoveToContent();
                    fullElement = XElement.Load(subReader, LoadOptions.SetLineInfo);
                }

                reader.Read();

                using (XmlReader elementReader = fullElement.CreateReader())
                {
                    elementReader.MoveToContent();
                    ProcessReader(elementReader, location);
                }

                FullElement = fullElement;
            }
            else
            {
                ProcessReader(reader, location);
            }


            void ProcessReader(XmlReader currentReader, string location)
            {
                bool isEmpty = currentReader.IsEmptyElement;

                ReadContext context = new(this, currentReader, isEmpty, location);
                ReadXml(context);

                context.ParseElements();

                if (!isEmpty && currentReader.NodeType == XmlNodeType.EndElement)
                {
                    currentReader.ReadEndElement();
                }
            }
        }

        protected abstract void ReadXml(ReadContext context);
        public abstract void WriteXml(XmlWriter writer);

        protected static void WriteSerializedElement<T>(XmlWriter writer, string elementName, T value)
        {
            if (value is null) return;

            XmlSerializer serializer = SerializerCache.GetOrAdd((typeof(T), elementName),
                key => new XmlSerializer(key.Type, new XmlRootAttribute(key.ElementName)));

            XmlSerializerNamespaces emptyNamespaces = new();
            emptyNamespaces.Add(string.Empty, string.Empty);

            serializer.Serialize(writer, value, emptyNamespaces);
        }

        protected static void WriteSerializedListElements<T>(XmlWriter writer, string elementName, IReadOnlyList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                WriteSerializedElement(writer, elementName, list[i]);
            }
        }
    }
}
