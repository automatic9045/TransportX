using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Audio
{
    public interface ISoundSource : IDisposable
    {
        public static readonly ISoundSource Empty = new Null();


        ISoundAsset? Asset { get; }
        float Volume { get; set; }
        float Pitch { get; set; }
        bool IsPlaying { get; }

        void Play(ISoundAsset asset, bool loop);
        void Stop();


        internal class Null : ISoundSource
        {
            public ISoundAsset? Asset => null;
            public float Volume { get; set; } = 0;
            public float Pitch { get; set; } = 1;
            public bool IsPlaying => false;

            public void Dispose()
            {
            }

            public void Play(ISoundAsset asset, bool loop)
            {
            }

            public void Stop()
            {
            }
        }
    }
}
