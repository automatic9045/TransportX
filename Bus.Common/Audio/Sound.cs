using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Multimedia;
using Vortice.XAudio2;

namespace Bus.Common.Audio
{
    public class Sound : IDisposable
    {
        public SoundStream Stream { get; }
        public IXAudio2SourceVoice SourceVoice { get; }
        public AudioBuffer Buffer { get; }
        public bool IsPlaying => 0 < SourceVoice.StateNoSamplesPlayed.BuffersQueued;

        public float Volume { get; protected set; } = 1;
        public float Pitch { get; protected set; } = 1;

        public Sound(SoundStream stream, IXAudio2SourceVoice sourceVoice)
        {
            Stream = stream;
            SourceVoice = sourceVoice;

            Buffer = new AudioBuffer(Stream)
            {
                AudioBytes = (uint)Stream.Length,
                LoopCount = XAudio2.NoLoopRegion,
                LoopBegin = 0,
                LoopLength = 0,
                PlayBegin = 0,
                PlayLength = 0,
                Flags = BufferFlags.EndOfStream,
            };
        }

        public static Sound FromFile(IXAudio2 xaudio2, string filePath)
        {
            SoundStream stream = new SoundStream(File.OpenRead(filePath));
            IXAudio2SourceVoice sourceVoice = xaudio2.CreateSourceVoice(stream.Format!, maxFrequencyRatio: 5);
            return new Sound(stream, sourceVoice);
        }

        public void Dispose()
        {
            SourceVoice.Stop();
            Buffer.Dispose();
            SourceVoice.Dispose();
            Stream.Dispose();
        }

        public virtual void SetVolume(float volume)
        {
            Volume = volume;
            SourceVoice.SetVolume(volume);
        }

        public virtual void SetPitch(float pitch)
        {
            Pitch = pitch;
            SourceVoice.SetFrequencyRatio(pitch, XAudio2.CommitNow);
        }

        public virtual void Play(bool loop)
        {
            Buffer.LoopCount = (uint)(loop ? XAudio2.LoopInfinite : XAudio2.NoLoopRegion);
            SourceVoice.SubmitSourceBuffer(Buffer, Stream.DecodedPacketsInfo);
            SourceVoice.Start();
        }

        public void Stop()
        {
            SourceVoice.Stop();
            SourceVoice.FlushSourceBuffers();
        }
    }
}
