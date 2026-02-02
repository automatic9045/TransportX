using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using Vortice.Direct3D11;

using Bus.Common.Rendering;

namespace Bus.Common.Rendering
{
    public interface IDebugVisualizable : IDisposable
    {
        bool CanDrawDebug { get; }
        Vector4 DebugColor { get; set; }

        void CreateDebugResources(ID3D11Device device);
        void DrawDebug(DrawContext context);
    }
}
