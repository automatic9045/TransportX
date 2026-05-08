using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

namespace TransportX.Input
{
    public delegate void KeyEventHandler(IKeyboard keyboard);

    public delegate void MouseScrollEventHandler(IMouse mouse, ScrollWheel delta);
    public delegate void MouseMoveEventHandler(IMouse mouse, Vector2 delta);
}
