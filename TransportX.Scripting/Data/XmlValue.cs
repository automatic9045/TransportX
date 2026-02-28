using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Data
{
    public readonly struct XmlValue<T>
    {
        public readonly T Value { get; }

        public readonly string? Location { get; }
        public readonly int LineNumber { get; }
        public readonly int LinePosition { get; }

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

        public XmlValue() : this(default!)
        {
            if (default(T) is null) throw new InvalidOperationException();
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

        public override string? ToString()
        {
            return Value?.ToString();
        }
    }
}
