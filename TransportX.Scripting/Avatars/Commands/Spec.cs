using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Spec
    {
        private readonly ScriptAvatar Avatar;

        internal Spec(ScriptAvatar avatar)
        {
            Avatar = avatar;
        }

        public void SetSize(double width, double height, double length)
        {
            Avatar.Width = (float)width;
            Avatar.Height = (float)height;
            Avatar.Length = (float)length;
        }
    }
}
