using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Multimedia;
using Vortice.XAudio2;

using Bus.Common.Scenery;

namespace Bus.Common.Audio
{
    public class Sound3D : Sound
    {
        private readonly IXAudio2MasteringVoice MasteringVoice;

        public Emitter Emitter { get; }
        public DspSettings DspSettings { get; }

        public LocatableObject? AttachedTo { get; set; } = null;

        public Sound3D(IXAudio2MasteringVoice masteringVoice, SoundStream stream, IXAudio2SourceVoice sourceVoice) : base(stream, sourceVoice)
        {
            if (Stream.Format!.Channels != 1) throw new NotSupportedException($"{nameof(Sound3D)} はモノラルサウンド以外には対応していません。");

            MasteringVoice = masteringVoice;

            Emitter = new Emitter()
            {
                ChannelCount = 1,
                CurveDistanceScaler = 1,
                DopplerScaler = 1,
            };

            DspSettings = new DspSettings(1, MasteringVoice.VoiceDetails.InputChannels);
        }

        public static Sound3D FromFile(IXAudio2 xaudio2, IXAudio2MasteringVoice masteringVoice, string filePath)
        {
            SoundStream stream = new SoundStream(File.OpenRead(filePath));
            IXAudio2SourceVoice sourceVoice = xaudio2.CreateSourceVoice(stream.Format!, maxFrequencyRatio: 5);
            return new Sound3D(masteringVoice, stream, sourceVoice);
        }

        public void Update(X3DAudio x3dAudio, Listener listener, int cameraX, int cameraZ)
        {
            if (AttachedTo is not null)
            {
                Emitter.OrientFront = AttachedTo.Direction;
                Emitter.OrientTop = AttachedTo.Up;
                Emitter.Position = AttachedTo.Position + new PlateOffset(AttachedTo.PlateX - cameraX, AttachedTo.PlateZ - cameraZ).Position;
                //Emitter.Velocity = AttachedTo.Velocity;
            }

            x3dAudio.Calculate(listener, Emitter, CalculateFlags.Matrix | CalculateFlags.Doppler | CalculateFlags.LpfDirect | CalculateFlags.Reverb, DspSettings);

            SourceVoice.SetOutputMatrix(MasteringVoice, 1, MasteringVoice.VoiceDetails.InputChannels, DspSettings.MatrixCoefficients);
            SourceVoice.SetFrequencyRatio((float)Pitch * DspSettings.DopplerFactor, XAudio2.CommitNow);
            SourceVoice.SetVolume((float)Volume);

            SourceVoice.SetOutputMatrix(1, 1, [DspSettings.ReverbLevel]);

            FilterParameters filterParameters = new FilterParameters()
            {
                Type = FilterType.LowPassFilter,
                Frequency = 2 * float.Sin(float.Pi / 6 * DspSettings.LpfDirectCoefficient),
                OneOverQ = 1,
            };
            SourceVoice.SetFilterParameters(filterParameters, XAudio2.CommitNow);
        }

        public override void SetVolume(double volume)
        {
            Volume = volume;
        }

        public override void SetPitch(double pitch)
        {
            Pitch = pitch;
        }
    }
}
