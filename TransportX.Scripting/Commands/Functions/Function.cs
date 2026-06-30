using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Commands.Functions
{
    public class Function
    {
        public FunctionSignature Signature { get; }
        public string Name { get; }
        public IReadOnlyList<object> Args { get; }

        public Function(FunctionSignature signature, string name, IReadOnlyList<object> args)
        {
            Signature = signature;
            Name = name;
            Args = args;
        }

        public override string ToString() => $"{Name}({string.Join(", ", Args)})";
    }
}
