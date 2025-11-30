using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bus.Common.Input
{
    public class MouseDragMoveEventArgs : EventArgs
    {
        public Vector Offset { get; }
        public MouseButtonState LeftButton { get; }
        public MouseButtonState MiddleButton { get; }
        public MouseButtonState RightButton { get; }

        public MouseDragMoveEventArgs(Vector offset, MouseButtonState leftButton, MouseButtonState middleButton, MouseButtonState rightButton)
        {
            Offset = offset;
            LeftButton = leftButton;
            MiddleButton = middleButton;
            RightButton = rightButton;
        }
    }
}
