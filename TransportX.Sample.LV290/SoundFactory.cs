using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vortice.XAudio2;

using TransportX;
using TransportX.Audio;

namespace TransportX.Sample.LV290
{
    internal class SoundFactory
    {
        private static readonly string BaseDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"Sound");


        private readonly IXAudio2 XAudio2;
        private readonly IXAudio2MasteringVoice MasteringVoice;
        private readonly X3DAudio X3DAudio;
        private readonly ILocatable Body;

        private readonly ConcurrentDictionary<SixDoF, AttachableObject> Locations = new();

        public SoundFactory(IXAudio2 xaudio2, IXAudio2MasteringVoice masteringVoice, X3DAudio x3dAudio, ILocatable body)
        {
            XAudio2 = xaudio2;
            MasteringVoice = masteringVoice;
            X3DAudio = x3dAudio;
            Body = body;
        }

        public Sound FromFile(string relativePath)
        {
            string path = Path.Combine(BaseDirectory, relativePath);
            Sound sound = Sound.FromFile(XAudio2, path);
            return sound;
        }

        public Sound3D FromFile3D(string relativePath, SixDoF offset)
        {
            string path = Path.Combine(BaseDirectory, relativePath);
            Sound3D sound = Sound3D.FromFile(XAudio2, MasteringVoice, X3DAudio, path);
            sound.AttachedTo = Locations.GetOrAdd(offset, new AttachableObject(Body, offset));
            return sound;
        }
    }
}
