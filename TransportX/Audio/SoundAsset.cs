using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Vortice.Multimedia;
using Vortice.XAudio2;

namespace TransportX.Audio
{
    public class SoundAsset : ISoundAsset
    {
        private readonly byte[] AudioBytes;

        public WaveFormat Format { get; }
        public TimeSpan Duration { get; }
        public AudioBuffer Buffer { get; }
        public uint[]? DecodedPacketsInfo { get; }

        public SoundAsset(SoundStream soundStream)
        {
            WaveFormat sourceFormat = soundStream.Format ?? throw new InvalidOperationException("音声データのフォーマットを取得できませんでした。");
            DecodedPacketsInfo = soundStream.DecodedPacketsInfo;

            int rawLength = (int)soundStream.Length;
            byte[] rawBytes = new byte[rawLength];
            soundStream.ReadExactly(rawBytes, 0, rawLength);

            if (sourceFormat.Encoding == WaveFormatEncoding.Pcm && sourceFormat.BitsPerSample == 16)
            {
                ReadOnlySpan<short> sourceSamples = MemoryMarshal.Cast<byte, short>(rawBytes);
                float[] floatSamples = new float[sourceSamples.Length];

                for (int i = 0; i < sourceSamples.Length; i++)
                {
                    floatSamples[i] = sourceSamples[i] / 32768f;
                }

                AudioBytes = MemoryMarshal.AsBytes<float>(floatSamples).ToArray();
                Format = WaveFormat.CreateIeeeFloatWaveFormat(sourceFormat.SampleRate, sourceFormat.Channels);
            }
            else if (sourceFormat.Encoding == WaveFormatEncoding.IeeeFloat && sourceFormat.BitsPerSample == 32)
            {
                AudioBytes = rawBytes;
                Format = sourceFormat;
            }
            else
            {
                throw new NotSupportedException($"未対応のフォーマットです: {sourceFormat.Encoding}, {sourceFormat.BitsPerSample}bit");
            }

            Buffer = new AudioBuffer(AudioBytes)
            {
                AudioBytes = (uint)AudioBytes.Length,
                LoopCount = XAudio2.NoLoopRegion,
                Flags = BufferFlags.EndOfStream,
            };

            double totalSeconds = (double)AudioBytes.Length / Format.AverageBytesPerSecond;
            Duration = TimeSpan.FromSeconds(totalSeconds);
        }

        public static SoundAsset Empty()
        {
            using SoundStream soundStream = new(Stream.Null);
            return new SoundAsset(soundStream);
        }

        public static SoundAsset FromFile(string filePath)
        {
            using FileStream fileStream = File.OpenRead(filePath);
            using SoundStream soundStream = new(fileStream);
            return new SoundAsset(soundStream);
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
