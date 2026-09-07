using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Data
{
    public class Config
    {
        internal static readonly string BaseDirectory;
        internal static readonly string FilePath;

        static Config()
        {
            Process process = Process.GetCurrentProcess();
            BaseDirectory = Path.Combine(Path.GetDirectoryName(process.MainModule!.FileName)!, "Config");
            FilePath = Path.Combine(BaseDirectory, "Config.xml");
        }


        public int SimulationChunkCount
        {
            get;
            set
            {
                if (value < 1) throw new InvalidOperationException($"{nameof(SimulationChunkCount)} は 1 以上である必要があります。");
                field = value;
            }
        } = 3;

        public int DrawChunkCount
        {
            get;
            set
            {
                if (value < 1) throw new InvalidOperationException($"{nameof(DrawChunkCount)} は 1 以上である必要があります。");
                field = value;
            }
        } = 3;

        public int ShadowDrawChunkCount
        {
            get;
            set
            {
                if (value < 1) throw new InvalidOperationException($"{nameof(ShadowDrawChunkCount)} は 1 以上である必要があります。");
                field = value;
            }
        } = 2;

        public int ShadowResolution
        {
            get;
            set
            {
                if (value < 0) throw new InvalidOperationException($"{nameof(ShadowResolution)} は 0 (影を描画しない) または 1 以上である必要があります。");
                field = value;
            }
        } = 1024;

        public bool IsDebugMode { get; set; }

        public InputConfig Input { get; set; } = new();

        public static Config Import(IErrorCollector errorCollector)
        {
            if (!File.Exists(FilePath))
            {
                XmlSerializer<Config>.ToXml(new Config(), FilePath);
            }

            Config data = XmlSerializer<Config>.FromXml(FilePath, errorCollector) ?? new Config();
            return data;
        }
    }
}
