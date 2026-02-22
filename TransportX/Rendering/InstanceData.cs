using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InstanceData
    {
        public static readonly int Size = Marshal.SizeOf<InstanceData>();


        public Matrix4x4 World;

        public InstanceData()
        {
        }
    }
}
