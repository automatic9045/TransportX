using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

using TransportX.Dependency;

namespace TransportX
{
    public interface IRuntime : IDisposable
    {
        PluginLoadContext Context { get; }

        void Draw();

        void OnKeyDown(Key key) { }
        void OnKeyUp(Key key) { }
        void OnMouseDragMove(Vector2 offset, MouseButton button) { }
        void OnMouseWheel(int delta) { }
    }
}
