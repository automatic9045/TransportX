using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Network
{
    [Flags]
    public enum FlowDirections
    {
        In = 1,
        Out = 2,
        InOut = 3,
    }


    public static class FlowDirectionsExtensions
    {
        public static FlowDirections GetOpposition(this FlowDirections source)
        {
            return source switch
            {
                FlowDirections.In => FlowDirections.Out,
                FlowDirections.Out => FlowDirections.In,
                FlowDirections.InOut => FlowDirections.InOut,
                _ => throw new ArgumentOutOfRangeException(nameof(source)),
            };
        }

        public static bool IsOppositeOf(this FlowDirections a, FlowDirections b)
        {
            return a.GetOpposition() == b;
        }
    }
}
