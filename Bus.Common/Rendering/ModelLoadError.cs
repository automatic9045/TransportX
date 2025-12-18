using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;

namespace Bus.Common.Rendering
{
    public class ModelLoadError : Error
    {
        public required ModelLoadErrorTypes Types { get; init; }

        public ModelLoadError() : base()
        {
        }

        [SetsRequiredMembers]
        public ModelLoadError(ModelLoadErrorTypes types, ErrorLevel level, string message, string location) : base(level, message, location)
        {
            Types = types;
        }

        public override Error ChangeSource(string location, int lineNumber = 0, int linePosition = 0, int length = 0)
        {
            return new ModelLoadError(Types, Level, Message, location)
            {
                Code = Code,
                LineNumber = lineNumber,
                LinePosition = linePosition,
                Length = Length,
                Exception = Exception,
            };
        }
    }
}
