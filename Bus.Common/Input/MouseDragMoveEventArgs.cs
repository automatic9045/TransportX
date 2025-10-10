using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bus.Common.Input
{
    public class MouseDragMoveEventArgs : EventArgs
    {
        public Vector Offset { get; }

        public MouseDragMoveEventArgs(Vector offset)
        {
            Offset = offset;
        }
    }
}
