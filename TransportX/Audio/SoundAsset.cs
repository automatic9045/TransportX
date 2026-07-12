using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Multimedia;
using Vortice.XAudio2;

namespace TransportX.Audio
{
    public class SoundAsset : ISoundAsset
    {
        private readonly IXAudio2 XAudio2;
        private readonly IXAudio2MasteringVoice MasteringVoice;
        private readonly X3DAudio X3DAudio;

        private readonly byte[] AudioBytes;
        private readonly uint[]? DecodedPacketsInfo;

        public WaveFormat Format { get; }
        public TimeSpan Duration { get; }
        public bool IsMono => Format.Channels == 1;

        public SoundAsset(IXAudio2 xaudio2, IXAudio2MasteringVoice masteringVoice, X3DAudio x3dAudio, SoundStream soundStream)
        {
            XAudio2 = xaudio2;
            MasteringVoice = masteringVoice;
            X3DAudio = x3dAudio;

            Format = soundStream.Format ?? throw new InvalidOperationException("音声データのフォーマットを取得できませんでした。");
            DecodedPacketsInfo = soundStream.DecodedPacketsInfo;

            int length = (int)soundStream.Length;
            byte[] buffer = new byte[length];
            soundStream.ReadExactly(buffer, 0, length);
            AudioBytes = buffer;

            double totalSeconds = (double)length / Format.AverageBytesPerSecond;
            Duration = TimeSpan.FromSeconds(totalSeconds);
        }

        public static SoundAsset Empty(IXAudio2 xaudio2, IXAudio2MasteringVoice masteringVoice, X3DAudio x3dAudio)
        {
            using SoundStream soundStream = new(Stream.Null);
            return new SoundAsset(xaudio2, masteringVoice, x3dAudio, soundStream);
        }

        public static SoundAsset FromFile(IXAudio2 xaudio2, IXAudio2MasteringVoice masteringVoice, X3DAudio x3dAudio, string filePath)
        {
            using FileStream fileStream = File.OpenRead(filePath);
            using SoundStream soundStream = new(fileStream);
            return new SoundAsset(xaudio2, masteringVoice, x3dAudio, soundStream);
        }

        public void Dispose()
        {
        }

        public Sound CreateSound(float maxFrequencyRatio)
        {
            IXAudio2SourceVoice sourceVoice = XAudio2.CreateSourceVoice(Format, maxFrequencyRatio: maxFrequencyRatio);
            return new Sound(AudioBytes, DecodedPacketsInfo, sourceVoice, maxFrequencyRatio);
        }

        ISound ISoundAsset.CreateSound(float maxFrequencyRatio) => CreateSound(maxFrequencyRatio);

        public Sound3D CreateSound3D(float maxFrequencyRatio)
        {
            if (!IsMono)
            {
                throw new NotSupportedException("3D サウンド (Sound3D) を生成するには、モノラル (1ch) の音声データである必要があります。");
            }

            IXAudio2SourceVoice sourceVoice = XAudio2.CreateSourceVoice(Format, maxFrequencyRatio: maxFrequencyRatio);
            return new Sound3D(MasteringVoice, X3DAudio, AudioBytes, DecodedPacketsInfo, sourceVoice, maxFrequencyRatio);
        }

        ISound3D ISoundAsset.CreateSound3D(float maxFrequencyRatio) => CreateSound3D(maxFrequencyRatio);
    }
}
