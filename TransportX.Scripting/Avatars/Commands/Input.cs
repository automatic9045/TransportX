using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input.Configuration;

using TransportX.Scripting.Commands;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Input : InputBase<Input>
    {
        public ScriptAvatar Avatar { get; }
        public override InputProfile Profile
        {
            get => Avatar.InputProfile;
            set => Avatar.InputProfile = value;
        }

        public Input(ScriptAvatar avatar, Signals signals) : base(avatar.InputClient, signals, avatar.ErrorCollector)
        {
            Avatar = avatar;
        }

        public override ButtonFactory<Input> AddButton(string key)
        {
            ButtonFactory<Input> buttonFactory = new(this, key);
            return buttonFactory;
        }

        public override AxisFactory<Input> AddAxis(string key, double min, double neutral, double max)
        {
            AxisFactory<Input> axisFactory = new(this, Avatar, key, (float)min, (float)neutral, (float)max);
            return axisFactory;
        }
    }
}
