using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Commands.Functions
{
    public class FunctionSignature
    {
        public string Name { get; }
        public IReadOnlyList<Type> ArgTypes { get; }

        public FunctionSignature(string name, IReadOnlyList<Type> argTypes)
        {
            Name = name.ToLowerInvariant();
            ArgTypes = argTypes;
        }

        public FunctionSignature(string name, params Type[] argTypes) : this(name, (IReadOnlyList<Type>)argTypes)
        {
        }

        public static FunctionSignature Create(string name)
        {
            return new FunctionSignature(name);
        }

        public static FunctionSignature Create<T>(string name)
        {
            return new FunctionSignature(name, typeof(T));
        }

        public static FunctionSignature Create<T1, T2>(string name)
        {
            return new FunctionSignature(name, typeof(T1), typeof(T2));
        }

        public static FunctionSignature Create<T1, T2, T3>(string name)
        {
            return new FunctionSignature(name, typeof(T1), typeof(T2), typeof(T3));
        }

        public static FunctionSignature Create<T1, T2, T3, T4>(string name)
        {
            return new FunctionSignature(name, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public static FunctionSignature Create<T1, T2, T3, T4, T5>(string name)
        {
            return new FunctionSignature(name, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public static FunctionSignature Create<T1, T2, T3, T4, T5, T6>(string name)
        {
            return new FunctionSignature(name, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public static FunctionSignature Create<T1, T2, T3, T4, T5, T6, T7>(string name)
        {
            return new FunctionSignature(name, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }

        public static FunctionSignature Create<T1, T2, T3, T4, T5, T6, T7, T8>(string name)
        {
            return new FunctionSignature(name, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }

        public static FunctionSignature Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name)
        {
            return new FunctionSignature(name, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        }

        public static FunctionSignature Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string name)
        {
            return new FunctionSignature(name, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        }

        public override string ToString() => $"{Name}({string.Join(", ", ArgTypes)})";
    }
}
