using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vortice.XAudio2;

using Bus.Common;
using Bus.Common.Audio;

namespace Bus.Sample
{
    internal class SoundFactory
    {
        private static readonly string BaseDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"Sound");


        private readonly IXAudio2 XAudio2;
        private readonly IXAudio2MasteringVoice MasteringVoice;

        public SoundFactory(IXAudio2 xaudio2, IXAudio2MasteringVoice masteringVoice)
        {
            XAudio2 = xaudio2;
            MasteringVoice = masteringVoice;
        }

        public Sound FromFile(string relativePath)
        {
            string path = Path.Combine(BaseDirectory, relativePath);
            Sound sound = Sound.FromFile(XAudio2, path);
            return sound;
        }

        public Sound3D FromFile3D(string relativePath, LocatableObject? attachTo = null)
        {
            string path = Path.Combine(BaseDirectory, relativePath);
            Sound3D sound = Sound3D.FromFile(XAudio2, MasteringVoice, path);
            sound.AttachedTo = attachTo;
            return sound;
        }
    }
}
