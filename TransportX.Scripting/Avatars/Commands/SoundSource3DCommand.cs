using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;
using TransportX.Communication;

namespace TransportX.Scripting.Avatars.Commands
{
    public class SoundSource3DCommand : SoundSourceCommand<SoundSource3DCommand>
    {
        public static SoundSource3DCommand Empty(ScriptAvatar avatar, string key) => new(avatar, key, ISoundSource3D.Empty);


        public new ISoundSource3D Source => (ISoundSource3D)base.Source;

        public SoundSource3DCommand(ScriptAvatar avatar, string key, ISoundSource3D source) : base(avatar, key, source)
        {
        }

        internal override void Tick(TimeSpan elapsed)
        {
            base.Tick(elapsed);
            Source.Tick(Avatar.AudioClient.Listener, Avatar.Camera.WorldPose.Chunk, elapsed);
        }
    }
}
