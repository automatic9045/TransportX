using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.SDL;

namespace TransportX.Diagnostics
{
    public static class MessageBox
    {
        private static readonly Sdl Sdl = Sdl.GetApi();

        public static unsafe void Show(string message, string? title = null, MessageBoxFlags flags = MessageBoxFlags.None)
        {
            Sdl.ShowSimpleMessageBox((uint)flags, title, message, null);
        }
    }
}
