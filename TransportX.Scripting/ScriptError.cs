using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Scripting
{
    public class ScriptError : Error
    {
        public static string? DefaultLocation { get; set; } = null;

        [SetsRequiredMembers]
        public ScriptError(ErrorLevel level, string message, StackTrace? stackTrace = null, string? defaultLocation = null) : base()
        {
            Level = level;
            Message = message;
            StackTrace = stackTrace ?? StackTrace;

            StackFrame[] stackFrames = StackTrace.GetFrames();

            string? location = defaultLocation ?? DefaultLocation;
            int lineNumber = 0, linePosition = 0;
            foreach (StackFrame frame in stackFrames)
            {
                string? filePath = frame.GetFileName();
                if (filePath is null) continue;
                if (!File.Exists(filePath)) continue;
#if DEBUG
                if (Path.GetExtension(filePath) == ".cs") continue;
#endif

                int line = frame.GetFileLineNumber();
                if (line == 0) continue;

                int column = frame.GetFileColumnNumber();
                if (column == 0) continue;

                location = filePath;
                lineNumber = line;
                linePosition = column;
                break;
            }

            Location = location;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        private static StackTrace? GetStackTrace(Exception exception, StackTrace? stackTrace, bool useExceptionStackTrace)
        {
            return stackTrace ?? (useExceptionStackTrace ? new StackTrace(exception, true) : null);
        }

        [SetsRequiredMembers]
        public ScriptError(ErrorLevel level, Exception exception,
            string? message = null, StackTrace? stackTrace = null, bool useExceptionStackTrace = false, string? defaultLocation = null)
            : this(level, message ?? exception.Message, GetStackTrace(exception, stackTrace, useExceptionStackTrace), defaultLocation)
        {
            Exception = exception;
        }

        public static ScriptError CreateFrom(Error source)
        {
            ScriptError error = new(source.Level, source.Message, source.StackTrace)
            {
                Code = source.Code,
                Exception = source.Exception,
            };
            return error;
        }
    }
}
