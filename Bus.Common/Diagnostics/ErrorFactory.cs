using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bus.Common.Diagnostics
{
    public static class ErrorFactory
    {
        public static Error CreateError(this XmlException exception, ErrorLevel errorLevel = ErrorLevel.Error, string? errorCode = null)
        {
            return new Error()
            {
                Level = errorLevel,
                Message = exception.Message,
                Location = exception.SourceUri ?? string.Empty,
                Code = errorCode,
                LineNumber = exception.LineNumber,
                LinePosition = exception.LinePosition,
                Exception = exception,
            };
        }
    }
}
