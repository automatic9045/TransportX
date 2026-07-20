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

        private readonly ScriptKeyedList<string, Sound3DCommand> Sound3DsKey;
        public IReadOnlyScriptKeyedList<string, Sound3DCommand> Sound3Ds => Sound3DsKey;

        internal Sounds(ScriptAvatar avatar)
        {
            Avatar = avatar;
            Internal = new SoundsInternal(Avatar.DXHost, Avatar.Sounds, Avatar.ErrorCollector);

            Sound3DsKey = new ScriptKeyedList<string, Sound3DCommand>(
                sound => sound.Key, Avatar.ErrorCollector, "3D サウンド", key => Sound3DCommand.Empty(avatar, key));
        }

        internal void Dispose()
        {
            foreach (Sound3DCommand command in Sound3Ds)
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

        public Sound3DCommand Create3D(string key, string assetKey, Pose pose, double maxFrequencyRatio)
        {
            if (!Avatar.SoundsKey.GetSound(assetKey, out ISoundAsset asset)) return Sound3DCommand.Empty(Avatar, key);

            ISound3D sound3D = asset.CreateSound3D((float)maxFrequencyRatio);
            sound3D.Volume = 1;
            sound3D.Pitch = 1;

            Sound3DCommand command = new(Avatar, key, sound3D);
            Sound3DsKey.Add(command);

            if (Sound3Ds[key] == command)
            {
                sound3D.AttachedTo = new AttachableObject(Avatar, pose);
                return command;
            }
            else
            {
                command.Dispose();
                return Sound3DCommand.Empty(Avatar, key);
            }
        }

        public Sound3DCommand Create3D(string key, string assetKey,
            double x, double y, double z, double rotationX, double rotationY, double rotationZ, double maxFrequencyRatio)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return Create3D(key, assetKey, position.ToPose(), maxFrequencyRatio);
        }

        public Sound3DCommand Create3D(string key, string assetKey, double x, double y, double z, double maxFrequencyRatio)
            => Create3D(key, assetKey, x, y, z, 0, 0, 0, maxFrequencyRatio);

        internal void Tick(TimeSpan elapsed)
        {
            for (int i = 0; i < Sound3Ds.Count; i++)
            {
                Sound3Ds[i].Tick(elapsed);
            }
        }
    }
}
