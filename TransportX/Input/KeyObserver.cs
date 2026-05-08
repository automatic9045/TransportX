using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

namespace TransportX.Input
{
    public class KeyObserver : IDisposable
    {
        public Key Key { get; }

        public bool IsPressed { get; private set; }

        internal event EventHandler? Disposing;

        public event EventHandler? Pressed;
        public event EventHandler? Released;

        public KeyObserver(Key key)
        {
            Key = key;
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);
        }

        internal void Press(IKeyboard keyboard)
        {
            IsPressed = true;
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        internal void Release(IKeyboard keyboard)
        {
            IsPressed = false;
            Released?.Invoke(this, EventArgs.Empty);
        }
    }
}
