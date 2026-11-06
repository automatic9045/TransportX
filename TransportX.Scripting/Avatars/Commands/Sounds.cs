using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;

using TransportX.Scripting.Collections;
using TransportX.Scripting.Commands;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Sounds
    {
        private readonly ScriptAvatar Avatar;
        private readonly SoundsInternal Internal;

        private int DefaultKeyIndex = 0;

        private readonly ScriptKeyedList<string, SoundSource3DCommand> Source3DsKey;
        public IReadOnlyScriptKeyedList<string, SoundSource3DCommand> Source3Ds => Source3DsKey;

        internal Sounds(ScriptAvatar avatar)
        {
            Avatar = avatar;
            Internal = new SoundsInternal(Avatar.AudioHost, Avatar.Sounds, Avatar.ErrorCollector);

            Source3DsKey = new ScriptKeyedList<string, SoundSource3DCommand>(
                sound => sound.Key, Avatar.ErrorCollector, "3D サウンド", key => SoundSource3DCommand.Empty(avatar, key));
        }

        internal void Dispose()
        {
            foreach (SoundSource3DCommand command in Source3Ds)
            {
                command.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ISoundBundle LoadList(string key, string path)
        {
            return Internal.LoadList(key, path, Avatar.BaseDirectory);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ISoundBundle LoadList(string listPath)
        {
            string key;
            do
            {
                key = FormattableString.Invariant($"Avatar_{DefaultKeyIndex++}");
            }
            while (Avatar.Sounds.Bundles.Contains(key));

            return Internal.LoadList(key, listPath, Avatar.BaseDirectory);
        }

        public SoundSource3DCommand CreateSource3D(string key, Pose pose, double maxFrequencyRatio)
        {
            ISoundSource3D source3D = Avatar.AudioHost.CreateSoundSource3D((float)maxFrequencyRatio);
            SoundSource3DCommand command = new(Avatar, key, source3D);
            Source3DsKey.Add(command);

            if (Source3Ds[key] == command)
            {
                source3D.AttachedTo = new AttachableObject(Avatar, pose);
                return command;
            }
            else
            {
                command.Dispose();
                return SoundSource3DCommand.Empty(Avatar, key);
            }
        }

        public SoundSource3DCommand CreateSource3D(string key, double x, double y, double z, double rotationX, double rotationY, double rotationZ, double maxFrequencyRatio)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return CreateSource3D(key, position.ToPose(), maxFrequencyRatio);
        }

        public SoundSource3DCommand CreateSource3D(string key, double x, double y, double z, double maxFrequencyRatio)
            => CreateSource3D(key, x, y, z, 0, 0, 0, maxFrequencyRatio);

        internal void Tick(TimeSpan elapsed)
        {
            for (int i = 0; i < Source3Ds.Count; i++)
            {
                Source3Ds[i].Tick(elapsed);
            }
        }
    }
}
