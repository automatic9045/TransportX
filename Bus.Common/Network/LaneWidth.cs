using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Network
{
    public readonly struct LaneWidth
    {
        public float Left { get; }
        public float Right { get; }
        public float Total => Left + Right;

        public LaneWidth(float left, float right)
        {
            Left = left;
            Right = right;
        }

        public static LaneWidth Constant(float width) => new(width * 0.5f, width * 0.5f);

        public static LaneWidth Opposition(LaneWidth a) => new LaneWidth(a.Right, a.Left);

        public static LaneWidth Lerp(LaneWidth a, LaneWidth b, float t)
        {
            return new LaneWidth(
                float.Lerp(a.Left, b.Left, t),
                float.Lerp(a.Right, b.Right, t)
            );
        }
    }
}
