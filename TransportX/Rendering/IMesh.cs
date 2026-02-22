using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Rendering
{
    public interface IMesh : IDisposable
    {
        BoundingBox BoundingBox { get; }
        Material Material { get; }
        string? DebugName { get; set; }

        void Draw(in DrawContext context);
    }
}
