using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;
using TransportX.Communication;

namespace TransportX.Scripting.Avatars.Commands
{
    public class SoundSourceCommand<TSelf> where TSelf : SoundSourceCommand<TSelf>
    {
        protected readonly ScriptAvatar Avatar;

        private readonly List<Action> Triggers = [];
        
        private Func<TimeSpan, float> VolumeFactory = _ => 1;
        private Func<TimeSpan, float> PitchFactory = _ => 1;

        private float TriggerVolume = 1;
        private float TriggerPitch = 1;

        public string Key { get; }
        public ISoundSource Source { get; }

        public SoundSourceCommand(ScriptAvatar avatar, string key, ISoundSource source)
        {
            Avatar = avatar;

            Key = key;
            Source = source;
        }

        internal void Dispose()
        {
            Source.Dispose();
        }

        public void Play(ISoundAsset asset, float volume = 1, float pitch = 1, bool loop = false)
        {
            TriggerVolume = volume;
            TriggerPitch = pitch;

            Source.Volume = VolumeFactory(TimeSpan.Zero) * TriggerVolume;
            Source.Pitch = PitchFactory(TimeSpan.Zero) * TriggerPitch;
            Source.Play(asset, loop);
        }

        public void Play(string assetKey, float volume = 1, float pitch = 1, bool loop = false)
        {
            if (Avatar.SoundsKey.GetSound(assetKey, out ISoundAsset asset))
            {
                Play(asset, volume, pitch, loop);
            }
        }

        public void Stop()
        {
            Source.Stop();
            TriggerVolume = 1;
            TriggerPitch = 1;
        }

        public TSelf PlayWhen(ISoundAsset asset, Func<int> countFactory, float volume = 1, float pitch = 1, bool loop = false)
        {
            int lastCount = countFactory();
            Triggers.Add(() =>
            {
                int count = countFactory();
                if (count != lastCount)
                {
                    lastCount = count;
                    Play(asset, volume, pitch, loop);
                }
            });
            return (TSelf)this;
        }

        public TSelf PlayWhen(string assetKey, Func<int> countFactory, float volume = 1, float pitch = 1, bool loop = false)
            => PlayWhen(Avatar.Sounds.GetSound(assetKey), countFactory, volume, pitch, loop);
        public TSelf PlayWhen(ISoundAsset asset, Signal<int> countSignal, float volume = 1, float pitch = 1, bool loop = false)
            => PlayWhen(asset, () => countSignal.Value, volume, pitch, loop);
        public TSelf PlayWhen(string assetKey, Signal<int> countSignal, float volume = 1, float pitch = 1, bool loop = false)
            => PlayWhen(assetKey, () => countSignal.Value, volume, pitch, loop);
        public TSelf PlayWhen(string assetKey, string countIntSignalKey, float volume = 1, float pitch = 1, bool loop = false)
            => PlayWhen(assetKey, Avatar.Commander.Signals.Int(countIntSignalKey), volume, pitch, loop);

        public TSelf PlayStopWhen(ISoundAsset asset, Func<int> playCountFactory, Func<int> stopCountFactory, float volume = 1, float pitch = 1, bool loop = false)
        {
            int lastPlayCount = playCountFactory();
            int lastStopCount = stopCountFactory();
            Triggers.Add(() =>
            {
                int playCount = playCountFactory();
                if (playCount != lastPlayCount)
                {
                    lastPlayCount = playCount;
                    Play(asset, volume, pitch, loop);
                }

                int stopCount = stopCountFactory();
                if (stopCount != lastStopCount)
                {
                    lastStopCount = stopCount;
                    Stop();
                }
            });
            return (TSelf)this;
        }

        public TSelf PlayStopWhen(string assetKey, Func<int> playCountFactory, Func<int> stopCountFactory, float volume = 1, float pitch = 1, bool loop = false)
            => PlayStopWhen(Avatar.Sounds.GetSound(assetKey), playCountFactory, stopCountFactory, volume, pitch, loop);
        public TSelf PlayStopWhen(ISoundAsset asset, Signal<int> playCountSignal, Signal<int> stopCountSignal, float volume = 1, float pitch = 1, bool loop = false)
            => PlayStopWhen(asset, () => playCountSignal.Value, () => stopCountSignal.Value, volume, pitch, loop);
        public TSelf PlayStopWhen(string assetKey, Signal<int> playCountSignal, Signal<int> stopCountSignal, float volume = 1, float pitch = 1, bool loop = false)
            => PlayStopWhen(assetKey, () => playCountSignal.Value, () => stopCountSignal.Value, volume, pitch, loop);
        public TSelf PlayStopWhen(string assetKey, string playCountIntSignalKey, string stopCountIntSignalKey, float volume = 1, float pitch = 1, bool loop = false)
            => PlayStopWhen(assetKey, Avatar.Commander.Signals.Int(playCountIntSignalKey), Avatar.Commander.Signals.Int(stopCountIntSignalKey), volume, pitch, loop);

        public TSelf Loop(ISoundAsset asset)
        {
            Play(asset, loop: true);
            return (TSelf)this;
        }

        public TSelf Loop(string assetKey) => Loop(Avatar.Sounds.GetSound(assetKey));

        public TSelf Volume(Func<TimeSpan, float> factory)
        {
            VolumeFactory = factory;
            return (TSelf)this;
        }

        public TSelf Volume(double volume)
        {
            float floatVolume = (float)volume;
            return Volume(_ => floatVolume);
        }

        public TSelf Volume(Func<TimeSpan, double> factory)
            => Volume(elapsed => (float)factory(elapsed));
        public TSelf Volume(Signal<float> signal, float multiplier)
            => Volume(_ => signal.Value * multiplier);
        public TSelf Volume(string floatSignalKey, float multiplier)
            => Volume(Avatar.Commander.Signals.Float(floatSignalKey), multiplier);

        public TSelf Pitch(Func<TimeSpan, float> factory)
        {
            PitchFactory = factory;
            return (TSelf)this;
        }

        public TSelf Pitch(double pitch)
        {
            float floatPitch = (float)pitch;
            return Pitch(_ => floatPitch);
        }

        public TSelf Pitch(Func<TimeSpan, double> factory)
            => Pitch(elapsed => (float)factory(elapsed));
        public TSelf Pitch(Signal<float> signal, float multiplier)
            => Pitch(_ => signal.Value * multiplier);
        public TSelf Pitch(string floatSignalKey, float multiplier)
            => Pitch(Avatar.Commander.Signals.Float(floatSignalKey), multiplier);

        internal virtual void Tick(TimeSpan elapsed)
        {
            for (int i = 0; i < Triggers.Count; i++)
            {
                Triggers[i]();
            }

            Source.Volume = VolumeFactory(elapsed) * TriggerVolume;
            Source.Pitch = PitchFactory(elapsed) * TriggerPitch;
        }
    }

    public class SoundSourceCommand : SoundSourceCommand<SoundSourceCommand>
    {
        public static SoundSourceCommand Empty(ScriptAvatar avatar, string key) => new(avatar, key, ISoundSource.Empty);


        public SoundSourceCommand(ScriptAvatar avatar, string key, ISoundSource source)
            : base(avatar, key, source)
        {
        }
    }
}
