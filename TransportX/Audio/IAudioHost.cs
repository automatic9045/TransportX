using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.XAudio2;

namespace TransportX.Audio
{
    public interface IAudioHost : IDisposable
    {
        IXAudio2 XAudio2 { get; }
        IXAudio2MasteringVoice MasteringVoice { get; }
        X3DAudio X3DAudio { get; }
    }
}
