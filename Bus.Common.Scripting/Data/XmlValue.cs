using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;

namespace Bus.Common.Scripting.Data
{
    public class XmlValue<T>
    {
        public T Value { get; }

        public string? Location { get; }
        public int LineNumber { get; }
        public int LinePosition { get; }

        public XmlValue(T value, string? location, int lineNumber, int linePosition)
        {
            Value = value;
            Location = location;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        public XmlValue(T value) : this(value, null, 0, 0)
        {
        }

        public Error CreateError(string message, ErrorLevel level = ErrorLevel.Error, Exception? exception = null)
        {
            return new Error(level, message, Location)
            {
                LineNumber = LineNumber,
                LinePosition = LinePosition,
                Exception = exception,
            };
        }
    }
}
