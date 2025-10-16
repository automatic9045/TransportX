using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Vortice.Direct3D11;

namespace Bus.Common
{
    public interface IGame : IDisposable
    {
        void Draw(ID3D11RenderTargetView renderTarget, ID3D11DepthStencilView depthStencil, System.Drawing.Size clientSize);

        void OnKeyDown(Key key) { }
        void OnKeyUp(Key key) { }
        void OnMouseDragMove(Vector offset) { }
        void OnMouseWheel(int delta) { }
    }
}
