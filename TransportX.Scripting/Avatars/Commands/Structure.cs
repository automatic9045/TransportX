using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Scripting.Collections;
using TransportX.Spatial;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Structure
    {
        private readonly ScriptAvatar Avatar;

        public Parts Parts { get; }
        public Joints Joints { get; }

        internal Structure(ScriptAvatar avatar)
        {
            Avatar = avatar;

            Parts = new Parts(Avatar);
            Joints = new Joints(Avatar);
        }

        internal void Dispose()
        {
            Joints.Dispose();
        }

        internal void RegisterComponents()
        {
            Parts.RegisterComponents();
            Joints.RegisterComponents();
        }
    }
}
