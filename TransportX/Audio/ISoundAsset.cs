using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Multimedia;
using Vortice.XAudio2;

namespace TransportX.Audio
{
    public interface ISoundAsset : IDisposable
    {
        public static ISoundAsset Empty = new Null();


        WaveFormat Format { get; }
        TimeSpan Duration { get; }
        AudioBuffer Buffer { get; }
        uint[]? DecodedPacketsInfo { get; }


        private class Null : ISoundAsset
        {
            public WaveFormat Format { get; } = new(44100, 16, 1);
            public TimeSpan Duration => TimeSpan.Zero;
            public AudioBuffer Buffer { get; } = new();
            public uint[]? DecodedPacketsInfo { get; } = null;

            public void Dispose()
            {
                Buffer.Dispose();
            }
        }
    }
}
