using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Cameras;
using TransportX.Spatial;

namespace TransportX.Data
{
    internal class Save
    {
        private static readonly Process Process;
        private static readonly string FilePath;

        static Save()
        {
            Process = Process.GetCurrentProcess();
            FilePath = Path.Combine(Path.GetDirectoryName(Process.MainModule!.FileName)!, "Save.dat");
        }


        public CameraPose? FreeViewpointPose { get; set; } = null;

        public Save()
        {
        }

        public static Save Import()
        {
            CameraPose? freeViewpointPose = null;
            try
            {
                string[] saveContent = File.ReadAllLines(FilePath);

                if (int.Parse(saveContent[0]) == Process.Id)
                {
                    string[] chunkText = saveContent[1].Split(',');
                    int chunkX = int.Parse(chunkText[0]);
                    int chunkZ = int.Parse(chunkText[1]);
                    ChunkIndex chunkIndex = new(chunkX, chunkZ);

                    string[] positionText = saveContent[2].Split(',');
                    Vector3 position = new(float.Parse(positionText[0]), float.Parse(positionText[1]), float.Parse(positionText[2]));

                    string[] angleText = saveContent[3].Split(',');
                    Vector2 angle = new(float.Parse(angleText[0]), float.Parse(angleText[1]));

                    freeViewpointPose = new CameraPose(chunkIndex, position, angle);
                }
            }
            catch { }

            return new Save()
            {
                FreeViewpointPose = freeViewpointPose,
            };
        }

        public void Export()
        {
            try
            {
                StringBuilder saveContentBuilder = new();
                saveContentBuilder.AppendLine(Process.Id.ToString(CultureInfo.InvariantCulture));

                if (FreeViewpointPose.HasValue)
                {
                    CameraPose pose = FreeViewpointPose.Value;
                    saveContentBuilder.AppendLine(FormattableString.Invariant($"{pose.Chunk.X},{pose.Chunk.Z}"));
                    saveContentBuilder.AppendLine(FormattableString.Invariant($"{pose.Position.X},{pose.Position.Y},{pose.Position.Z}"));
                    saveContentBuilder.AppendLine(FormattableString.Invariant($"{pose.Angle.X},{pose.Angle.Y}"));
                }

                File.WriteAllText(FilePath, saveContentBuilder.ToString());
            }
            catch { }
        }
    }
}
