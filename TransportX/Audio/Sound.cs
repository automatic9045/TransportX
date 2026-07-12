using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.XAudio2;

namespace TransportX.Audio
{
    public class Sound : ISound
    {
        protected readonly uint[]? DecodedPacketsInfo;
        protected readonly float MaxFrequencyRatio;

        public IXAudio2SourceVoice SourceVoice { get; }
        public AudioBuffer Buffer { get; }

        public bool IsPlaying => 0 < SourceVoice.StateNoSamplesPlayed.BuffersQueued;

        public virtual float Volume
        {
            get => field;
            set
            {
                field = float.Max(0, value);
                SourceVoice.SetVolume(field);
            }
        }

        public virtual float Pitch
        {
            get => field;
            set
            {
                if (value <= 0) return;
                field = value;
                SourceVoice.SetFrequencyRatio(field, XAudio2.CommitNow);
            }
        }

        public Sound(byte[] audioBytes, uint[]? decodedPacketsInfo, IXAudio2SourceVoice sourceVoice, float maxFrequencyRatio)
        {
            DecodedPacketsInfo = decodedPacketsInfo;
            MaxFrequencyRatio = maxFrequencyRatio;
            SourceVoice = sourceVoice;

            Buffer = new AudioBuffer(audioBytes)
            {
                AudioBytes = (uint)audioBytes.Length,
                LoopCount = XAudio2.NoLoopRegion,
                Flags = BufferFlags.EndOfStream,
            };
        }

        public void Dispose()
        {
            SourceVoice.Stop();
            Buffer.Dispose();
            SourceVoice.Dispose();
        }

        public virtual void Play(bool loop)
        {
            if (IsPlaying)
            {
                Stop();
            }

            Buffer.LoopCount = (uint)(loop ? XAudio2.LoopInfinite : XAudio2.NoLoopRegion);
            SourceVoice.SubmitSourceBuffer(Buffer, DecodedPacketsInfo);
            SourceVoice.Start();
        }

        public void Stop()
        {
            SourceVoice.Stop();
            SourceVoice.FlushSourceBuffers();
        }
    }
}
