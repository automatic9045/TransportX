using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Collections
{
    public class DrainableMax<T> where T : struct, IComparable<T>
    {
        public bool IsReported { get; private set; } = false;
        public T Value { get; private set; } = default;

        public DrainableMax()
        {
        }

        public void Report(T value)
        {
            if (!IsReported || 0 < value.CompareTo(Value))
            {
                Value = value;
                IsReported = true;
            }
        }

        public T Consume(T defaultValue = default)
        {
            if (!IsReported) return defaultValue;

            T result = Value;
            Value = default;
            IsReported = false;
            return result;
        }
    }
}
