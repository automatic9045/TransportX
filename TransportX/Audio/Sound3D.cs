using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.XAudio2;

using TransportX.Spatial;

namespace TransportX.Audio
{
    public class Sound3D : Sound, ISound3D
    {
        private readonly IXAudio2MasteringVoice MasteringVoice;
        private readonly X3DAudio X3DAudio;

        public Emitter Emitter { get; }
        public DspSettings DspSettings { get; }

        public WorldPose WorldPose { get; set; }
        public Vector3 Velocity { get; set; }
        public IWorldObject? AttachedTo { get; set; } = null;

        public event MovedEventHandler? Moved
        {
            add => throw new NotSupportedException();
            remove => throw new NotSupportedException();
        }

        public Sound3D(IXAudio2MasteringVoice masteringVoice, X3DAudio x3dAudio,
            byte[] audioBytes, uint[]? decodedPacketsInfo, IXAudio2SourceVoice sourceVoice, float maxFrequencyRatio)
            : base(audioBytes, decodedPacketsInfo, sourceVoice, maxFrequencyRatio)
        {
            MasteringVoice = masteringVoice;
            X3DAudio = x3dAudio;

            Emitter = new Emitter()
            {
                ChannelCount = 1,
                CurveDistanceScaler = 50,
                DopplerScaler = 1,
                InnerRadius = 1,
                InnerRadiusAngle = 0,
                VolumeCurve = [
                    new CurvePoint() { Distance = 0, DspSetting = 1 },
                    new CurvePoint() { Distance = 0.1f, DspSetting = 0.3f },
                    new CurvePoint() { Distance = 0.5f, DspSetting = 0.1f },
                    new CurvePoint() { Distance = 1, DspSetting = 0 },
                ],
            };

            DspSettings = new DspSettings(1, MasteringVoice.VoiceDetails.InputChannels);
        }

        public void Update(Listener listener, ChunkIndex cameraChunk)
        {
            if (AttachedTo is not null)
            {
                WorldPose = AttachedTo.WorldPose;
                Velocity = AttachedTo.Velocity;
            }

            Emitter.OrientFront = WorldPose.Pose.Direction;
            Emitter.OrientTop = WorldPose.Pose.Up;

            Emitter.Position = WorldPose.Pose.Position + (WorldPose.Chunk - cameraChunk).Position;
            Emitter.Velocity = Velocity;

            X3DAudio.Calculate(listener, Emitter, CalculateFlags.Matrix | CalculateFlags.Doppler | CalculateFlags.LpfDirect | CalculateFlags.Reverb, DspSettings);

            SourceVoice.SetOutputMatrix(MasteringVoice, 1, MasteringVoice.VoiceDetails.InputChannels, DspSettings.MatrixCoefficients);
            SourceVoice.SetFrequencyRatio(float.Clamp(Pitch * DspSettings.DopplerFactor, XAudio2.MinimumFrequencyRatio, MaxFrequencyRatio), XAudio2.CommitNow);
            SourceVoice.SetVolume(Volume);
            //SourceVoice.SetOutputMatrix(1, 1, [DspSettings.ReverbLevel]);

            FilterParameters filterParameters = new()
            {
                Type = FilterType.LowPassFilter,
                Frequency = 2 * float.Sin(float.Pi / 6 * DspSettings.LpfDirectCoefficient),
                OneOverQ = 1,
            };
            SourceVoice.SetFilterParameters(filterParameters, XAudio2.CommitNow);
        }
    }
}
