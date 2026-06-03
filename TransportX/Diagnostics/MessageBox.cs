using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Core.Native;
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

        public static unsafe bool Show(string message, ReadOnlySpan<Button> buttons, string? title, MessageBoxFlags flags, out Button result)
        {
            byte* pMessage = (byte*)SilkMarshal.StringToPtr(message, NativeStringEncoding.UTF8);
            byte* pTitle = (byte*)SilkMarshal.StringToPtr(title, NativeStringEncoding.UTF8);

            Span<MessageBoxButtonData> rawButtons = stackalloc MessageBoxButtonData[buttons.Length];
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                byte* pText = (byte*)SilkMarshal.StringToPtr(button.Text, NativeStringEncoding.UTF8);
                rawButtons[i] = new MessageBoxButtonData((uint)button.Flags, i + 1, pText);
            }

            try
            {
                fixed (MessageBoxButtonData* pButtons = rawButtons)
                {
                    MessageBoxData data = new((uint)flags, null, pTitle, pMessage, buttons.Length, pButtons);

                    int buttonId = 0;
                    Sdl.ShowMessageBox(ref data, ref buttonId);

                    if (0 < buttonId)
                    {
                        result = buttons[buttonId - 1];
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }
            }
            finally
            {
                SilkMarshal.FreeString((nint)pMessage, NativeStringEncoding.UTF8);
                SilkMarshal.FreeString((nint)pTitle, NativeStringEncoding.UTF8);

                for (int i = 0; i < rawButtons.Length; i++)
                {
                    SilkMarshal.FreeString((nint)rawButtons[i].Text, NativeStringEncoding.UTF8);
                }
            }
        }


        public class Button
        {
            public string Text { get; }
            public MessageBoxButtonFlags Flags { get; }

            public Button(string text, MessageBoxButtonFlags flags)
            {
                Text = text;
                Flags = flags;
            }
        }
    }
}
