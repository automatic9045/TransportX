using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using TransportX.Avatars;
using TransportX.Diagnostics;
using TransportX.Spatial;

namespace TransportX.Scripting.Worlds.Commands
{
    public class Avatar
    {
        private readonly ScriptWorld World;

        internal Avatar(ScriptWorld world)
        {
            World = world;
        }

        public void Load(string path)
        {
            if (Path.GetExtension(path).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                ScriptError error = new(ErrorLevel.Error, "アバターの dll ファイルを直接指定することはできません。xml 形式のアバター情報ファイルを指定してください。");
                World.ErrorCollector.Report(error);
                return;
            }

            try
            {
                string absolutePath = Path.Combine(BaseDirectory.Find() ?? World.BaseDirectory, path);
                IAvatarInfo info = AvatarInfo.Deserialize(absolutePath, false);
                World.Avatar = World.CreateAvatar(info);
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
            ChunkIndex chunkIndex = new(chunkX, chunkZ);
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            WorldPose worldPose = new(chunkIndex, position.ToPose());
            Locate(worldPose);
        }

        public void Locate(int chunkX, int chunkZ, double x, double y, double z)
        {
            Locate(chunkX, chunkZ, x, y, z, 0, 0, 0);
        }
    }
}
