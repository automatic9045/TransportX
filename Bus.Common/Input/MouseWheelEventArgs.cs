using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Input
{
    public class MouseWheelEventArgs : EventArgs
    {
        public int Delta { get; }

        public MouseWheelEventArgs(int delta)
        {
            Delta = delta;
        }
    }
}
