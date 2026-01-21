using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;

namespace Bus.Common.Scripting.Commands
{
    public class Avatars
    {
        private readonly ScriptWorld World;

        internal Avatars(ScriptWorld world)
        {
            World = world;
        }

        public void Load(string path, string? identifier = null)
        {
            string avatarPath = Path.Combine(World.BaseDirectory, path);
            try
            {
                World.Avatar = World.CreateAvatar(avatarPath, identifier);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException ti && ti.InnerException is not null) ex = ti.InnerException;

                ScriptError error = new(ErrorLevel.Error, ex);
                World.ErrorCollector.Report(error);
            }
        }
    }
}
