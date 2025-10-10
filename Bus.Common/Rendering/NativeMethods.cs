using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Rendering
{
    internal static class NativeMethods
    {
        [DllImport("Bus.Assets.dll", CharSet = CharSet.Unicode)]
        public static extern int CreateWICTextureFromFile_(nint device, nint context, string fileName, out nint texture, out nint textureView, ulong maxSize = 0);

        [DllImport("Bus.Assets.dll")]
        public static extern int CreateWICTextureFromMemory_(nint device, nint context, byte[] wicData, int wicDataSize, out nint texture, out nint textureView, ulong maxSize = 0);
    }
}
