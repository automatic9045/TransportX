using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Rendering.Importing
{
    internal static class TextureDiagnostics
    {
        public static void ReportIfNpot(int width, int height, IErrorCollector errorCollector)
        {
            if (!IsPowerOfTwo(width) || !IsPowerOfTwo(height))
            {
                Error error = new(ErrorLevel.Warning, $"テクスチャサイズ ({width}x{height}) が 2 の累乗ではありません。パフォーマンスが低下する可能性があります。", null);
                errorCollector.Report(error);
            }


            static bool IsPowerOfTwo(int x) => 0 < x && (x & (x - 1)) == 0;
        }
    }
}
