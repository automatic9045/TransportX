using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace Bus.Common.Rendering
{
    public interface IDebugVisualizable : IDisposable
    {
        IDebugModel? DebugModel { get; }

        void CreateDebugModel(ID3D11Device device);
        void DrawDebug(LocatedDrawContext context);
    }
}
