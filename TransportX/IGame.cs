using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using TransportX.Dependency;

namespace TransportX
{
    public interface IGame : IDisposable
    {
        PluginLoadContext Context { get; }

        void Draw(System.Drawing.Size clientSize);

        void OnKeyDown(Key key) { }
        void OnKeyUp(Key key) { }
        void OnMouseDragMove(Vector offset, MouseButtonState leftButton, MouseButtonState middleButton, MouseButtonState rightButton) { }
        void OnMouseWheel(int delta) { }
    }
}
