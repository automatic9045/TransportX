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

        private Action OnTick = () => { };

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
            OnTick = () =>
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
            OnTick = () =>
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

        public Sound3DCommand Loop(Func<(float Volume, float Pitch)> volumePitchFactory)
        {
            OnTick = () =>
            {
                (Source.Volume, Source.Pitch) = volumePitchFactory();
                if (!Source.IsPlaying) Source.Play(true);
            };
            return this;
        }

        public Sound3DCommand Loop(Signal<float> volumeSignal, float volumeMultiplier, Signal<float> pitchSignal, float pitchMultiplier)
            => Loop(() => (volumeSignal.Value * volumeMultiplier, pitchSignal.Value * pitchMultiplier));
        public Sound3DCommand Loop(string volumeFloatSignalKey, float volumeMultiplier, string pitchFloatSignalKey, float pitchMultiplier)
            => Loop(Avatar.Commander.Signals.Float(volumeFloatSignalKey), volumeMultiplier, Avatar.Commander.Signals.Float(pitchFloatSignalKey), pitchMultiplier);

        internal void Tick(TimeSpan elapsed)
        {
            OnTick();
            Source.Update(Avatar.Camera.Listener, Avatar.Camera.WorldPose.Chunk);
        }
    }
}
