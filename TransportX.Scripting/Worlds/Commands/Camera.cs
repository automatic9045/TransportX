using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Cameras;
using TransportX.Spatial;

namespace TransportX.Scripting.Worlds.Commands
{
    public class Camera
    {
        private readonly ScriptWorld World;

        public Camera(ScriptWorld world)
        {
            World = world;
            Locate(new WorldPose(ChunkIndex.Zero, new Pose(Chunk.Size / 2, 10, Chunk.Size / 2)));
        }

        public void Locate(WorldPose worldPose)
        {
            CameraPose cameraPose = CameraPose.FromWorldPose(worldPose);
            World.Camera.Viewpoints.Free.Locate(cameraPose);
        }

        public void Locate(int chunkX, int chunkZ, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            ChunkIndex chunkIndex = new(chunkX, chunkZ);
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            WorldPose worldPose = new(chunkIndex, position.ToPose());
            Locate(worldPose);
        }
    }
}
