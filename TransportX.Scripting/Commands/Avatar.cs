using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Spatial;

namespace TransportX.Scripting.Commands
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

        public void Locate(WorldPose worldPose)
        {
            if (World.Avatar is null)
            {
                ScriptError error = new(ErrorLevel.Error, "アバターが設定されていません。");
                World.ErrorCollector.Report(error);
                return;
            }

            World.Avatar.TeleportTo(worldPose);
        }

        public void Locate(int chunkX, int chunkZ, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            WorldPose worldPose = new(chunkX, chunkZ, position.ToPose());
            Locate(worldPose);
        }

        public void Locate(int chunkX, int chunkZ, double x, double y, double z)
        {
            Locate(chunkX, chunkZ, x, y, z, 0, 0, 0);
        }
    }
}
