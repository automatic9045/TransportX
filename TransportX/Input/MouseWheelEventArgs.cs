using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input
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
