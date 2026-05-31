using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering.Pipelines
{
    public readonly struct ShadowCascade
    {
        public required Matrix4x4 LightView { get; init; }
        public required Matrix4x4 LightProjection { get; init; }
        public required Matrix4x4 LightViewProjection { get; init; }
        public required float SplitDepth { get; init; }
    }
}
