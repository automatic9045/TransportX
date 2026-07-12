using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TransportX.Audio;
using TransportX.Collections;
using TransportX.Diagnostics;
using TransportX.Rendering.Backend;

using TransportX.Scripting.Collections;

namespace TransportX.Scripting.Commands
{
    internal class SoundsInternal
    {
        private readonly IDXHost DXHost;
        private readonly ISoundCollection Sounds;
        private readonly IErrorCollector ErrorCollector;

        public SoundsInternal(IDXHost dxHost, ISoundCollection sounds, IErrorCollector errorCollector)
        {
            DXHost = dxHost;
            Sounds = sounds;
            ErrorCollector = errorCollector;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public SoundBundle LoadList(string key, string path, string defaultBaseDirectory)
        {
            using AssetList list = new(path, defaultBaseDirectory, "サウンドリスト", [2], ErrorCollector);
            if (!list.IsValid) return SoundBundle.Empty(key);

            try
            {
                ScriptDictionary<string, ISoundAsset> sounds = new(ErrorCollector, "サウンド", key => ISoundAsset.Empty);

                while (list.ReadLine(out string[] line))
                {
                    try
                    {
                        string soundKey = line[0];
                        string soundPath = Path.Combine(list.ListDirectory, line[1]);

                        SoundAsset sound = SoundAsset.FromFile(DXHost.XAudio2, DXHost.MasteringVoice, DXHost.X3DAudio, soundPath);
                        sounds.Add(soundKey, sound);
                    }
                    catch (Exception ex)
                    {
                        Error error = new(ErrorLevel.Error, $"レコード '{line[0]}' は無効です。", list.ListPath)
                        {
                            LineNumber = list.LineNumber,
                            Exception = ex,
                        };
                        ErrorCollector.Report(error);
                    }
                }

                SoundBundle bundle = new(key, sounds);
                if (Sounds.AdoptBundle(bundle))
                {
                    return bundle;
                }
                else
                {
                    bundle.Dispose();
                    return SoundBundle.Empty(key);
                }
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex, $"サウンドリスト '{list.ListPath}' を読み込めませんでした。");
                ErrorCollector.Report(error);
                return SoundBundle.Empty(key);
            }
        }
    }
}
