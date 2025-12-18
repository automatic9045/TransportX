using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Diagnostics
{
    public class Error
    {
        public required ErrorLevel Level { get; init; }
        public required string Message { get; init; }
        public required string? Location { get; init; }
        public string? Code { get; init; } = null;
        public int LineNumber { get; init; } = 0;
        public int LinePosition { get; init; } = 0;
        public int Length { get; init; } = 0;
        public Exception? Exception { get; init; } = null;
        public StackTrace StackTrace { get; protected init; } = new StackTrace(true);

        public Error()
        {
        }

        [SetsRequiredMembers]
        public Error(ErrorLevel level, string message, string? location)
        {
            Level = level;
            Message = message;
            Location = location;
        }

        public override string ToString()
        {
            return $"{Location}({LineNumber},{LinePosition}): {Level} {Code ?? string.Empty}: {Message}";
        }

        public virtual Error ChangeSource(string location, int lineNumber = 0, int linePosition = 0, int length = 0)
        {
            return new Error(Level, Message, location)
            {
                Code = Code,
                LineNumber = lineNumber,
                LinePosition = linePosition,
                Length = Length,
                Exception = Exception,
                StackTrace = StackTrace,
            };
        }
    }
}
