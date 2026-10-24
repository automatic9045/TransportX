using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Rendering
{
    public interface IModel : IDisposable
    {
        BoundingBox BoundingBox { get; }
        public string? DebugName { get; set; }

        void Draw(in DrawContext context);
    }
}
