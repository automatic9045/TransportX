using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Bus.Common.Worlds;

namespace Bus.Models
{
    public sealed class WorldInfo : IWorldInfo
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(WorldInfo));


        public string Title { get; set; } = "無題のワールド";
        public string Description { get; set; } = "(説明がありません)";
        public string Author { get; set; } = string.Empty;

        private string PathKey = string.Empty;
        public string Path
        {
            get => PathKey;
            set => PathKey = ExpandMacros(value);
        }

        public string? Identifier { get; set; } = null;

        private string GamePathKey = typeof(IWorldInfo).Assembly.Location;
        public string GamePath
        {
            get => GamePathKey;
            set => GamePathKey = ExpandMacros(value);
        }

        [XmlArrayItem("Arg")]
        public string[] Args { get; set; } = [];

        [XmlIgnore]
        public string InfoPath { get; private set; } = string.Empty;
        [XmlIgnore]
        IReadOnlyList<string> IWorldInfo.Args => Args;

        private string ExpandMacros(string pathWithMacros)
        {
            string binDirectory = System.IO.Path.GetDirectoryName(typeof(WorldInfo).Assembly.Location)!;
            return pathWithMacros.Replace("$(bin)", binDirectory);
        }

        internal void Serialize(string path)
        {
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                Serializer.Serialize(sw, this);
            }
        }

        internal static WorldInfo Deserialize(string path, bool generateIfNotExists)
        {
            if (!File.Exists(path) && generateIfNotExists)
            {
                WorldInfo empty = new WorldInfo();
                empty.Serialize(path);
            }

            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                WorldInfo result = (WorldInfo)Serializer.Deserialize(sr)!;
                result.InfoPath = path;
                return result;
            }
        }
    }
}
