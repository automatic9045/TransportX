using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Rendering
{
    public class ModelLoadError : Error
    {
        public required ErrorSource Source { get; init; }
        public required ErrorTarget Target { get; init; }

        public ModelLoadError() : base()
        {
        }

        [SetsRequiredMembers]
        public ModelLoadError(ErrorSource source, ErrorTarget target, ErrorLevel level, string message, string? location) : base(level, message, location)
        {
            Source = source;
            Target = target;
        }

        [SetsRequiredMembers]
        public ModelLoadError(Error baseError, ErrorSource source, ErrorTarget target) : this(source, target, baseError.Level, baseError.Message, baseError.Location)
        {
            Code = baseError.Code;
            LineNumber = baseError.LineNumber;
            LinePosition = baseError.LinePosition;
            Length = baseError.Length;
            Exception = baseError.Exception;
            StackTrace = baseError.StackTrace;
        }

        public override Error ChangeSource(string location, int lineNumber = 0, int linePosition = 0, int length = 0)
        {
            return new ModelLoadError(Source, Target, Level, Message, location)
            {
                Code = Code,
                LineNumber = lineNumber,
                LinePosition = linePosition,
                Length = Length,
                Exception = Exception,
                StackTrace = StackTrace,
            };
        }


        public enum ErrorSource
        {
            Reference,
            Data,
        }

        public enum ErrorTarget
        {
            Visual,
            Collision,
        }
    }
}
