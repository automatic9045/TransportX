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
        private static readonly string FilePath;

        static Config()
        {
            Process process = Process.GetCurrentProcess();
            FilePath = Path.Combine(Path.GetDirectoryName(process.MainModule!.FileName)!, "TransportX.Config.xml");
        }


        public int ShadowResolution
        {
            get => field;
            set
            {
                if (value < 0) throw new InvalidOperationException($"{nameof(ShadowResolution)} は 0 (影を描画しない) または 1 以上である必要があります。");
                field = value;
            }
        } = 1024;

        public static Config Import(IErrorCollector errorCollector)
        {
            if (!File.Exists(FilePath))
            {
                XmlSerializer<Config>.ToXml(new Config(), FilePath);
            }

            Config settings = XmlSerializer<Config>.FromXml(FilePath, errorCollector) ?? new Config();

            return settings;
        }
    }
}
