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
    public class Avatar
    {
        private readonly ScriptWorld World;

        internal Avatar(ScriptWorld world)
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

        public void Locate(int plateX, int plateZ, Pose pose)
        {
            if (World.Avatar is null)
            {
                ScriptError error = new(ErrorLevel.Error, "アバターが設定されていません。");
                World.ErrorCollector.Report(error);
                return;
            }

            World.Avatar.TeleportTo(plateX, plateZ, pose);
        }

        public void Locate(int plateX, int plateZ, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            Locate(plateX, plateZ, position.ToPose());
        }

        public void Locate(int plateX, int plateZ, double x, double y, double z)
        {
            Locate(plateX, plateZ, x, y, z, 0, 0, 0);
        }
    }
}
