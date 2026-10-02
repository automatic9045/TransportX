using System;
using System.Collections.Generic;
using System.Text;

using TransportX.Audio;
using TransportX.Communication;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Sound3DCommand
    {
        public static Sound3DCommand Empty(ScriptAvatar avatar, string key) => new(avatar, key, ISound3D.Empty);


        private readonly ScriptAvatar Avatar;

        private Func<TimeSpan, float> VolumeFactory = _ => 1;
        private Func<TimeSpan, float> PitchFactory = _ => 1;
        private Action PlayOrStopAction = () => { };

        public string Key { get; }
        public ISound3D Source { get; }

        public Sound3DCommand(ScriptAvatar avatar, string key, ISound3D source)
        {
            Avatar = avatar;

            Key = key;
            Source = source;
        }

        internal void Dispose()
        {
            Source.Dispose();
        }

        public Sound3DCommand PlayWhen(Func<int> countFactory)
        {
            int lastCount = countFactory();
            PlayOrStopAction = () =>
            {
                int count = countFactory();
                if (count != lastCount)
                {
                    lastCount = count;
                    Source.Play(false);
                }
            };
            return this;
        }

        public Sound3DCommand PlayWhen(Signal<int> countSignal) => PlayWhen(() => countSignal.Value);
        public Sound3DCommand PlayWhen(string countIntSignalKey) => PlayWhen(Avatar.Commander.Signals.Int(countIntSignalKey));

        public Sound3DCommand PlayStopWhen(Func<int> playCountFactory, Func<int> stopCountFactory)
        {
            int lastPlayCount = playCountFactory();
            int lastStopCount = stopCountFactory();
            PlayOrStopAction = () =>
            {
                int playCount = playCountFactory();
                if (playCount != lastPlayCount)
                {
                    lastPlayCount = playCount;
                    Source.Play(false);
                }

                int stopCount = stopCountFactory();
                if (stopCount != lastStopCount)
                {
                    lastStopCount = stopCount;
                    Source.Stop();
                }
            };
            return this;
        }

        public Sound3DCommand PlayStopWhen(Signal<int> playCountSignal, Signal<int> stopCountSignal)
            => PlayStopWhen(() => playCountSignal.Value, () => stopCountSignal.Value);
        public Sound3DCommand PlayStopWhen(string playCountIntSignalKey, string stopCountIntSignalKey)
            => PlayStopWhen(Avatar.Commander.Signals.Int(playCountIntSignalKey), Avatar.Commander.Signals.Int(stopCountIntSignalKey));

        public Sound3DCommand Loop()
        {
            PlayOrStopAction = () =>
            {
                if (!Source.IsPlaying) Source.Play(true);
            };
            return this;
        }

        public Sound3DCommand Volume(Func<TimeSpan, float> factory)
        {
            VolumeFactory = factory;
            return this;
        }

        public Sound3DCommand Volume(double volume)
        {
            float floatVolume = (float)volume;
            return Volume(_ => floatVolume);
        }

        public Sound3DCommand Volume(Func<TimeSpan, double> factory)
            => Volume(elapsed => (float)factory(elapsed));
        public Sound3DCommand Volume(Signal<float> signal, float multiplier)
            => Volume(_ => signal.Value * multiplier);
        public Sound3DCommand Volume(string floatSignalKey, float multiplier)
            => Volume(Avatar.Commander.Signals.Float(floatSignalKey), multiplier);

        public Sound3DCommand Pitch(Func<TimeSpan, float> factory)
        {
            PitchFactory = factory;
            return this;
        }

        public Sound3DCommand Pitch(double pitch)
        {
            float floatPitch = (float)pitch;
            return Pitch(_ => floatPitch);
        }

        public Sound3DCommand Pitch(Func<TimeSpan, double> factory)
            => Pitch(elapsed => (float)factory(elapsed));
        public Sound3DCommand Pitch(Signal<float> signal, float multiplier)
            => Pitch(_ => signal.Value * multiplier);
        public Sound3DCommand Pitch(string floatSignalKey, float multiplier)
            => Pitch(Avatar.Commander.Signals.Float(floatSignalKey), multiplier);

        internal void Tick(TimeSpan elapsed)
        {
            Source.Volume = VolumeFactory(elapsed);
            Source.Pitch = PitchFactory(elapsed);
            PlayOrStopAction();
            Source.Tick(Avatar.AudioClient.Listener, Avatar.Camera.WorldPose.Chunk, elapsed);
        }
    }
}
