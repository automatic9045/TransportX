using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.XAudio2;

namespace TransportX.Audio
{
    public class AudioHost : IAudioHost
    {
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
    }
}
