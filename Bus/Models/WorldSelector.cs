using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Bus.Models
{
    internal static class WorldSelector
    {
        public static WorldInfo? Select()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                DefaultDirectory = Path.GetDirectoryName(typeof(GameLoader).Assembly.Location),
                DefaultExt = ".xml",
                Filter = "ワールド情報ファイル|*.xml|すべてのファイル|*.*",
            };
            if (dialog.ShowDialog() != true) return null;

            WorldInfo worldInfo = WorldInfo.Deserialize(dialog.FileName, false);
            return worldInfo;
        }
    }
}
