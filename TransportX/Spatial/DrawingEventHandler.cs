using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Rendering;

namespace TransportX.Spatial
{
    public delegate void DrawingEventHandler(TransformedModel sender, in TransformedDrawContext context, in Matrix4x4 world);
}
