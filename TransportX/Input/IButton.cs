using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Input
{
    public interface IButton : IDisposable
    {
        public static IButton Empty(string key) => new EmptyButton(key);


        string Key { get; }
        bool IsPressed { get; }

        event ButtonEventHandler? Pressed;
        event ButtonEventHandler? Released;

        void Tick(TimeSpan elapsed);


        private class EmptyButton : IButton
        {
            public string Key { get; }
            public bool IsPressed => false;

            public event ButtonEventHandler? Pressed;
            public event ButtonEventHandler? Released;

            public EmptyButton(string key)
            {
                Key = key;
            }

            public void Dispose()
            {
            }

            public void Tick(TimeSpan elapsed)
            {
            }
        }
    }


    public delegate void ButtonEventHandler(IButton sender);
}
