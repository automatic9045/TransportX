using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Debug
    {
        private readonly ScriptAvatar Avatar;

        internal Debug(ScriptAvatar avatar)
        {
            Avatar = avatar;
        }

        public void ShowDialog(string message)
        {
            MessageBox.Show(message);
        }
    }
}
