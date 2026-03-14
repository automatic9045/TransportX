using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Multimedia;
using Vortice.XAudio2;

using TransportX.Spatial;

namespace TransportX.Audio
{
    public class Sound3D : Sound
    {
        private readonly IXAudio2MasteringVoice MasteringVoice;
        private readonly X3DAudio X3DAudio;

        public Emitter Emitter { get; }
        public DspSettings DspSettings { get; }

        public ILocatable? AttachedTo { get; set; } = null;

        public Sound3D(IXAudio2MasteringVoice masteringVoice, X3DAudio x3dAudio, SoundStream stream, IXAudio2SourceVoice sourceVoice) : base(stream, sourceVoice)
        {
            if (Stream.Format!.Channels != 1) throw new NotSupportedException($"{nameof(Sound3D)} はモノラルサウンド以外には対応していません。");

            MasteringVoice = masteringVoice;
            X3DAudio = x3dAudio;

            Emitter = new Emitter()
            {
                ChannelCount = 1,
                CurveDistanceScaler = 1,
                DopplerScaler = 1,
                VolumeCurve = [
                    new CurvePoint() { Distance = 0, DspSetting = 1 },
                    new CurvePoint() { Distance = 0.1f, DspSetting = 0.3f },
                    new CurvePoint() { Distance = 0.5f, DspSetting = 0.1f },
                    new CurvePoint() { Distance = 1, DspSetting = 0 },
                ],
            };

            DspSettings = new DspSettings(1, MasteringVoice.VoiceDetails.InputChannels);
        }

        public static Sound3D FromFile(IXAudio2 xaudio2, IXAudio2MasteringVoice masteringVoice, X3DAudio x3dAudio, string filePath)
        {
            SoundStream stream = new SoundStream(File.OpenRead(filePath));
            IXAudio2SourceVoice sourceVoice = xaudio2.CreateSourceVoice(stream.Format!, maxFrequencyRatio: 5);
            return new Sound3D(masteringVoice, x3dAudio, stream, sourceVoice);
        }

        public void Update(Listener listener, int cameraX, int cameraZ)
        {
            if (AttachedTo is not null)
            {
                Emitter.OrientFront = AttachedTo.Pose.Direction;
                Emitter.OrientTop = AttachedTo.Pose.Up;
                Emitter.Position = AttachedTo.Pose.Position + new PlateOffset(AttachedTo.PlateX - cameraX, AttachedTo.PlateZ - cameraZ).Position;
                Emitter.Velocity = AttachedTo.Velocity;
            }

            X3DAudio.Calculate(listener, Emitter, CalculateFlags.Matrix | CalculateFlags.Doppler | CalculateFlags.LpfDirect | CalculateFlags.Reverb, DspSettings);

            SourceVoice.SetOutputMatrix(MasteringVoice, 1, MasteringVoice.VoiceDetails.InputChannels, DspSettings.MatrixCoefficients);
            SourceVoice.SetFrequencyRatio(Pitch * DspSettings.DopplerFactor, XAudio2.CommitNow);
            SourceVoice.SetVolume(Volume);

            SourceVoice.SetOutputMatrix(1, 1, [DspSettings.ReverbLevel]);

            FilterParameters filterParameters = new FilterParameters()
            {
                Type = FilterType.LowPassFilter,
                Frequency = 2 * float.Sin(float.Pi / 6 * DspSettings.LpfDirectCoefficient),
                OneOverQ = 1,
            };
            SourceVoice.SetFilterParameters(filterParameters, XAudio2.CommitNow);
        }

        public override void SetVolume(float volume)
        {
            Volume = volume;
        }

        public override void SetPitch(float pitch)
        {
            Pitch = pitch;
        }
    }
}
