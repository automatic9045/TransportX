using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Vortice.XAudio2;

namespace TransportX.Audio
{
    public class SoundSource : ISoundSource
    {
        protected readonly float MaxFrequencyRatio;

        public IXAudio2SourceVoice SourceVoice { get; }
        public ISoundAsset? Asset { get; private set; } = null;

        public bool IsPlaying => 0 < SourceVoice.StateNoSamplesPlayed.BuffersQueued;

        public virtual float Volume
        {
            get => field;
            set
            {
                field = float.Max(0, value);
                SourceVoice.SetVolume(field);
            }
        } = 1;

        public virtual float Pitch
        {
            get => field;
            set
            {
                field = float.Clamp(value, XAudio2.MinimumFrequencyRatio, MaxFrequencyRatio);
                SourceVoice.SetFrequencyRatio(field, XAudio2.CommitNow);
            }
        } = 1;

        public SoundSource(IXAudio2SourceVoice sourceVoice, float maxFrequencyRatio)
        {
            SourceVoice = sourceVoice;
            MaxFrequencyRatio = maxFrequencyRatio;
        }

        public void Dispose()
        {
            SourceVoice.Stop();
            SourceVoice.Dispose();
        }

        public virtual void Play(ISoundAsset asset, bool loop)
        {
            if (IsPlaying) Stop();

            if (SourceVoice.VoiceDetails.InputChannels != asset.Format.Channels)
            {
                throw new InvalidOperationException(
                    $"ボイスのチャンネル数 ({SourceVoice.VoiceDetails.InputChannels}ch) とアセットのチャンネル数 ({asset.Format.Channels}ch) が一致しません。");
            }

            Asset = asset;
            Asset.Buffer.LoopCount = (uint)(loop ? XAudio2.LoopInfinite : XAudio2.NoLoopRegion);

            uint targetRate = (uint)asset.Format.SampleRate;
            if (SourceVoice.VoiceDetails.InputSampleRate != targetRate)
            {
                while (0 < SourceVoice.StateNoSamplesPlayed.BuffersQueued)
                {
                    Thread.Yield();
                }

                SourceVoice.SetSourceSampleRate(targetRate);
            }

            SourceVoice.SubmitSourceBuffer(asset.Buffer, asset.DecodedPacketsInfo);
            SourceVoice.Start();
        }

        public void Stop()
        {
            SourceVoice.Stop();
            SourceVoice.FlushSourceBuffers();
            Asset = null;
        }
    }
}
