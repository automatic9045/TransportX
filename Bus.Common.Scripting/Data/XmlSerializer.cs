using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using Bus.Common.Diagnostics;

namespace Bus.Common.Scripting.Data
{
    public static class XmlSerializer<T>
    {
        private static readonly XmlSerializer Serializer = new(typeof(T));

        public static T FromXml(string filePath)
        {
            using XmlReader reader = XmlReader.Create(filePath);
            return (T)Serializer.Deserialize(reader)!;
        }

        public static T? FromXml(string filePath, IErrorCollector errorCollector)
        {
            try
            {
                return FromXml(filePath);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException is not XmlException xmlException) throw;

                Error error = xmlException.CreateError();
                errorCollector.Report(error);
                return default;
            }
        }

        public static void ToXml(T obj, string filePath)
        {
            using XmlWriter writer = XmlWriter.Create(filePath);
            Serializer.Serialize(writer, obj);
        }
    }
}
