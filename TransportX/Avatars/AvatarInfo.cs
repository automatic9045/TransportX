using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using TransportX.IO;

namespace TransportX.Avatars
{
    public class AvatarInfo : IAvatarInfo
    {
        private static readonly XmlSerializer Serializer = new(typeof(AvatarInfo));


        public string Title { get; set; } = "無題のアバター";
        public string Description { get; set; } = "(説明がありません)";
        public string Author { get; set; } = string.Empty;

        public string Path
        {
            get => field;
            set => field = PathMacros.Expand(value);
        } = string.Empty;

        public string? Identifier { get; set; } = null;

        [XmlArrayItem("Arg")]
        public string[] Args { get; set; } = [];

        [XmlIgnore]
        public string InfoPath { get; private set; } = string.Empty;
        [XmlIgnore]
        IReadOnlyList<string> IAvatarInfo.Args => Args;

        public void Serialize(string path)
        {
            using StreamWriter sw = new(path, false, Encoding.UTF8);
            Serializer.Serialize(sw, this);
        }

        public static AvatarInfo Deserialize(string path, bool generateIfNotExists)
        {
            if (!File.Exists(path) && generateIfNotExists)
            {
                AvatarInfo empty = new();
                empty.Serialize(path);
            }

            using StreamReader sr = new(path, Encoding.UTF8);
            AvatarInfo result = (AvatarInfo)Serializer.Deserialize(sr)!;
            result.InfoPath = path;
            return result;
        }
    }
}
