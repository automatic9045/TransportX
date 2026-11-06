using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Multimedia;
using Vortice.XAudio2;

namespace TransportX.Audio
{
    public class AudioHost : IAudioHost
    {
        private const int DefaultSampleRate = 48000;


        public IXAudio2 XAudio2 { get; }
        public IXAudio2MasteringVoice MasteringVoice { get; }
        public X3DAudio X3DAudio { get; }

        public AudioHost()
        {
            XAudio2 = Vortice.XAudio2.XAudio2.XAudio2Create();
            MasteringVoice = XAudio2.CreateMasteringVoice();
            X3DAudio = new X3DAudio(MasteringVoice.ChannelMask);
        }

        public void Dispose()
        {
            MasteringVoice.Dispose();
            XAudio2.Dispose();
        }

        public ISoundSource CreateSoundSource(float maxFrequencyRatio, AudioChannelType channelType = AudioChannelType.Stereo)
        {
            WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(DefaultSampleRate, (int)channelType);
            IXAudio2SourceVoice sourceVoice = XAudio2.CreateSourceVoice(format, maxFrequencyRatio: maxFrequencyRatio);
            return new SoundSource(sourceVoice, maxFrequencyRatio);
        }

        public ISoundSource3D CreateSoundSource3D(float maxFrequencyRatio)
        {
            WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(DefaultSampleRate, 1);
            IXAudio2SourceVoice sourceVoice = XAudio2.CreateSourceVoice(format, maxFrequencyRatio: maxFrequencyRatio);
            return new SoundSource3D(MasteringVoice, X3DAudio, sourceVoice, maxFrequencyRatio);
        }
    }
}
