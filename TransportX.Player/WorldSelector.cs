using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NativeFileDialogs.Net;

using TransportX.Worlds;

namespace TransportX.Player
{
    internal static class WorldSelector
    {
        public static IWorldInfo? Select()
        {
            Dictionary<string, string> filter = new()
            {
                { "ワールド情報ファイル", "xml" },
                { "すべてのファイル", "*" },
            };
            string defaultDirectory = Path.GetDirectoryName(typeof(WorldSelector).Assembly.Location)!;
            NfdStatus status = Nfd.OpenDialog(out string? outPath, filter, defaultDirectory);

            if (status != NfdStatus.Ok || outPath is null) return null;

            WorldInfo worldInfo = WorldInfo.Deserialize(outPath, false);
            return worldInfo;
        }
    }
}
