using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting
{
    public static class BaseDirectory
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string? Find(int frameIndex = 2)
        {
            StackTrace stackTrace = new(true);
            StackFrame? frame = stackTrace.GetFrame(frameIndex);
            string? callerPath = frame?.GetFileName();
            return Path.GetDirectoryName(callerPath);
        }
    }
}
