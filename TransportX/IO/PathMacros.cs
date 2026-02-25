using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.IO
{
    public static class PathMacros
    {
        private static readonly string Bin;
        private static readonly string Plugins;

        static PathMacros()
        {
            Bin = AddSeparator(Path.GetDirectoryName(typeof(PathMacros).Assembly.Location)!);
            Plugins = AddSeparator(Path.Combine(Bin, "Plugins"));


            static string AddSeparator(string path)
            {
                return Path.EndsInDirectorySeparator(path)
                    ? path
                    : path + Path.DirectorySeparatorChar;
            }
        }

        public static string Expand(string pathWithMacros)
        {
            return pathWithMacros
                .Replace("$(bin)", Bin, StringComparison.OrdinalIgnoreCase)
                .Replace("$(plugins)", Plugins, StringComparison.OrdinalIgnoreCase);
        }
    }
}
