using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Multimedia;

namespace TransportX.Audio
{
    public interface ISoundAsset : IDisposable
    {
        public static ISoundAsset Empty = new Null();


        WaveFormat Format { get; }
        TimeSpan Duration { get; }
        bool IsMono { get; }

        ISound CreateSound(float maxFrequencyRatio);
        ISound3D CreateSound3D(float maxFrequencyRatio);


        private class Null : ISoundAsset
        {
            public WaveFormat Format { get; } = new(44100, 16, 1);
            public TimeSpan Duration => TimeSpan.Zero;
            public bool IsMono => true;

            public ISound CreateSound(float maxFrequencyRatio) => ISound.Empty;
            public ISound3D CreateSound3D(float maxFrequencyRatio) => ISound3D.Empty;

            public void Dispose()
            {
            }
        }
    }
}
