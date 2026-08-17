using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;

namespace TransportX.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InstanceData
    {
        private static readonly InputElementDescription[] InputElementsKey = [
            new InputElementDescription("WORLD", 0, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),
            new InputElementDescription("WORLD", 1, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 1, InputClassification.PerInstanceData, 1),
            new InputElementDescription("WORLD", 2, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 1, InputClassification.PerInstanceData, 1),
            new InputElementDescription("WORLD", 3, Format.R32G32B32A32_Float, InputElementDescription.AppendAligned, 1, InputClassification.PerInstanceData, 1),
        ];
        public static ReadOnlySpan<InputElementDescription> InputElements => InputElementsKey;

        public static readonly int Size = Marshal.SizeOf<InstanceData>();


        public Matrix4x4 World;

        public InstanceData()
        {
        }
    }
}
