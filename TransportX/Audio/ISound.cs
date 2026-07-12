using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Audio
{
    public interface ISound : IDisposable
    {
        public static readonly ISound Empty = new Null();


        float Volume { get; set; }
        float Pitch { get; set; }
        bool IsPlaying { get; }

        void Play(bool loop);
        void Stop();


        internal class Null : ISound
        {
            public float Volume { get; set; } = 0;
            public float Pitch { get; set; } = 1;
            public bool IsPlaying => false;

            public void Dispose()
            {
            }

            public void Play(bool loop)
            {
            }

            public void Stop()
            {
            }
        }
    }
}
